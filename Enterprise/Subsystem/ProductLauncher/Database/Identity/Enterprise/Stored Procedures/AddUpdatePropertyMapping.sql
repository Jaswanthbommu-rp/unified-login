CREATE PROCEDURE [Enterprise].[AddUpdatePropertyMapping] (
	@PersonaId bigint,
	@ProductId int,
	@PropertyJSON varchar(max)
 )
AS
BEGIN
	DECLARE @Now datetime = GETUTCDATE()

	BEGIN TRY
		IF (ISJSON(@PropertyJSON) = 1)
		BEGIN
			IF EXISTS(SELECT TOP 1 1 FROM OPENJSON(JSON_QUERY(@PropertyJSON, '$.InputJson.PropertyList')))
			BEGIN
				IF EXISTS(SELECT TOP 1 1 FROM OPENJSON(JSON_QUERY(@PropertyJSON, '$.InputJson.PropertyList')) WHERE [value] = -1)
				BEGIN
					--Unassign all individual properties
					UPDATE	Enterprise.PropertyMapping
					SET			ThruDate = @Now
					WHERE	PersonaId = @PersonaId
					AND			ProductId = @ProductId
					AND			ThruDate IS NULL

					--Assign All properties
					INSERT INTO Enterprise.PropertyMapping (
						PersonaId,
						PropertyId,
						ProductId,
						FromDate,
						ThruDate
					)
					VALUES (
						@PersonaId,
						-1, --all
						@ProductId,
						@Now,
						NULL
					)
				END
				ELSE
				BEGIN
					--Unassign All (-1)
					UPDATE	Enterprise.PropertyMapping
					SET			ThruDate = @Now
					WHERE	PersonaId = @PersonaId
					AND			ProductId = @ProductId
					AND			PropertyId = -1
					
					--Assign
					INSERT INTO Enterprise.PropertyMapping (
						PersonaId,
						PropertyId,
						ProductId,
						FromDate,
						ThruDate
					)
					SELECT	@PersonaId,
								ap.[value] AS 'PropertyId',
								@ProductId,
								@Now,
								NULL
					FROM	Enterprise.PropertyMapping epm
								RIGHT OUTER JOIN OPENJSON(JSON_QUERY(@PropertyJSON, '$.InputJson.PropertyList')) ap ON (epm.PropertyId = ap.[value] AND epm.PersonaId = @PersonaId AND epm.ProductId = @ProductId)
					WHERE		epm.PropertyId IS NULL

					--Reassign.  Already unassigned (ThruDate Is NOT NULL)
					INSERT INTO Enterprise.PropertyMapping (
						PersonaId,
						PropertyId,
						ProductId,
						FromDate,
						ThruDate
					)
					SELECT	@PersonaId,
								[value] AS 'PropertyId',
								@ProductId,
								@Now,
								NULL
					FROM		OPENJSON(JSON_QUERY(@PropertyJSON, '$.InputJson.PropertyList'))
					WHERE	[value] IN (SELECT PropertyId FROM Enterprise.PropertyMapping WHERE PersonaId = @PersonaId AND ProductId = @ProductId AND ThruDate IS NOT NULL)
				END
			END

			--Remove
			UPDATE	epm
			SET			epm.ThruDate = @Now
			FROM		Enterprise.PropertyMapping epm
							INNER JOIN OPENJSON(JSON_QUERY(@PropertyJSON, '$.InputJson.RemovedPropertyList')) rp ON (epm.PropertyId = rp.[value])
			WHERE	epm.PersonaId = @PersonaId
			AND			epm.ProductId = @ProductId
			AND			epm.ThruDate IS NULL

            SELECT	1 AS Id,
							'' AS ErrorMessage;
		END
	END TRY
	BEGIN CATCH
		DECLARE @ErrorLogID INT;

		EXEC dbo.LogError
			@ErrorLogID = @ErrorLogID OUTPUT;
	
		SELECT	0 AS Id,
						ErrorMessage
		FROM		dbo.ErrorLog
		WHERE	ErrorLogID = @ErrorLogID;
	END CATCH;
END