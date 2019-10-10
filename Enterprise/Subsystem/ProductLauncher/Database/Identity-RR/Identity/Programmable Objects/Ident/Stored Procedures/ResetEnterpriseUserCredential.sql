IF OBJECT_ID('[Ident].[ResetEnterpriseUserCredential]') IS NOT NULL
	DROP PROCEDURE [Ident].[ResetEnterpriseUserCredential];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[ResetEnterpriseUserCredential] ( 
	@realPageId as uniqueidentifier,
	@newPasswordHash as nvarchar(255),
	@newPasswordSalt as nvarchar(255) 
)
AS
BEGIN
	BEGIN TRY
		
		declare @UserId as bigint,
		@oldPassword as nvarchar(255),
		@oldPasswordSalt as nvarchar(255),
		@currentUtcDate datetime

		select @currentUtcDate = GETUTCDATE()
			
		SELECT @UserId=Ident.UserLogin.UserId, @oldPassword=Ident.UserLogin.PasswordHash, @oldPasswordSalt=Ident.UserLogin.PasswordSalt  FROM Enterprise.Party 
		INNER JOIN Ident.UserLogin ON Enterprise.Party.PartyId = Ident.UserLogin.PartyId
		where Enterprise.Party.RealPageId =@realPageId
					 				 
		UPDATE	[ident].[UserLogin]
		SET		PasswordHash=@newPasswordHash, PasswordSalt = @newPasswordSalt, PasswordModifiedDate=@currentUtcDate WHERE userId = @UserId

		-- insert old password in history table
		if(@oldPassword is not null)
			INSERT INTO [Ident].[PasswordHistory] ([UserId],[ActivityId],[ChangedPasswordHash],[ChangedPasswordSalt],[ChangedPasswordDateTime])
			VALUES(@UserId,2,@oldPassword,@oldPasswordSalt,@currentUtcDate)
 

		SELECT	@@ROWCOUNT AS 'rowCount',
				@UserId AS Id, 
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
