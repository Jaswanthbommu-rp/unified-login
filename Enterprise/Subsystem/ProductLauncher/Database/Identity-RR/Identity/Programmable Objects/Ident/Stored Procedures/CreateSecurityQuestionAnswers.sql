IF OBJECT_ID('[Ident].[CreateSecurityQuestionAnswers]') IS NOT NULL
	DROP PROCEDURE [Ident].[CreateSecurityQuestionAnswers];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[CreateSecurityQuestionAnswers]
(
	@enterpriseUserName as nvarchar(255),
	@activityToken as nvarchar(50),
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

	declare @realPageId as uniqueidentifier,
	@userid as bigint,
	@insertDateTime as smalldatetime

	SELECT   @realPageId = p.RealPageId, @userid=u.userid  FROM Ident.UserLogin u
	INNER JOIN Enterprise.Party p ON u.PartyId = p.PartyId
	where u.LoginName=@EnterpriseUserName

	if Exists(select top 1  [ActivityToken] from [Ident].ActivityToken
	where [ActivityId]=@ActivityId and IsActive =1 and  realPageId=@realPageId and ActivityToken=@activityToken) 
	Begin
	
		select @insertDateTime=getutcdate()
		
		delete from [Ident].[UserSecurityAnswer] where userid=@userid -- delete old questions-answers

		INSERT INTO [Ident].[UserSecurityAnswer]
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
