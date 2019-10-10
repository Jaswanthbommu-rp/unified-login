CREATE PROCEDURE [Enterprise].[CreateCommunicationEventEmail]
	@CommunicationEmailTemplateID INT, 
	@CommunicationEventID BIGINT,
	@CommunicationEventEmailID BIGINT OUTPUT
AS
BEGIN
	DECLARE @Now DATETIME
	SET @Now = GETUTCDATE()
	SET @CommunicationEventEmailID = NULL
	BEGIN TRY
		INSERT INTO Enterprise.CommunicationEventEmail (
			CommunicationEmailTemplateID,
			CommunicationEventID)
		SELECT @CommunicationEmailTemplateID, @CommunicationEventID 
		FROM Enterprise.CommunicationEvent CE
			INNER JOIN Enterprise.PartyContactMechanism PCM
				ON CE.PartyContactMechanismIdTo = PCM.PartyContactMechanismID
				AND ((@Now BETWEEN PCM.FromDate AND PCM.ThruDate) OR
					 (@Now >= PCM.ThruDate AND PCM.ThruDate IS NULL))
			INNER JOIN Enterprise.ContactMechanismUsage CMU
				ON PCM.PartyContactMechanismID = CMU.PartyContactMechanismID
			INNER JOIN Enterprise.ContactMechanismUsageType CMUT
				ON CMU.ContactMechanismUsageTypeId = CMUT.ContactMechanismUsageTypeId
				AND (CMUT.ContactMechanismUsageTypeId = 300 OR CMUT.ParentContactMechanismUsageTypeId = 300)
			WHERE CE.CommunicationEventID = @CommunicationEventId

		IF @@ROWCOUNT > 0
		BEGIN
			SET @CommunicationEventEmailID = SCOPE_IDENTITY()
			SELECT @CommunicationEventEmailID as Id, '' AS ErrorMessage
		END
		ELSE
		BEGIN
			SELECT NULL AS Id, 'Email is not a valid communication method for this party.' as ErrorMessage
		END
	END TRY  
	BEGIN CATCH
        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
	END CATCH
END;