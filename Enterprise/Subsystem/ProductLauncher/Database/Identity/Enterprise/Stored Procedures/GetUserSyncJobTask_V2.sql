Create PROCEDURE [Enterprise].[GetUserSyncJobTask_V2]
(@UserSyncJobTaskId BIGINT)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @NOW DATETIME = GETUTCDATE();
    SELECT DISTINCT TOP (1)
           sjt.UserSyncJobId,
           x.PersonaId,
           x.LoginName,
           x.ProductId,
           sjt.[Source],
           x.UserSyncJobTypeId,
		   x.KafkaTopicName,
		   x.UserOrgRealpageId
    FROM Enterprise.UserSyncJobTask_V2 sjt
        CROSS APPLY
    (
        SELECT TOP 1000000000000
               sua.PersonaId,
               sua.Value AS LoginName,
               p.ProductId,
               ISNULL(p.UDMSourceCode, p.BooksProductCode) AS Source,
               sj.UserSyncJobTypeId,
			   JT.KafkaTopicName,
			   pt.RealPageId AS 'UserOrgRealpageId'
        FROM Enterprise.UserSyncJob_V2 sj
		    INNER JOIN [Enterprise].[UserSyncJobType] JT ON
				sj.[UserSyncJobTypeId] = JT.[UserSyncJobTypeId]
            INNER JOIN Ident.SamlUserAttribute sua
                ON sj.UserPersonaId = sua.PersonaId
            INNER JOIN Enterprise.Product p
                ON p.ProductId = sua.ProductId
            INNER JOIN Ident.SamlAttribute sa
                ON sua.SamlAttributeId = sa.SamlAttributeId
			INNER JOIN  Person.Persona PP ON
				sj.[UserPersonaId] = PP.PersonaId
			INNER JOIN  ident.UserLoginPersona ulp on
					pp.UserLoginPersonaId  = ulp.UserLoginPersonaId
			INNER JOIN Enterprise.Party pt on
				pt.PartyId = ulp.OrganizationPartyId
        WHERE sjt.UserSyncJobId = sj.UserSyncJobId
              AND sa.Name = 'UserId'
              AND
              (
                  (@NOW
              BETWEEN sua.FromDate AND sua.ThruDate
                  )
                  OR
                  (
                      @NOW >= sua.FromDate
                      AND sua.ThruDate IS NULL
                  )
              )
              AND sjt.Source = ISNULL(p.UDMSourceCode, p.BooksProductCode)
    ) AS x
    WHERE UserSyncJobTaskId = @UserSyncJobTaskId;
END;
GO

