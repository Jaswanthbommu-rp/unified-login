CREATE PROCEDURE [Ident].[GetActivityToken]
(@EnterpriseUserName AS NVARCHAR(255),
 @ActivityToken AS      NVARCHAR(50),
 @ActivityTypeId AS         INT,
 @PartyId INT
)
AS
     BEGIN
         SET NOCOUNT ON;
         DECLARE @UserId AS BIGINT;
         DECLARE @realPageId AS UNIQUEIDENTIFIER;
         DECLARE @ActivityTokenExpirationMinutes AS INT;
		 DECLARE @ActivityConfigurationId INT
         
		 SELECT @ActivityTokenExpirationMinutes = A.ActivityTokenExpirationMinutes,
				@ActivityConfigurationId = A.ActivityConfigurationId
         FROM [Ident].ActivityConfiguration A
			INNER JOIN Ident.ActivityType AT
				ON AT.ActivityTypeId = A.ActivityTypeId
         WHERE AT.activityTypeId = @ActivityTypeId
		 AND A.PartyId = @PartyId

         SELECT @UserId = u.UserId,
                @realPageId = p.RealPageId
         FROM Ident.UserLogin AS u
              INNER JOIN Enterprise.Party AS p ON u.PersonPartyId = p.PartyId
         WHERE u.LoginName = @EnterpriseUserName;

         SELECT TOP 1 @UserId AS EnterpriseUserId,
                      [ActivityToken] AS Token,
                      @realPageId AS realPageId
         FROM [Ident].[ActivityToken] ATK
			INNER JOIN Ident.ActivityConfiguration AC
				ON AC.ActivityConfigurationId = ATK.ActivityConfigurationId
			INNER JOIN Ident.ActivityType ATP
				ON ATP.ActivityTypeId = AC.ActivityTypeId
         WHERE ATP.[ActivityTypeId] = @ActivityTypeId
               AND ATK.IsActive = 1
               AND ATK.realPageId = @realPageId
               AND ActivityToken = @ActivityToken
               AND ATK.[ExpireDateTime] > GETUTCDATE()
			   AND AC.PartyId = @PartyId
     END;