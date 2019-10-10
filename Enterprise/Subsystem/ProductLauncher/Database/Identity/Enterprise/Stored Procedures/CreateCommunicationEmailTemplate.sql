CREATE PROCEDURE [Enterprise].[CreateCommunicationEmailTemplate] (
	@CommunicationEventAudienceTypeId INT,
	@CommunicationEventPurposeTypeID INT,
	@Subject NVARCHAR(255),
	@Body NVARCHAR(MAX),
	@CommunicationEmailTemplateId INT OUTPUT)
AS
BEGIN
	SET @CommunicationEmailTemplateId = NULL
	BEGIN TRY
		SELECT @CommunicationEmailTemplateId = CommunicationEmailTemplateId 
		FROM Enterprise.CommunicationEmailTemplate 
		WHERE Body = @Body 
			AND [Subject] = @Subject
			AND CommunicationEventAudienceTypeId = @CommunicationEventAudienceTypeId
			AND CommunicationEventPurposeTypeId = @CommunicationEventPurposeTypeID

		IF @CommunicationEmailTemplateId IS NULL
		BEGIN 
			INSERT INTO Enterprise.CommunicationEmailTemplate 
				(CommunicationEventAudienceTypeId, CommunicationEventPurposeTypeId, Subject, Body)
			VALUES (@CommunicationEventAudienceTypeId, @CommunicationEventPurposeTypeID, @Subject, @Body)

			SET @CommunicationEmailTemplateId = SCOPE_IDENTITY();
		END
		
		SELECT @CommunicationEmailTemplateId as Id, '' AS ErrorMessage
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
