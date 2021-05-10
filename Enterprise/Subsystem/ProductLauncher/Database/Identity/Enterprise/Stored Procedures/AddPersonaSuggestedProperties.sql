CREATE PROCEDURE [Enterprise].[AddPersonaSuggestedProperties] (
    @ProductId int,
	@PersonaId bigint,
	@PropertyInstanceJSON nvarchar(max),
    @ModifiedBy bigint
 )
AS
BEGIN
	DECLARE @CreateDate datetime2 = GETUTCDATE();
	DECLARE @ModifiedDate datetime2 = GETUTCDATE();
	BEGIN TRY  
		INSERT INTO [Enterprise].[PersonaSuggestedProperties] (
			 [ProductId]
			,[PersonaId]
			,[PropertyInstanceId]
			,[ProductPropertyId]
			,[ModifiedBy]
			,[ModifiedDate]
			,[CreateDate]
		)

		SELECT @ProductId,@PersonaId, pa.PropertyInstanceId, JSON1.productPropertyId, @ModifiedBy,@ModifiedDate, @CreateDate
		FROM Enterprise.PropertyInstance pa
				INNER JOIN 
			(SELECT productPropertyId, propertyInstanceId  FROM
				OPENJSON (@PropertyInstanceJSON)  
				WITH (
				           productPropertyId bigint '$.productPropertyId',
				           propertyInstanceId varchar(max) '$.propertyInstanceId'
				 )
			 )
			 AS JSON1 ON PA.InstanceId = JSON1.propertyInstanceId 
		SELECT	1 AS Id
	END TRY
	BEGIN CATCH
		DECLARE @ErrorLogID int;
		EXEC dbo.LogError
			@ErrorLogID = @ErrorLogID OUTPUT;
		SELECT	0 AS Id,
						ErrorMessage
		FROM		dbo.ErrorLog
		WHERE	ErrorLogID = @ErrorLogID;
	END CATCH;
END
