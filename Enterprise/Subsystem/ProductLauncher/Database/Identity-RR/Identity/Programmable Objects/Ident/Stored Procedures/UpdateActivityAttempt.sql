IF OBJECT_ID('[Ident].[UpdateActivityAttempt]') IS NOT NULL
	DROP PROCEDURE [Ident].[UpdateActivityAttempt];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[UpdateActivityAttempt]
 		@enterpriseUserName as nvarchar(255),
		@activityId as int,
		@browserName as nvarchar(20)='',
		@browserType as nvarchar(20)='',
		@ipAddress as nvarchar(50)='',
		@isMobile as bit=0,
		@platform as nvarchar(20)='', 
		@version as nvarchar(10)='',
		@deviceType as nvarchar(20)='',
		@timezone as nvarchar(100)='',
		@authenticationServiceId as nvarchar(50)='' 

AS
BEGIN

	SET NOCOUNT ON;
	 
	declare @AttemptCount as int,
	@maxActivitycount as int,
	@ActivityAttemptsId as int,
	@ActivityTokenExpirationMinutes as tinyint,
	@currentUtcDate datetime

	select @currentUtcDate=GETUTCDATE()
	 
	select @maxActivitycount = MaxActivityAttemptCount, @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from Ident.Activity where activityId=@activityId
 
	select top 1 @AttemptCount=AttemptCount, @ActivityAttemptsId=ActivityAttemptsId from Ident.[ActivityAttempts] where 
		[EnterpriseUserName]=@enterpriseUserName and activityId=@activityId and LastAttemptDateTime >= dateadd(minute, -@ActivityTokenExpirationMinutes, @currentUtcDate)   
		order by [ActivityAttemptsId] desc
	 

	IF @activityId = 10 -- unlock user - login / forgot password
		BEGIN				
			update Ident.[ActivityAttempts]	set [AttemptCount] = 0 
			where (activityid= 1 or activityid= 2 or activityid=5) and EnterpriseUserName=@enterpriseUserName and [LastAttemptDateTime] >DATEADD(minute,-60, @currentUtcDate)				
		END
	ELSE IF @activityId = 7 -- LoginSuccess activity
		BEGIN
			-- insert unique record for LoginSuccess			
			INSERT INTO Ident.[ActivityAttempts]
						([ActivityId] ,[EnterpriseUserName] ,[AttemptCount]  ,[IpAddress] ,[BrowserType] ,
						[BrowserName]  ,[Version] ,[Platform]  ,[IsMobile]  ,[LastAttemptDateTime], [DeviceType], [Timezone], [AuthenticationServiceId])
					VALUES
						(@activityId ,@enterpriseUserName,1,@ipAddress,@browserType
						,@browserName ,@version,@platform,@isMobile,@currentUtcDate, @deviceType, @timezone, @authenticationServiceId)

			select * from Ident.[ActivityAttempts] where ActivityAttemptsId = (select scope_identity())

			-- after successful login reset falied login activity count for last 1 hr
			update Ident.[ActivityAttempts]	set [AttemptCount] = 0 
			where activityid= 1 and EnterpriseUserName=@enterpriseUserName and [LastAttemptDateTime] >DATEADD(minute,-60,@currentUtcDate)				

			update Ident.[UserLogin] set [LastLoginDate] = GETUTCDATE() WHERE LoginName = @enterpriseUserName

		END
	ELSE IF @AttemptCount >= @maxActivitycount	
		BEGIN
			select top 1 * from Ident.[ActivityAttempts] where  activityAttemptsId=@ActivityAttemptsId 
			-- increment @AttemptCount
			update Ident.[ActivityAttempts]
			set [AttemptCount]  = @AttemptCount +1 where  activityAttemptsId=@ActivityAttemptsId  
		END		
	ELSE if @AttemptCount is null or @AttemptCount = 0			
			BEGIN
			-- insert record
				 INSERT INTO Ident.[ActivityAttempts]
					   ([ActivityId] ,[EnterpriseUserName] ,[AttemptCount]  ,[IpAddress] ,[BrowserType] ,
						[BrowserName]  ,[Version] ,[Platform]  ,[IsMobile]  ,[LastAttemptDateTime], [DeviceType], [Timezone], [AuthenticationServiceId])
				 VALUES
					   (@activityId ,@enterpriseUserName,1,@ipAddress,@browserType
					   ,@browserName ,@version,@platform,@isMobile,@currentUtcDate, @deviceType, @timezone, @authenticationServiceId)
						 
				select top 1 * from Ident.[ActivityAttempts] where [EnterpriseUserName]=@enterpriseUserName 
				and activityId=@activityId and LastAttemptDateTime < DATEADD(minute,-@ActivityTokenExpirationMinutes,@currentUtcDate)
				order by [ActivityAttemptsId] desc
			END 
	ELSE
			BEGIN
				-- increment @AttemptCount
				update Ident.[ActivityAttempts]
				set [AttemptCount]  = @AttemptCount +1 where  activityAttemptsId=@ActivityAttemptsId   
    
				select top 1 * from Ident.[ActivityAttempts] where activityAttemptsId=@ActivityAttemptsId  
				order by [ActivityAttemptsId] desc
			END 
	 
END
GO
