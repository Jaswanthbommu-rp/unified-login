CREATE PROCEDURE [Auth].ScopesUpdate
(
	@Name nvarchar(200),
	@DisplayName nvarchar(200),
	@Description nvarchar(1000),
	@ClaimsRule nvarchar(200),
	@Enabled bit,
	@Required bit,
	@Emphasize bit,
	@Type int,
	@IncludeAllClaimsForUser bit,
	@ShowInDiscoveryDocument bit,
	@AllowUnrestrictedIntrospection bit,
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
	@Original_AllowUnrestrictedIntrospection bit,
	@ScopeId int
)
AS
BEGIN
	SET NOCOUNT OFF;
	
	IF @Original_Name <> @Name
	BEGIN
		-- UPDATE ANY CLIENTS WITH THE OLD NAME TO THE NEW NAME
		UPDATE Auth.ClientScopes
			SET Scope = @Name
		WHERE
			Scope = @Original_Name
	END

	UPDATE [Auth].[Scopes] 
	SET 
		[Name] = @Name
		, [DisplayName] = @DisplayName
		, [Description] = @Description
		, [ClaimsRule] = @ClaimsRule
		, [Enabled] = @Enabled
		, [Required] = @Required
		, [Emphasize] = @Emphasize
		, [Type] = @Type
		, [IncludeAllClaimsForUser] = @IncludeAllClaimsForUser
		, [ShowInDiscoveryDocument] = @ShowInDiscoveryDocument
		, [AllowUnrestrictedIntrospection] = @AllowUnrestrictedIntrospection 
	WHERE 
		(([ScopeId] = @Original_ScopeId) 
			AND ([Name] = @Original_Name) 
			AND ((@IsNull_DisplayName = 1 AND [DisplayName] IS NULL) OR ([DisplayName] = @Original_DisplayName)) 
			AND ((@IsNull_Description = 1 AND [Description] IS NULL) OR ([Description] = @Original_Description)) 
			AND ((@IsNull_ClaimsRule = 1 AND [ClaimsRule] IS NULL) OR ([ClaimsRule] = @Original_ClaimsRule)) 
			AND ([Enabled] = @Original_Enabled) 
			AND ([Required] = @Original_Required) 
			AND ([Emphasize] = @Original_Emphasize) 
			AND ([Type] = @Original_Type) 
			AND ([IncludeAllClaimsForUser] = @Original_IncludeAllClaimsForUser) 
			AND ([ShowInDiscoveryDocument] = @Original_ShowInDiscoveryDocument) 
			AND ([AllowUnrestrictedIntrospection] = @Original_AllowUnrestrictedIntrospection));
	
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
	WHERE 
		(ScopeId = @ScopeId)
END
