CREATE PROCEDURE [Auth].ScopesSelect
AS
BEGIN
	SET NOCOUNT ON;
	SELECT        
		ScopeId
		, Name
		, DisplayName
		, Description
		, ClaimsRule
		, Enabled
		, Required
		, Emphasize
		, Type
		, IncludeAllClaimsForUser
		, ShowInDiscoveryDocument
		, AllowUnrestrictedIntrospection
	FROM
		Auth.Scopes
END
