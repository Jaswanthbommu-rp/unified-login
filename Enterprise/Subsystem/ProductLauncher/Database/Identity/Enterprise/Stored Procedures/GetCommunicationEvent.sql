CREATE PROCEDURE [Enterprise].[GetCommunicationEvent]
	@CommunicationEventID BIGINT
AS
	SELECT CommunicationEventID,
		StatusTypeID,
		PartyContactMechanismIdFrom,
		PartyContactMechanismIdTo,
		[Started],
		[Ended],
		[Note]
	FROM CommunicationEvent
	WHERE CommunicationEventID = @CommunicationEventID;
