CREATE PROCEDURE [Enterprise].[CreateCommunicationEventPurposeType]
	@Description NVARCHAR(50),
	@CommunicationEventPurposeTypeID INT OUTPUT
AS
BEGIN
	SET @CommunicationEventPurposeTypeID = NULL

	SELECT @CommunicationEventPurposeTypeID = CommunicationEventPurposeTypeId FROM Enterprise.CommunicationEventPurposeType WHERE [Description] = @Description

	BEGIN TRY
		IF @Description IS NOT NULL AND @CommunicationEventPurposeTypeID IS NULL
		BEGIN
			INSERT INTO Enterprise.CommunicationEventPurposeType ([Description])
			VALUES (@Description)

			SET @CommunicationEventPurposeTypeID = SCOPE_IDENTITY();
		END
		
		SELECT @CommunicationEventPurposeTypeID as Id, '' AS ErrorMessage
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