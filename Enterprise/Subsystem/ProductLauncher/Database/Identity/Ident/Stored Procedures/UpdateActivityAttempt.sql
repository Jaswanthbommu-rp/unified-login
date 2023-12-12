CREATE PROCEDURE [Ident].[UpdateActivityAttempt]
(@enterpriseUserName AS      NVARCHAR(255),
@ActivityTypeId AS          INT,
@browserName AS             NVARCHAR(20)  = '',
@browserType AS             NVARCHAR(20)  = '',
@ipAddress AS               NVARCHAR(50)  = '',
@isMobile AS                BIT           = 0,
@platform AS                NVARCHAR(20)  = '',
@version AS                 NVARCHAR(10)  = '',
@deviceType AS              NVARCHAR(20)  = '',
@timezone AS                NVARCHAR(100) = '',
@authenticationServiceId AS NVARCHAR(50)  = '',
@PartyId                 INT
)
AS
     BEGIN
         SET NOCOUNT ON;
         DECLARE @AttemptCount AS INT;
         DECLARE @maxActivitycount AS INT;
         DECLARE @ActivityAttemptsId AS INT;
         DECLARE @ActivityTokenExpirationMinutes AS TINYINT;
         DECLARE @currentUtcDate DATETIME;
         DECLARE @ActivityConfigurationId INT;
         DECLARE @UnlockUser INT;
         DECLARE @UnlockExpirationMinutes INT;
         DECLARE @UnlockTokenExpirationMinutes INT;
         DECLARE @UnlockActivityConfigurationId INT;

         SELECT @currentUtcDate = GETUTCDATE();
         
              IF @ActivityTypeId = 10 -- unlock user - login / forgot password
              BEGIN
                   SELECT @UnlockUser = AC.MaxActivityAttemptCount,
                        @UnlockExpirationMinutes = AC.ActivityTokenExpirationMinutes,
                        @UnlockActivityConfigurationId = AC.ActivityConfigurationId
                        -- @ActivityConfigurationId = AC.ActivityConfigurationId
                   FROM Ident.ActivityConfiguration AS AC
                        INNER JOIN Ident.ActivityType AS ATP ON ATP.ActivityTypeId = AC.ActivityTypeId
                        INNER JOIN enterprise.organization AS o ON AC.partyid = o.partyid
                   WHERE ATP.ActivityCode = 'UnlockUser'
                        AND AC.PartyId = @PartyId;
                   
                   
                   ;WITH UPCTE AS
                        (SELECT TOP 1
                             AA.ActivityAttemptsId
                   FROM Ident.[ActivityAttempts] AA
                        INNER JOIN Ident.ActivityConfiguration AC ON AC.ActivityConfigurationId = AA.ActivityConfigurationId
                        INNER JOIN Ident.ActivityType ATP ON ATP.ActivityTypeId = AC.ActivityTypeId
                   WHERE ATP.ActivityCode IN(N'Login', N'ForgotPassword', N'QuestionAttempts')
                        AND AA.EnterpriseUserName = @enterpriseUserName
                        --AND AA.[LastAttemptDateTime] > DATEADD(minute, 0, @currentUtcDate)
                        AND AC.PartyId = @PartyId
                        ORDER BY AA.ActivityAttemptsId DESC
                   )
                   UPDATE
                        A SET A.AttemptCount = 0
                   FROM UPCTE U
                        INNER JOIN Ident.[ActivityAttempts] A
                        ON A.ActivityAttemptsId = U.ActivityAttemptsId


              END;
            ELSE
         
                      SELECT @maxActivitycount = AC.MaxActivityAttemptCount,
                             @ActivityTokenExpirationMinutes = AC.ActivityTokenExpirationMinutes,
                             @ActivityConfigurationId = AC.ActivityConfigurationId
                        FROM Ident.ActivityConfiguration AC
                          INNER JOIN Ident.ActivityType ATP ON ATP.ActivityTypeId = AC.ActivityTypeId
                          INNER JOIN enterprise.organization o ON AC.partyid = o.partyid
                        WHERE ATP.ActivityTypeId = @ActivityTypeId
                           AND AC.PartyId = @PartyId;
         
                      SELECT TOP 1 @AttemptCount = AttemptCount,
                                    @ActivityAttemptsId = ActivityAttemptsId
                        FROM Ident.[ActivityAttempts] AA
                          INNER JOIN Ident.ActivityConfiguration AC ON AC.ActivityConfigurationId = AA.ActivityConfigurationId
                          INNER JOIN Ident.ActivityType ATP ON ATP.ActivityTypeId = AC.ActivityTypeId
                        WHERE [EnterpriseUserName] = @enterpriseUserName
                           AND ATP.ActivityTypeId = @ActivityTypeId
                           AND AA.LastAttemptDateTime >= DATEADD(minute, -@ActivityTokenExpirationMinutes, @currentUtcDate)
                        ORDER BY AA.[ActivityAttemptsId] DESC;
          
                      IF @ActivityTypeId = 7 -- LoginSuccess activity
                        BEGIN
                          -- insert unique record for LoginSuccess            
                              INSERT INTO Ident.[ActivityAttempts]
                                       ([ActivityConfigurationId],
                                       [EnterpriseUserName],
                                       [AttemptCount],
                                       [IpAddress],
                                       [BrowserType],
                                       [BrowserName],
                                       [Version],
                                       [Platform],
                                       [IsMobile],
                                       [LastAttemptDateTime],
                                       [DeviceType],
                                       [Timezone],
                                       [AuthenticationServiceId]
                                      )
                             VALUES
                                       (@ActivityConfigurationId,
                                       @enterpriseUserName,
                                      1,
                                       @ipAddress,
                                       @browserType,
                                       @browserName,
                                       @version,
                                       @platform,
                                       @isMobile,
                                       @currentUtcDate,
                                       @deviceType,
                                       @timezone,
                                       @authenticationServiceId
                                      );
                             SELECT *
                             FROM Ident.[ActivityAttempts]
                             WHERE ActivityAttemptsId =
                                      (
                                           SELECT SCOPE_IDENTITY()
                                      );

                          -- after successful login reset falied login activity count for last 1 hr
                             UPDATE AA
                                SET
                                     [AttemptCount] = 0
                             FROM Ident.[ActivityAttempts] AA
                                    INNER JOIN Ident.ActivityConfiguration AC ON AC.ActivityConfigurationId = AA.ActivityConfigurationId
                                    INNER JOIN Ident.ActivityType ATP ON ATP.ActivityTypeId = AC.ActivityTypeId
                             WHERE ATP.ActivityTypeId = 1
                                     AND AA.EnterpriseUserName = @enterpriseUserName
                                     AND AA.[LastAttemptDateTime] > DATEADD(minute, -@ActivityTokenExpirationMinutes, @currentUtcDate)
                                     AND AC.PartyId = @PartyId;
                 
                                  UPDATE Ident.[UserLogin]
                                SET
                                     [LastLoginDate] = GETUTCDATE()
                             WHERE LoginName = @enterpriseUserName;

                             UPDATE ulp
                                   SET ulp.LastLoginDate = GETUTCDATE()
                             FROM  Ident.UserLoginPersona ulp 
                             INNER JOIN ident.userlogin ul ON ul.UserId = ulp.UserLoginId
                             WHERE ul.LoginName = @enterpriseUserName AND ulp.OrganizationPartyId = @PartyId
                        END;
                        ELSE
                                IF @AttemptCount <= @maxActivitycount
                                BEGIN
                                      SELECT TOP 1 *
                                      FROM Ident.[ActivityAttempts]
                                      WHERE activityAttemptsId = @ActivityAttemptsId; 
                                   -- increment @AttemptCount
                                      UPDATE Ident.[ActivityAttempts]
                                         SET
                                                [AttemptCount] = @AttemptCount + 1
                                      WHERE activityAttemptsId = @ActivityAttemptsId;
                                END;
             ELSE
                   IF (@AttemptCount IS NULL
                        OR @AttemptCount = 0)
                          AND @ActivityTypeId NOT IN (7, 10)
                        BEGIN
                          -- insert record
                             INSERT INTO Ident.[ActivityAttempts]
                                       ([ActivityConfigurationId],
                                       [EnterpriseUserName],
                                       [AttemptCount],
                                       [IpAddress],
                                       [BrowserType],
                                       [BrowserName],
                                       [Version],
                                       [Platform],
                                       [IsMobile],
                                       [LastAttemptDateTime],
                                       [DeviceType],
                                       [Timezone],
                                       [AuthenticationServiceId]
                                      )
                             VALUES
                                       (@ActivityConfigurationId,
                                       @enterpriseUserName,
                                      1,
                                       @ipAddress,
                                       @browserType,
                                       @browserName,
                                       @version,
                                      @platform,
                                       @isMobile,
                                       @currentUtcDate,
                                       @deviceType,
                                       @timezone,
                                       @authenticationServiceId
                                      );
                             SELECT TOP 1 *
                             FROM Ident.[ActivityAttempts] AA
                                    INNER JOIN Ident.ActivityConfiguration AC ON AC.ActivityConfigurationId = AA.ActivityConfigurationId
                                    INNER JOIN Ident.ActivityType ATP ON ATP.ActivityTypeId = AC.ActivityTypeId
                             WHERE [EnterpriseUserName] = @enterpriseUserName
                                     AND ATP.ActivityTypeId = @ActivityTypeId
                                     AND AA.LastAttemptDateTime < DATEADD(minute, -@ActivityTokenExpirationMinutes, @currentUtcDate)
                             ORDER BY AA.[ActivityAttemptsId] DESC;
                        END;
                        ELSE
                        BEGIN
                                -- increment @AttemptCount
                             --UPDATE Ident.[ActivityAttempts]
                             --  SET
                             --      [AttemptCount] = @AttemptCount + 1
                             --WHERE activityAttemptsId = @ActivityAttemptsId;
                             SELECT TOP 1 *
                             FROM Ident.[ActivityAttempts]
                             WHERE activityAttemptsId = @ActivityAttemptsId
                             ORDER BY [ActivityAttemptsId] DESC;
                        END;
     END;
