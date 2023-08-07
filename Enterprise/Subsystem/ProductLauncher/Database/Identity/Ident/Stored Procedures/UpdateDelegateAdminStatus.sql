CREATE PROCEDURE [Security].[UpdateDelegateAdminStatus] 
(
  @UserLoginPersonaId BIGINT,
  @isDelateAdmin bit = 0
)AS
BEGIN
    UPDATE Ident.UserLoginPersona 
	SET IsDelegateAdmin = @isDelateAdmin 
	WHERE UserLoginPersonaId = @UserLoginPersonaId;
END

