CREATE PROCEDURE [Ident].[SetActivityAttempt]  
(@enterpriseUserName AS NVARCHAR(255),  
@browserName AS             NVARCHAR(20)  = '',  
@browserType AS             NVARCHAR(20)  = '',  
@ipAddress AS               NVARCHAR(50)  = '',  
@isMobile AS                BIT           = 0,  
@platform AS                NVARCHAR(20)  = '',  
@version AS                 NVARCHAR(10)  = '',  
@deviceType AS              NVARCHAR(20)  = '',  
@timezone AS                NVARCHAR(100) = '',  
@authenticationServiceId AS NVARCHAR(50)  = '',  
@AttemptCount AS INT,  
@ActivityAttemptsId AS INT,  
@ActivityConfigurationId AS INT  
)  
AS  
 BEGIN  
 DECLARE @currentUtcDate DATETIME  
 SELECT @currentUtcDate = GETUTCDATE();  
 --First Attempt  
 IF @ActivityAttemptsId = 0  
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
                  @AttemptCount,  
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
  END  
 ELSE  
     BEGIN  
  UPDATE Ident.[ActivityAttempts]  
  SET [EnterpriseUserName] = @enterpriseUserName,  
  [BrowserName] = @browserName,  
  [BrowserType] = @browserType,  
  [IpAddress] = @ipAddress,  
  [IsMobile] = @isMobile,  
  [Platform] = @platform,  
  [Version] = @version,  
  [DeviceType] = @deviceType,  
  [Timezone] = @timezone,  
  [AuthenticationServiceId] =  @authenticationServiceId,  
  [AttemptCount] = @AttemptCount    
  WHERE activityAttemptsId = @ActivityAttemptsId;  
  END  
END