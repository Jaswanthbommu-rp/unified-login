IF OBJECT_ID('[Ident].[GetActivityToken]') IS NOT NULL
	DROP PROCEDURE [Ident].[GetActivityToken];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[GetActivityToken]
	@EnterpriseUserName as nvarchar(255),
	@ActivityToken as nvarchar(50),
	@ActivityId		as int
AS
BEGIN
	
	SET NOCOUNT ON;

	declare @UserId as bigint,
	@realPageId as uniqueidentifier,
	@ActivityTokenExpirationMinutes as int
	
	select @ActivityTokenExpirationMinutes=ActivityTokenExpirationMinutes from [Ident].Activity where activityId=@activityId
		 
	SELECT @UserId=u.UserId, @realPageId=p.RealPageId	FROM Ident.UserLogin u
	INNER JOIN Enterprise.Party p ON u.PartyId = p.PartyId
	where u.LoginName=@EnterpriseUserName
	 
	select top 1  @UserId as EnterpriseUserId, [ActivityToken] as Token, @realPageId as realPageId from [Ident].[ActivityToken]
	where [ActivityId]= @ActivityId and IsActive =1 and  realPageId=@realPageId and  ActivityToken=@ActivityToken and [ExpireDateTime] >  dateadd(minute,-@ActivityTokenExpirationMinutes,GetUtcDate())
	
END
GO
