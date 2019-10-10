IF OBJECT_ID('[Ident].[GetActivityAttemptExceeds]') IS NOT NULL
	DROP PROCEDURE [Ident].[GetActivityAttemptExceeds];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[GetActivityAttemptExceeds]
 		@enterpriseUserName as nvarchar(255),
		@activityId as int 
AS
BEGIN

	SET NOCOUNT ON;
	 
	declare @AttemptCount as int,
	@maxActivitycount as int,
	@ActivityTokenExpirationMinutes as int
	 
	select @maxActivitycount = MaxActivityAttemptCount, @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from [Ident].Activity where activityId=@activityId
	 
	select top 1 @AttemptCount=AttemptCount from [Ident].[ActivityAttempts] 
	where [EnterpriseUserName]=@enterpriseUserName and activityId=@activityId and LastAttemptDateTime  >=   dateadd(minute, -@ActivityTokenExpirationMinutes, getutcdate()) order by LastAttemptDateTime desc
 
	select @AttemptCount as  AttemptCount,   @maxActivitycount as  maxActivitycount, @ActivityTokenExpirationMinutes as  ActivityTokenExpirationMinutes
END
GO
