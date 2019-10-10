IF OBJECT_ID('[Auth].[GetPasswordHistory]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetPasswordHistory];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetPasswordHistory]
    @enterpriseUserName AS NVARCHAR(50) ,   
    @numberOfPasswordsToRemember AS INT = 5
AS
    BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
        SET NOCOUNT ON;

  --      SELECT CAST(ISNULL(MAX(CASE WHEN p.ChangedPasswordHash = @NewPasswordHash THEN 1 ELSE 0 END),0) AS BIT) AS PasswordExists
  --      FROM Auth.Users u 
		--CROSS APPLY (SELECT TOP (@numberOfPasswordsToRemember) *
		--FROM Auth.PasswordHistory PH
		--WHERE ph.UserId = u.UserId
		--ORDER BY PH.ChangedPasswordDateTime DESC) AS p
		--WHERE u.LoginId = @enterpriseUserName

		SELECT   TOP (@numberOfPasswordsToRemember) Auth.PasswordHistory.ChangedPasswordHash as PasswordHash, Auth.PasswordHistory.ChangedPasswordSalt as PasswordSalt
		FROM            Auth.Users INNER JOIN
		 Auth.PasswordHistory ON Auth.Users.UserId = Auth.PasswordHistory.UserId
		 where Auth.Users.LoginId=  @enterpriseUserName
		 order by Auth.PasswordHistory.ChangedPasswordDateTime desc

    END;
GO
