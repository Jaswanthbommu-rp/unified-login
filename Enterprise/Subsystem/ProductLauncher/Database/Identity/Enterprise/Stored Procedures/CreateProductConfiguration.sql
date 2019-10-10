CREATE PROCEDURE [Enterprise].[CreateProductConfiguration] (
    @ConfigurationId INT OUTPUT
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION;
		INSERT INTO Enterprise.Configuration (
			CreateDate
		)
		OUTPUT	Inserted.ConfigurationId AS Id,
				'' AS ErrorMessage
		VALUES (
			GETUTCDATE()
		);

		SET @ConfigurationId = SCOPE_IDENTITY();
		COMMIT;
	END TRY  
	BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END;