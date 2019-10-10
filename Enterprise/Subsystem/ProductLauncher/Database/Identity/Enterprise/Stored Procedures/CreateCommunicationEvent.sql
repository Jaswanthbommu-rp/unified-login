CREATE PROCEDURE [Enterprise].[CreateCommunicationEvent] 
				@StatusTypeID int, @FromPartyContactMechanismId bigint, @ToPartyContactMechanismId bigint, @Started datetime, @Ended datetime, @Note nvarchar(1000), @CommunicationEventID bigint OUTPUT
AS
BEGIN
	DECLARE @Now datetime;
	DECLARE @CommunicationEventIdOutput TABLE (CommunicationEventId INT)
	SET @Now = GETUTCDATE();
	SET @CommunicationEventID = NULL;
	
	BEGIN TRY
		BEGIN
			MERGE Enterprise.CommunicationEvent AS TARGET
			USING(VALUES( @StatusTypeID, @FromPartyContactMechanismId, @ToPartyContactMechanismId, @Note )) AS SOURCE(StatusTypeId, PartyContactMechanismIdFrom, PartyContactMechanismIdTo, [Note])
			ON TARGET.PartyContactMechanismIdFrom = @FromPartyContactMechanismId AND 
			   TARGET.PartyContactMechanismIdTo = @ToPartyContactMechanismId
			WHEN MATCHED
				  THEN UPDATE SET StatusTypeID = @StatusTypeId, PartyContactMechanismIdFrom = @FromPartyContactMechanismId, PartyContactMechanismIdTo = @ToPartyContactMechanismId, [Started] = ISNULL(@Started, @Now), [Ended] = @Ended, [Note] = @Note
			WHEN NOT MATCHED BY TARGET
				  THEN
				  INSERT(StatusTypeID, PartyContactMechanismIdFrom, PartyContactMechanismIdTo, [Started], [Ended], [Note])
				  VALUES( @StatusTypeID, @FromPartyContactMechanismId, @ToPartyContactMechanismId, ISNULL(@Started, @Now), @Ended, @Note )
				  OUTPUT Inserted.CommunicationEventId INTO @CommunicationEventIdOutput;
			SELECT @CommunicationEventID = CommunicationEventId from @CommunicationEventIdOutput
			
		END;
		SELECT @CommunicationEventID AS Id, '' AS ErrorMessage FROM @CommunicationEventIdOutput;
	END TRY
	BEGIN CATCH
		DECLARE @ErrorLogID int;
		EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;
		SELECT 0 AS Id, ErrorMessage
		FROM dbo.ErrorLog
		WHERE ErrorLogID = @ErrorLogID;
	END CATCH;
END;