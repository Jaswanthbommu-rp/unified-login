CREATE PROCEDURE [Ident].[GetAllActivityTokenCounts]
(@EnterpriseUserName AS VARCHAR(255),
 @PartyId INT
)
AS
     BEGIN
         SET NOCOUNT ON;
         DECLARE @UserId AS BIGINT;
         DECLARE @realPageId AS UNIQUEIDENTIFIER;
         DECLARE @ActivityTokenExpirationMinutes AS INT;
		 DECLARE @ActivityConfigurationId INT

         SELECT @UserId = u.UserId,
                @realPageId = p.RealPageId
         FROM Ident.UserLogin AS u
              INNER JOIN Enterprise.Party AS p ON u.PersonPartyId = p.PartyId
         WHERE u.LoginName = @EnterpriseUserName;

         SELECT 
					  ATP.ActivityCode,

					  COUNT(1) AS TotalTokens
         FROM [Ident].[ActivityToken] ATK
			INNER JOIN Enterprise.Party P ON ATK.RealPageId = P.RealPageId
			INNER JOIN Ident.ActivityConfiguration AC
				ON AC.ActivityConfigurationId = ATK.ActivityConfigurationId
			INNER JOIN Ident.ActivityType ATP
				ON ATP.ActivityTypeId = AC.ActivityTypeId
         WHERE ATK.realPageId = @realPageId
			   AND AC.PartyId = @PartyId
			   AND ATK.CreateDateTime > DATEADD(DD, -30, GETUTCDATE())
		GROUP BY P.RealPageId, ATP.ActivityCode
     END;
