CREATE PROCEDURE [Auth].ScopeClaimsSelect
AS
BEGIN
	SET NOCOUNT ON;
	SELECT        
		ScopeClaimId as Id
		, ScopeId
		, Name
		, Description
		, AlwaysIncludeInIdToken
	FROM
		Auth.ScopeClaims
END
