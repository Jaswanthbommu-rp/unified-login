CREATE PROCEDURE [Enterprise].[CreateCommunicationEventAudienceType]
	@Description NVARCHAR(50),
	@CommunicationEventAudienceTypeID INT OUTPUT
AS
BEGIN
	SET @CommunicationEventAudienceTypeID = NULL

	SELECT @CommunicationEventAudienceTypeID = CommunicationEventAudienceTypeId FROM Enterprise.CommunicationEventAudienceType WHERE [Description] = @Description

	BEGIN TRY
		IF @Description IS NOT NULL AND @CommunicationEventAudienceTypeID IS NULL
		BEGIN
			INSERT INTO Enterprise.CommunicationEventAudienceType ([Description])
			VALUES (@Description)

			SET @CommunicationEventAudienceTypeID = SCOPE_IDENTITY();
		END
		
		SELECT @CommunicationEventAudienceTypeID as Id, '' AS ErrorMessage
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