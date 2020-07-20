CREATE PROCEDURE [Enterprise].[AddUpdatePropertyInstanceMapping] (
	@PersonaId bigint,
	@ProductId int,
	@PropertyInstanceJSON varchar(max)
 )
AS
BEGIN
	DECLARE @Now datetime = GETUTCDATE()

	BEGIN TRY
		IF (ISJSON(@PropertyInstanceJSON) = 1)
		BEGIN
			IF EXISTS(SELECT TOP 1 1 FROM OPENJSON(JSON_QUERY(@PropertyInstanceJSON, '$.InputJson.PropertyList')))
			BEGIN
				IF EXISTS(SELECT TOP 1 1 FROM OPENJSON(JSON_QUERY(@PropertyInstanceJSON, '$.InputJson.PropertyList')) WHERE [value] = -1)
				BEGIN
					--Unassign all individual properties
					UPDATE	Enterprise.PropertyInstanceMapping
					SET			ThruDate = @Now,
								Active = 0
					WHERE	PersonaId = @PersonaId
					AND			ProductId = @ProductId
					AND			ThruDate IS NULL

					--Assign All properties
					INSERT INTO Enterprise.PropertyInstanceMapping (
						PersonaId,
						PropertyInstanceId,
						ProductId
					)
					VALUES (
						@PersonaId,
						-1, --all
						@ProductId
					)
				END
				ELSE
				BEGIN
					--Unassign All (-1)
					UPDATE	Enterprise.PropertyInstanceMapping
					SET			ThruDate = @Now, Active = 0
					WHERE	PersonaId = @PersonaId
					AND			ProductId = @ProductId
					AND			PropertyInstanceId = -1
					
					--Assign
					INSERT INTO Enterprise.PropertyInstanceMapping (
						PersonaId,
						PropertyInstanceId,
						ProductId
					)
					SELECT	@PersonaId,
								ap.[value] AS 'PropertyInstanceId',
								@ProductId
					FROM	Enterprise.PropertyInstanceMapping epm
								RIGHT OUTER JOIN OPENJSON(JSON_QUERY(@PropertyInstanceJSON, '$.InputJson.PropertyList')) ap ON (epm.PropertyInstanceId = ap.[value] AND epm.PersonaId = @PersonaId AND epm.ProductId = @ProductId AND epm.ThruDate IS NULL)
					WHERE		epm.PropertyInstanceId IS NULL

					--Reassign.  Already unassigned (ThruDate Is NOT NULL)
					INSERT INTO Enterprise.PropertyInstanceMapping (
						PersonaId,
						PropertyInstanceId,
						ProductId
					)
					SELECT	@PersonaId,
								ap.[value] AS 'PropertyInstanceId',
								@ProductId
					FROM		OPENJSON(JSON_QUERY(@PropertyInstanceJSON, '$.InputJson.PropertyList')) ap
									LEFT OUTER JOIN Enterprise.PropertyInstanceMapping epm ON (epm.PropertyInstanceId = ap.[value] AND epm.PersonaId = @PersonaId AND epm.ProductId = @ProductId AND epm.ThruDate IS NULL)
					WHERE		epm.PropertyInstanceId IS NULL
				END
			END

			--Remove
			UPDATE	epm
			SET			epm.ThruDate = @Now, Active = 0
			FROM		Enterprise.PropertyInstanceMapping epm
							INNER JOIN OPENJSON(JSON_QUERY(@PropertyInstanceJSON, '$.InputJson.RemovedPropertyList')) rp ON (epm.PropertyInstanceId = rp.[value])
			WHERE	epm.PersonaId = @PersonaId
			AND			epm.ProductId = @ProductId
			AND			epm.ThruDate IS NULL
			AND			epm.Active = 1

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