CREATE PROCEDURE [Enterprise].[CreateCESCommunicationEvent]
	@CESId NVARCHAR(255),
	@CommunicationEventID BIGINT,
	@CESCommunicationEventID BIGINT OUTPUT
AS
BEGIN
	SET @CESCommunicationEventID = NULL

	BEGIN TRY
		INSERT INTO Enterprise.CESCommunicationEvent (
			CESId,
			CommunicationEventID)
		VALUES (@CESId, @CommunicationEventID)

		SET @CESCommunicationEventID = SCOPE_IDENTITY();

		SELECT @CESCommunicationEventID as Id, '' AS ErrorMessage
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

