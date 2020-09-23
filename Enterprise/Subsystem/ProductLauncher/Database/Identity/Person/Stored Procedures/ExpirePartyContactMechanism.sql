CREATE PROCEDURE [Person].[ExpirePartyContactMechanism]
    @RealPageId UNIQUEIDENTIFIER,
	@PartyContactMechanismId BIGINT = NULL
AS
    BEGIN TRY
        BEGIN TRANSACTION;
        UPDATE Enterprise.PartyContactMechanism
        SET    ThruDate = GETUTCDATE()
        OUTPUT Inserted.PartyContactMechanismId AS Id ,
               @RealPageId AS RealPageId ,
               '' AS ErrorMessage
        WHERE  PartyContactMechanismId = @PartyContactMechanismId;

		--Delete preference if it is selected as preferred contact
		Declare @ContactMechanismId int
		select @ContactMechanismId = ContactMechanismId
		FROM Enterprise.PartyContactMechanism
		WHERE  PartyContactMechanismId = @PartyContactMechanismId;

		exec [Enterprise].[DeleteContactMechanismPreference] @ContactMechanismId

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT 0 AS Id ,
               @RealPageId AS RealPageId ,
               ErrorMessage
        FROM   dbo.ErrorLog
        WHERE  ErrorLogID = @ErrorLogID;
    END CATCH;

