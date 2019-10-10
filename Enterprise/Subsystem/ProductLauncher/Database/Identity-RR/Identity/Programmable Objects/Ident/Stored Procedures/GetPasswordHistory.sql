IF OBJECT_ID('[Ident].[GetPasswordHistory]') IS NOT NULL
	DROP PROCEDURE [Ident].[GetPasswordHistory];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[GetPasswordHistory]
    @enterpriseUserName AS NVARCHAR(255) ,   
    @numberOfPasswordsToRemember AS INT = 5
AS
    BEGIN
	 
        SET NOCOUNT ON;

  --      SELECT CAST(ISNULL(MAX(CASE WHEN p.ChangedPasswordHash = @NewPasswordHash THEN 1 ELSE 0 END),0) AS BIT) AS PasswordExists
  --      FROM [Ident].Users u 
		--CROSS APPLY (SELECT TOP (@numberOfPasswordsToRemember) *
		--FROM [Ident].PasswordHistory PH
		--WHERE ph.UserId = u.UserId
		--ORDER BY PH.ChangedPasswordDateTime DESC) AS p
		--WHERE u.LoginId = @enterpriseUserName

		SELECT   TOP (@numberOfPasswordsToRemember) [Ident].PasswordHistory.ChangedPasswordHash as PasswordHash, [Ident].PasswordHistory.ChangedPasswordSalt as PasswordSalt
		FROM            [Ident].UserLogin INNER JOIN
		 [Ident].PasswordHistory ON [Ident].UserLogin.UserId = [Ident].PasswordHistory.UserId
		 where [Ident].UserLogin.LoginName =  @enterpriseUserName
		 order by [Ident].PasswordHistory.PasswordHistoryId desc

    END;
GO
