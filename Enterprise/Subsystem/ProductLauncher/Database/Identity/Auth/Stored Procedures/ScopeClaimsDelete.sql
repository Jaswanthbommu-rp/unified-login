CREATE PROCEDURE [Auth].ScopeClaimsDelete
(
	@Original_ScopeClaimId int,
	@Original_ScopeId int,
	@Original_Name nvarchar(200),
	@IsNull_Description Int,
	@Original_Description nvarchar(1000),
	@Original_AlwaysIncludeInIdToken bit
)
AS
BEGIN
	SET NOCOUNT OFF;
	DELETE FROM [Auth].[ScopeClaims] 
	WHERE 
	(
		([ScopeClaimId] = @Original_ScopeClaimId) 
		AND ([ScopeId] = @Original_ScopeId)
		AND ([Name] = @Original_Name) 
		AND ((@IsNull_Description = 1 AND [Description] IS NULL) OR ([Description] = @Original_Description)) 
		AND ([AlwaysIncludeInIdToken] = @Original_AlwaysIncludeInIdToken)
	)
END
