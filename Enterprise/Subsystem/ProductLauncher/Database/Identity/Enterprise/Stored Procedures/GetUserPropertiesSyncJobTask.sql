CREATE PROCEDURE  [Enterprise].[GetUserPropertiesSyncJobTask]
(@UserSyncJobTaskId BIGINT)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NOW DATETIME = GETUTCDATE();
    SELECT sjt.UserSyncJobId,
           pp.PersonaId,
           ul.LoginName,
           p.ProductId,
           ISNULL(p.UDMSourceCode, p.BooksProductCode) AS Source,
           sj.UserSyncJobTypeId,
		   jt.KafkaTopicName,
		   pt.RealPageId AS 'UserOrgRealpageId'
        FROM Enterprise.UserSyncJob_V2 sj
			INNER JOIN Enterprise.UserSyncJobTask_V2 sjt on
				sjt.UserSyncJobId = sj.UserSyncJobId
		    INNER JOIN [Enterprise].[UserSyncJobType] JT ON
				sj.[UserSyncJobTypeId] = JT.[UserSyncJobTypeId]           
			INNER JOIN Enterprise.Product p
                ON p.BooksProductCode = sjt.Source
			INNER JOIN  Person.Persona PP ON
				sj.[UserPersonaId] = PP.PersonaId
			INNER JOIN  ident.UserLoginPersona ulp on
					pp.UserLoginPersonaId  = ulp.UserLoginPersonaId
			INNER JOIN  ident.UserLogin ul on
					ul.UserId  = ulp.UserLoginId
			INNER JOIN Enterprise.Party pt on
				pt.PartyId = ulp.OrganizationPartyId
        WHERE    sjt.UserSyncJobTaskId = @UserSyncJobTaskId;
END;
GO