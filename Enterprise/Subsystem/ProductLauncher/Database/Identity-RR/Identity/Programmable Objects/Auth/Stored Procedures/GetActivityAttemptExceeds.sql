IF OBJECT_ID('[Auth].[GetActivityAttemptExceeds]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetActivityAttemptExceeds];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetActivityAttemptExceeds]
 		@enterpriseUserName as nvarchar(50),
		@activityId as int
AS
BEGIN

	SET NOCOUNT ON;
	 
	declare @AttemptCount as int,
	@maxActivitycount as int,
	@ActivityAttemptsId as int,
	@ActivityTokenExpirationMinutes as tinyint

	--TODO check @entepriseUserName exist

	select @maxActivitycount = MaxActivityAttemptCount, @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from auth.Activity where activityId=@activityId
	print @maxActivitycount
	select top 1 @AttemptCount=AttemptCount, @ActivityAttemptsId=ActivityAttemptsId from [Auth].[ActivityAttempts] 
	where [EnterpriseUserName]=@enterpriseUserName and activityId=@activityId and LastAttemptDateTime  >=   dateadd(minute, -@ActivityTokenExpirationMinutes, getdate()) order by LastAttemptDateTime desc
	print @AttemptCount

	IF @AttemptCount is not null and @AttemptCount >= @maxActivitycount	
		select 0 as IsAttemptCountSuccess
	ELSE 
		select 1 as IsAttemptCountSuccess
	 
END
GO
