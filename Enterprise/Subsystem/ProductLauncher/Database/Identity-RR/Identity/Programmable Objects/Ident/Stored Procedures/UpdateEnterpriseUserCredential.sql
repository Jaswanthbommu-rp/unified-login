IF OBJECT_ID('[Ident].[UpdateEnterpriseUserCredential]') IS NOT NULL
	DROP PROCEDURE [Ident].[UpdateEnterpriseUserCredential];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[UpdateEnterpriseUserCredential]
	@EnterpriseUserName as nvarchar(255),
	@correctAnswerToken as nvarchar(50),
	@ActivityId		as int,
	@NewPasswordHash as  nvarchar(255),
	@passwordSalt as  nvarchar(255)	 
AS
BEGIN
	
	SET NOCOUNT ON;

	declare @UserId as bigint,
	@realPageId as uniqueidentifier,
	@oldPassword as nvarchar(255),
	@oldPasswordSalt as nvarchar(255),
	@currentUtcDate as datetime

	select @currentUtcDate = getutcdate()

	SELECT @UserId=ul.UserId, @oldPassword=ul.PasswordHash, @oldPasswordSalt=ul.PasswordSalt, @realPageId=p.RealPageId
	FROM Ident.UserLogin ul 
	INNER JOIN Enterprise.Party p ON ul.PartyId = p.PartyId where ul.LoginName=@EnterpriseUserName
		 

	if Exists(select top 1  [ActivityToken] from Ident.ActivityToken
	 where [ActivityId]=@ActivityId and IsActive =1 and realPageId=@realPageId and ActivityToken=@correctAnswerToken)  
	 Begin
		-- update password
		Update Ident.UserLogin set PasswordHash=@NewPasswordHash,PasswordSalt=@passwordSalt,PasswordModifiedDate=@currentUtcDate where UserId=@UserId 

		-- insert old pwd in history table
		IF(@oldPassword IS NOT NULL)
			INSERT INTO Ident.[PasswordHistory]([UserId],[ActivityId],[ChangedPasswordHash],[ChangedPasswordSalt],[ChangedPasswordDateTime])
			VALUES (@UserId,@ActivityId,@oldPassword,@oldPasswordSalt,@currentUtcDate)
		
		-- reset token flags
		UPDATE Ident.[ActivityToken] set isActive=0 WHERE realPageId=@realPageId and activityId=2 OR activityId=6  
        UPDATE Ident.[ActivityAttempts] SET  [AttemptCount] = 0 WHERE [EnterpriseUserName] = @EnterpriseUserName 
		and LastAttemptDateTime >=   dateadd(day, -3, @currentUtcDate) and ([ActivityId] = 2 or [ActivityId] = 5 or [ActivityId] = 6)
	 end
	else
		select @UserId = null --RAISERROR('Activity token expired', 16,16)

	select @UserId
END
GO
