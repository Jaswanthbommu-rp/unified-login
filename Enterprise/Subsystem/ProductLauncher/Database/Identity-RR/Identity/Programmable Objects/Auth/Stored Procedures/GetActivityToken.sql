IF OBJECT_ID('[Auth].[GetActivityToken]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetActivityToken];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetActivityToken]
	@EnterpriseUserName as nvarchar(50),
	@ActivityToken as nvarchar(50),
	@ActivityId		as int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @UserId as bigint,
	@ActivityTokenExpirationMinutes as int


	select @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from auth.Activity where activityId=@activityId

	select @UserId=userid from users where LoginId=@EnterpriseUserName
	--TODO check if userid exist

	select top 1  @UserId as EnterpriseUserId, [ActivityToken] as Token from [Auth].[ActivityToken]
	 where [ActivityId]= @ActivityId and IsActive =1 and UserId=@UserId and  ActivityToken=@ActivityToken and [ExpireDateTime] >  dateadd(minute,-@ActivityTokenExpirationMinutes,GetDate())


END



--select top 1  [ActivityToken] from [Auth].[ActivityToken]
--	 where  [ActivityId]=2 and IsActive =1 and UserId=17 and [ExpireDateTime] > dateadd(minute,-30,GetDate())
GO
