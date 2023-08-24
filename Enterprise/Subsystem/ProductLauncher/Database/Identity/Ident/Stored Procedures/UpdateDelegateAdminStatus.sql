CREATE PROCEDURE [Security].[UpdateDelegateAdminStatus] 
(
  @UserLoginPersonaId BIGINT,
  @IsDelegateAdmin bit = 0
)AS
BEGIN
    UPDATE Ident.UserLoginPersona 
	SET IsDelegateAdmin = @IsDelegateAdmin 
	WHERE UserLoginPersonaId = @UserLoginPersonaId;
END

