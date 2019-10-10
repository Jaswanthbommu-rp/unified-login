CREATE PROCEDURE [Auth].ScopesDelete
(
	@Original_ScopeId int,
	@Original_Name nvarchar(200),
	@IsNull_DisplayName Int,
	@Original_DisplayName nvarchar(200),
	@IsNull_Description Int,
	@Original_Description nvarchar(1000),
	@IsNull_ClaimsRule Int,
	@Original_ClaimsRule nvarchar(200),
	@Original_Enabled bit,
	@Original_Required bit,
	@Original_Emphasize bit,
	@Original_Type int,
	@Original_IncludeAllClaimsForUser bit,
	@Original_ShowInDiscoveryDocument bit,
	@Original_AllowUnrestrictedIntrospection bit
)
AS
BEGIN
	SET NOCOUNT OFF;
	DELETE FROM [Auth].[Scopes] 
	WHERE 
		(([ScopeId] = @Original_ScopeId) 
		AND ([Name] = @Original_Name) 
		AND ((@IsNull_DisplayName = 1 
		AND [DisplayName] IS NULL) OR ([DisplayName] = @Original_DisplayName)) 
		AND ((@IsNull_Description = 1 AND [Description] IS NULL) OR ([Description] = @Original_Description)) 
		AND ((@IsNull_ClaimsRule = 1 AND [ClaimsRule] IS NULL) OR ([ClaimsRule] = @Original_ClaimsRule)) 
		AND ([Enabled] = @Original_Enabled) 
		AND ([Required] = @Original_Required) 
		AND ([Emphasize] = @Original_Emphasize) 
		AND ([Type] = @Original_Type) 
		AND ([IncludeAllClaimsForUser] = @Original_IncludeAllClaimsForUser) 
		AND ([ShowInDiscoveryDocument] = @Original_ShowInDiscoveryDocument) 
		AND ([AllowUnrestrictedIntrospection] = @Original_AllowUnrestrictedIntrospection))
END
