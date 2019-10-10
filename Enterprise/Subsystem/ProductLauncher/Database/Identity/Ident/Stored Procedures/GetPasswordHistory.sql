CREATE PROCEDURE [Ident].[GetPasswordHistory]
    @enterpriseUserName AS NVARCHAR(255) ,   
    @numberOfPasswordsToRemember AS INT = 5
AS
    BEGIN
	 
        SET NOCOUNT ON;

		SELECT PasswordHash,PasswordSalt FROM [Ident].UserLogin 
		WHERE LoginName = @enterpriseUserName AND NOT PasswordHash IS NULL--Cant set the new password to the sam as the current password.
		UNION
		SELECT PasswordHash, PasswordSalt FROM (
			SELECT   TOP (@numberOfPasswordsToRemember-1) --The -1 is to return 5 records not 6 per Ajit Mungale
				[Ident].PasswordHistory.ChangedPasswordHash as PasswordHash, [Ident].PasswordHistory.ChangedPasswordSalt as PasswordSalt
			FROM [Ident].UserLogin 
			INNER JOIN  [Ident].PasswordHistory ON [Ident].UserLogin.UserId = [Ident].PasswordHistory.UserId
			WHERE [Ident].UserLogin.LoginName =  @enterpriseUserName
			ORDER BY [ChangedPasswordDateTime] DESC) AS History

    END;