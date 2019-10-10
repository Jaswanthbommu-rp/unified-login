IF OBJECT_ID('[Auth].[UpdateActivityAttempt]') IS NOT NULL
	DROP PROCEDURE [Auth].[UpdateActivityAttempt];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[UpdateActivityAttempt]
 		@enterpriseUserName as nvarchar(50),
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
	@ActivityTokenExpirationMinutes as tinyint
	 
	select @maxActivitycount = MaxActivityAttemptCount, @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from auth.Activity where activityId=@activityId
	--print @maxActivitycount
	select top 1@AttemptCount=AttemptCount, @ActivityAttemptsId=ActivityAttemptsId from [Auth].[ActivityAttempts] where 
		[EnterpriseUserName]=@enterpriseUserName and activityId=@activityId and LastAttemptDateTime >= dateadd(minute, -@ActivityTokenExpirationMinutes, getdate())   
		order by [ActivityAttemptsId] desc
	--print @AttemptCount

	IF @activityId = 7 -- LoginSuccess activity
		BEGIN
			-- insert unique record for LoginSuccess
			if not exists (select top 1 [ActivityId] from [Auth].[ActivityAttempts] where ActivityId = @activityId and concat([EnterpriseUserName], [IpAddress], [DeviceType], [BrowserName]) 
				like concat(@enterpriseUserName, @ipAddress, @deviceType, @browserName))
				BEGIN
					INSERT INTO [Auth].[ActivityAttempts]
							   ([ActivityId] ,[EnterpriseUserName] ,[AttemptCount]  ,[IpAddress] ,[BrowserType] ,
								[BrowserName]  ,[Version] ,[Platform]  ,[IsMobile]  ,[LastAttemptDateTime], [DeviceType], [Timezone], [AuthenticationServiceId])
						 VALUES
							   (@activityId ,@enterpriseUserName,1,@ipAddress,@browserType
							   ,@browserName ,@version,@platform,@isMobile,GetDate(), @deviceType, @timezone, @authenticationServiceId)

					select * from [Auth].[ActivityAttempts] where ActivityAttemptsId = (select scope_identity())
				END
			else
				select top 1 * from [Auth].[ActivityAttempts] where ActivityId = @activityId and concat([EnterpriseUserName], [IpAddress], [DeviceType], [BrowserName]) 
				like concat(@enterpriseUserName, @ipAddress, @deviceType, @browserName)
		END
	ELSE IF @AttemptCount >= @maxActivitycount	
		select top 1 * from [Auth].[ActivityAttempts] where  activityAttemptsId=@ActivityAttemptsId -- [EntepriseUserName]=@@entepriseUserName and activityId=@activityId and LastAttemptDateTime	< DATEADD(minute,30,GETDATE())		
	ELSE if @AttemptCount is null or @AttemptCount = 0			
			BEGIN
			-- insert record
				 INSERT INTO [Auth].[ActivityAttempts]
					   ([ActivityId] ,[EnterpriseUserName] ,[AttemptCount]  ,[IpAddress] ,[BrowserType] ,
						[BrowserName]  ,[Version] ,[Platform]  ,[IsMobile]  ,[LastAttemptDateTime], [DeviceType], [Timezone], [AuthenticationServiceId])
				 VALUES
					   (@activityId ,@enterpriseUserName,1,@ipAddress,@browserType
					   ,@browserName ,@version,@platform,@isMobile,GetDate(), @deviceType, @timezone, @authenticationServiceId)
						 
				select top 1 * from [Auth].[ActivityAttempts] where [EnterpriseUserName]=@enterpriseUserName and activityId=@activityId and LastAttemptDateTime < DATEADD(minute,-@ActivityTokenExpirationMinutes,GETDATE())
				 order by [ActivityAttemptsId] desc
			END 
	ELSE
			BEGIN
				-- increment @AttemptCount
				update [Auth].[ActivityAttempts]
				set [AttemptCount]  = @AttemptCount +1 where  activityAttemptsId=@ActivityAttemptsId  --[EntepriseUserName]=@@entepriseUserName and activityId=@activityId and LastAttemptDateTime < DATEADD(minute,30,GETDATE())
    
				select top 1 * from [Auth].[ActivityAttempts] where activityAttemptsId=@ActivityAttemptsId  
				order by [ActivityAttemptsId] desc-- -- [EntepriseUserName]=@@entepriseUserName and activityId=@activityId and LastAttemptDateTime< DATEADD(minute,30,GETDATE())
			END 
	 
END
GO
