CREATE PROCEDURE Person.CreateContactMechanism
	@ContactMechanismId INT OUTPUT
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION; 

	    INSERT	INTO Enterprise.ContactMechanism
		OUTPUT	Inserted.ContactMechanismID AS Id,
				'' AS ErrorMessage
            DEFAULT VALUES;
		SELECT @ContactMechanismId = SCOPE_IDENTITY();
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
    END CATCH;
END;