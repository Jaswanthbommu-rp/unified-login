CREATE PROCEDURE [Enterprise].[CreateCommunicationEventPurposeUsage]
	@CommunicationEventID BIGINT,
	@CommunicationEventPurposeTypeID INT,
	@CommunicationEventPurposeUsageId BIGINT OUTPUT
AS
BEGIN
	BEGIN TRY
		BEGIN
			INSERT INTO Enterprise.CommunicationEventPurposeUsage (
				CommunicationEventID,
				CommunicationEventPurposeTypeID)
			VALUES (@CommunicationEventID,
				@CommunicationEventPurposeTypeId)

			SET @CommunicationEventPurposeUsageId = SCOPE_IDENTITY();
		END

		SELECT @CommunicationEventPurposeUsageId AS Id, '' AS ErrorMessage
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

