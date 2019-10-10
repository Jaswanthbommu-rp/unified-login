IF OBJECT_ID('[Auth].[CreateSecurityQuestionAnswers]') IS NOT NULL
	DROP PROCEDURE [Auth].[CreateSecurityQuestionAnswers];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[CreateSecurityQuestionAnswers]
(
	@enterpriseUserName as nvarchar(50),
	@activityToken as nvarchar(100),
	@activityId as int,
	@securityQuestion1Id as int,
	@securityAnswer1 as nvarchar(50),
	@securityQuestion2Id as int,
	@securityAnswer2 as nvarchar(50),
	@securityQuestion3Id as int,
	@securityAnswer3 as nvarchar(50)
)
AS
BEGIN

	BEGIN TRY

	declare @UserId as int,
	@insertDateTime as smalldatetime

	select @UserId=userid from auth.users where LoginId=@EnterpriseUserName

	if Exists(select top 1  [ActivityToken] from auth.ActivityToken
	where [ActivityId]=@ActivityId and IsActive =1 and UserId=@UserId and ActivityToken=@activityToken) 
	Begin
	
		select @insertDateTime=getdate()
		
		delete from [Auth].[UserSecurityAnswer] where UserId=@UserId -- delete old questions

		INSERT INTO [Auth].[UserSecurityAnswer]
           ([UserId],[SecurityQuestionId],[Answer],[CreateDateTime])
        VALUES
           (@UserId,@securityQuestion1Id,@securityAnswer1,@insertDateTime),
		   (@UserId,@securityQuestion2Id,@securityAnswer2,@insertDateTime),
		   (@UserId,@securityQuestion3Id,@securityAnswer3,@insertDateTime)


		SELECT	@@ROWCOUNT AS 'rowCount',
				@UserId AS Id,
				0	AS errorNumber,
				'' AS errorMessage	
		end	

	END TRY  
	BEGIN CATCH
		SELECT	@@ROWCOUNT AS 'rowCount',
				0 AS Id,
				ERROR_NUMBER() AS errorNumber,
				ERROR_MESSAGE() AS errorMessage
	END CATCH
END
GO
