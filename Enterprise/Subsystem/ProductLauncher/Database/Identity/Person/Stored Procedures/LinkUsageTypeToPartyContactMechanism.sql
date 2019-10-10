CREATE PROCEDURE Person.LinkUsageTypeToPartyContactMechanism (
    @PartyContactMechanismId INT ,
    @ContactMechanismUsageTypeId INT = 1
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION; 

		INSERT  INTO Enterprise.ContactMechanismUsage (
			PartyContactMechanismID ,
			ContactMechanismUsageTypeID
		)
		OUTPUT	Inserted.ContactMechanismUsageID AS Id,
				'' AS ErrorMessage
		VALUES (
			@PartyContactMechanismId,
			@ContactMechanismUsageTypeId
		);

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