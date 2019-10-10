IF OBJECT_ID('[Auth].[ResetEnterpriseUserCredential]') IS NOT NULL
	DROP PROCEDURE [Auth].[ResetEnterpriseUserCredential];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[ResetEnterpriseUserCredential] ( 
	@enterpriseUserName as nvarchar(50),
	@newPasswordHash as nvarchar(1000),
	@newPasswordSalt as nvarchar(255) 
)
AS
BEGIN
	BEGIN TRY
		
		declare @UserId as bigint,
		@oldPassword as nvarchar(1000),
		@oldPasswordSalt as nvarchar(255)

		select @UserId=userid, @oldPassword=PasswordHash,@oldPasswordSalt=PasswordSalt from auth.users where LoginId=@EnterpriseUserName
			 
		UPDATE	[Auth].[Users]
		SET		PasswordHash=@newPasswordHash, PasswordSalt = @newPasswordSalt WHERE	userId = @UserId

		SELECT	@@ROWCOUNT AS 'rowCount',
				@UserId AS Id, --TODO: get unique-user-id as input param instead of @enterpriseUserName
				0	AS errorNumber,
				'' AS errorMessage		 
	END TRY  
	BEGIN CATCH
		SELECT	@@ROWCOUNT AS 'rowCount',
				0 AS Id,
				ERROR_NUMBER() AS errorNumber,
				ERROR_MESSAGE() AS errorMessage
	END CATCH
END
GO
