CREATE PROCEDURE Enterprise.GetUserEmployeeIdByUserLoginPersonaId
(
	@UserLoginPersonaId BIGINT,
	@OrganizationPartyId BIGINT
)AS
BEGIN
	SELECT
		ue.UserEmployeeId,
		ue.UserLoginPersonaId,
		ue.Employee AS EmployeeId
	FROM Ident.UserLoginPersona ULP
		INNER JOIN Enterprise.UserEmployeeId AS ue ON ulp.UserLoginPersonaId = ue.UserLoginPersonaId
	WHERE
		ULP.UserLoginPersonaId = @UserLoginPersonaId
		AND ULP.OrganizationPartyId = @OrganizationPartyId
END;
