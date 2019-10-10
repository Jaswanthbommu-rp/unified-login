CREATE PROCEDURE Person.UpdateContactMechanismUsageForParty (
    @PartyContactMechanismId INT ,
    @ContactMechanismUsageTypeId INT = 1
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION; 
        UPDATE  Enterprise.ContactMechanismUsage
        SET     ContactMechanismUsageTypeID = @ContactMechanismUsageTypeId
		OUTPUT	Inserted.ContactMechanismUsageID AS Id,
				'' AS ErrorMessage
        WHERE   PartyContactMechanismID = @PartyContactMechanismId; 
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