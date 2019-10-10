CREATE PROCEDURE [Auth].ScopesInsert
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
	@AllowUnrestrictedIntrospection bit
)
AS
BEGIN
	SET NOCOUNT OFF;
	INSERT INTO [Auth].[Scopes] 
		([Name]
		, [DisplayName]
		, [Description]
		, [ClaimsRule]
		, [Enabled]
		, [Required]
		, [Emphasize]
		, [Type]
		, [IncludeAllClaimsForUser]
		, [ShowInDiscoveryDocument]
		, [AllowUnrestrictedIntrospection]) 
	VALUES 
		(@Name
		, @DisplayName
		, @Description
		, @ClaimsRule
		, @Enabled
		, @Required
		, @Emphasize
		, @Type
		, @IncludeAllClaimsForUser
		, @ShowInDiscoveryDocument
		, @AllowUnrestrictedIntrospection);
	
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
		(ScopeId = SCOPE_IDENTITY())
END
