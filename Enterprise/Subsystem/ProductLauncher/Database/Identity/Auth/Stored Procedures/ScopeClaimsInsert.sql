CREATE PROCEDURE [Auth].ScopeClaimsInsert
(
	@ScopeId int,
	@Name nvarchar(200),
	@Description nvarchar(1000),
	@AlwaysIncludeInIdToken bit
)
AS
BEGIN
	SET NOCOUNT OFF;
	INSERT INTO [Auth].[ScopeClaims] 
	(
		[ScopeId]
		, [Name]
		, [Description]
		, [AlwaysIncludeInIdToken]
	) 
	VALUES 
	(
		@ScopeId
		, @Name
		, @Description
		, @AlwaysIncludeInIdToken
	)
	
	SELECT 
		ScopeClaimId as Id
		, ScopeId
		, Name
		, Description
		, AlwaysIncludeInIdToken 
	FROM Auth.ScopeClaims 
	WHERE 
		(ScopeClaimId = SCOPE_IDENTITY())
END