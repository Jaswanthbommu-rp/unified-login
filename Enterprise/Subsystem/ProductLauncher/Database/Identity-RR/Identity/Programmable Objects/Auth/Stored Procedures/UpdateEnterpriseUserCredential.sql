IF OBJECT_ID('[Auth].[UpdateEnterpriseUserCredential]') IS NOT NULL
	DROP PROCEDURE [Auth].[UpdateEnterpriseUserCredential];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[UpdateEnterpriseUserCredential]
	@EnterpriseUserName as nvarchar(50),
	@correctAnswerToken as nvarchar(50),
	@ActivityId		as int,
	@NewPasswordHash as  nvarchar(1000),
	@passwordSalt as  nvarchar(255)	 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	declare @UserId as bigint,
	@oldPassword as nvarchar(1000),
	@oldPasswordSalt as nvarchar(255)
	select @UserId=userid, @oldPassword=PasswordHash,@oldPasswordSalt=PasswordSalt from auth.users where LoginId=@EnterpriseUserName
	--TODO check if userid exist

	if Exists(select top 1  [ActivityToken] from auth.ActivityToken
	 where [ActivityId]=@ActivityId and IsActive =1 and UserId=@UserId and ActivityToken=@correctAnswerToken) --and [ExpireDateTime] < dateadd(minute, -5, getdate())) -- user has to change password in 5 mins
	 Begin
		-- update password
		Update auth.Users set PasswordHash=@NewPasswordHash,PasswordSalt=@passwordSalt where UserId=@UserId 
		-- insert old pwd in history table
		INSERT INTO [Auth].[PasswordHistory]([UserId],[ActivityId],[ChangedPasswordHash],[ChangedPasswordSalt],[ChangedPasswordDateTime])
		VALUES (@UserId,@ActivityId,@oldPassword,@oldPasswordSalt,getdate())

		-- reset token flags
		update [Auth].[ActivityToken] set isActive=0 WHERE userid=@userid and activityId=2 OR activityId=6 --  activityId=2 OR activityId=6 is for ForgotPassword & CorrectAnswer
        UPDATE [Auth].[ActivityAttempts] SET  [AttemptCount] = 0 WHERE [EnterpriseUserName] = @EnterpriseUserName 
		and LastAttemptDateTime >=   dateadd(day, -3, getdate()) and ([ActivityId] = 2 or [ActivityId] = 5 or [ActivityId] = 6)
	 end
	else
		select @UserId = null --RAISERROR('Activity token expired', 16,16)

	select @UserId
END
GO
