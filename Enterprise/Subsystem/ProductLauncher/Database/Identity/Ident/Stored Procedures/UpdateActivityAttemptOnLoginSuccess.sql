CREATE PROCEDURE [Ident].[UpdateActivityAttemptOnLoginSuccess]  
(
	@enterpriseUserName AS      VARCHAR(255),  
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
	@PartyId                    BIGINT  
)  
AS  
     BEGIN  
         SET NOCOUNT ON;  
         DECLARE @ActivityTokenExpirationMinutes AS TINYINT;  
         DECLARE @currentUtcDate DATETIME;  
         DECLARE @ActivityConfigurationId INT;  
  
         SELECT @currentUtcDate = GETUTCDATE();           
                      SELECT @ActivityTokenExpirationMinutes = AC.ActivityTokenExpirationMinutes,  
                             @ActivityConfigurationId = AC.ActivityConfigurationId  
                        FROM Ident.ActivityConfiguration AC  
                          INNER JOIN Ident.ActivityType ATP ON ATP.ActivityTypeId = AC.ActivityTypeId  
                          INNER JOIN enterprise.organization o ON AC.partyid = o.partyid  
                        WHERE ATP.ActivityTypeId = @ActivityTypeId  
                           AND AC.PartyId = @PartyId;  
           
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
						SET 
						    [LastLoginDate] = GETUTCDATE()
					    FROM  Ident.UserLoginPersona ulp 
					    INNER JOIN ident.userlogin ul ON ul.UserId = ulp.UserLoginId
					    WHERE ul.LoginName = @enterpriseUserName AND ulp.OrganizationPartyId = @PartyId
     END;