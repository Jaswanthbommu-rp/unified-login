Create PROCEDURE [Enterprise].[GetUserSyncJobTaskDetails]
(@taskId	BIGINT)
AS
BEGIN

	Select [UserPersonaId],
		   [EditorUserPersonaId],
		   [Source] AS 'ProductSource',
		   p.RealPageId AS 'UserOrgRealpageId',
		   [KafkaTopicName]
	From [Enterprise].[UserSyncJobTask_V2] USJT
	Join [Enterprise].[UserSyncJob_V2] USJ On
		USJ.[UserSyncJobId] = USJT.[UserSyncJobId]
	Join [Enterprise].[UserSyncJobType] JT ON
		USJ.[UserSyncJobTypeId] = JT.[UserSyncJobTypeId]
	Join Person.Persona PP ON
		USJ.[UserPersonaId] = PP.PersonaId
	Join ident.UserLoginPersona ulp on
			pp.UserLoginPersonaId  = ulp.UserLoginPersonaId
		join Enterprise.Party p on
			p.PartyId = ulp.OrganizationPartyId
	Where USJT.[UserSyncJobTaskId] = @taskId

END
