CREATE PROCEDURE [Ident].[ListFutureLogins] (
	@BatchSize smallint
)
AS
BEGIN
	DECLARE @StatusTypeId int,
		@Now datetime = GETUTCDATE()

	SELECT	@StatusTypeId = StatusTypeId
	FROM		Enterprise.StatusType
	WHERE	Name = 'Running';

	DECLARE @FutureUsers TABLE (
		UserRealPageId uniqueidentifier,
		OrganizationRealPageId uniqueidentifier,
		UserLogin varchar(255),
		FromDate datetime,
		StatusTypeId int
	);

	UPDATE TOP (@BatchSize) CE
	SET	StatusTYpeId = @StatusTypeId
	OUTPUT	PA.RealPageId,
			PA2.RealPageId,
					UL.LoginName,
					ULP.FromDate,
					inserted.StatusTypeId
	INTO @FutureUsers
	FROM	Ident.UserLogin UL
				INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
				INNER JOIN Enterprise.Party PA ON PA.PartyId = UL.PersonPartyId
				INNER JOIN Enterprise.Party PA2 ON PA2.PartyId = ULP.OrganizationPartyId
				INNER JOIN Enterprise.PartyRelationShip PR ON (PR.PartyIdFrom = PA.PartyId AND pr.PartyIdTo = ULP.OrganizationPartyId)
				INNER JOIN Enterprise.PartyContactMechanism PCM ON PA.PartyId = PCM.PartyId
				INNER JOIN Enterprise.PartyContactMechanism PCM2 ON PA2.PartyId = PCM2.PartyId
				INNER JOIN Enterprise.ElectronicAddress EA on EA.ContactMechanismID = PCM2.ContactMechanismId
				INNER JOIN Enterprise.CommunicationEvent CE ON PCM.PartyContactMechanismId = CE.PartyContactMechanismIdTo AND PCM2.PartyContactMechanismId = CE.PartyContactMechanismIdFrom
				LEFT OUTER JOIN Enterprise.StatusType ST ON CE.StatusTypeID = ST.StatusTypeId  
				LEFT OUTER JOIN Enterprise.CommunicationEventEmail CEE ON CE.CommunicationEventID = CEE.CommunicationEventId  
				LEFT OUTER JOIN Enterprise.CommunicationEmailTemplate CET ON CEE.CommunicationEmailTemplateID = CET.CommunicationEmailTemplateID  
				LEFT OUTER JOIN Enterprise.CommunicationEventPurposeType CEPT ON CET.CommunicationEventPurposeTypeId = CEPT.CommunicationEventPurposeTypeID  
	WHERE	ULP.FromDate BETWEEN DATEADD(MINUTE, 0, @Now) AND DATEADD(MINUTE, 30, @Now)
			AND			PR.[RoleTypeIdFrom] IN (401, 402, 404, 405)
			AND			ULP.StatusTypeId = 24 --Pending
			AND			(CEPT.Description <> 'New User Setup' OR CEPT.Description IS NULL)

	SELECT	UserRealPageId,
			OrganizationRealPageId,
			UserLogin,
			FromDate
	FROM	@FutureUsers
		ORDER BY
			OrganizationRealPageId
END;
