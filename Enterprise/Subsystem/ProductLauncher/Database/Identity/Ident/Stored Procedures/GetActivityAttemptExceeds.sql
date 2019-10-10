CREATE PROCEDURE [Ident].[GetActivityAttemptExceeds] 
(
@enterpriseUserName AS NVARCHAR(255),
@ActivityTypeId AS         INT,
@PartyId INT
)
AS
     BEGIN
         SET NOCOUNT ON;
         DECLARE @AttemptCount AS INT
		 DECLARE @maxActivitycount AS INT
		 DECLARE @ActivityTokenExpirationMinutes AS INT;
         --DECLARE @Activ
		 SELECT @maxActivitycount = AC.MaxActivityAttemptCount,
                @ActivityTokenExpirationMinutes = AC.ActivityTokenExpirationMinutes
         FROM [Ident].ActivityConfiguration AC
			INNER JOIN Ident.ActivityType ATP
				ON ATP.ActivityTypeId = AC.ActivityTypeId
		WHERE AC.PartyId = @PartyId
         AND  ATP.ActivityTypeId = @ActivityTypeId;
         
		 SELECT TOP 1 @AttemptCount = AA.AttemptCount
         FROM [Ident].[ActivityAttempts] AA
			INNER JOIN  [Ident].ActivityConfiguration AC 
				ON AC.ActivityConfigurationId = AA.ActivityConfigurationId
			INNER JOIN Ident.ActivityType ATP
				ON ATP.ActivityTypeId = AC.ActivityTypeId
         WHERE AA.[EnterpriseUserName] = @enterpriseUserName
               AND ATP.ActivityTypeId = @ActivityTypeId
               AND AA.LastAttemptDateTime >= DATEADD(minute, -@ActivityTokenExpirationMinutes, GETUTCDATE())
			   AND AC.PartyId = @PartyId
         ORDER BY LastAttemptDateTime DESC;
         
		 SELECT @AttemptCount AS AttemptCount,
                @maxActivitycount AS maxActivitycount,
                @ActivityTokenExpirationMinutes AS ActivityTokenExpirationMinutes;
     END;