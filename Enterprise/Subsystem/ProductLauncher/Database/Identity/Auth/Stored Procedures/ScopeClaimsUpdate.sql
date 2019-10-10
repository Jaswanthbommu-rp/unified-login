CREATE PROCEDURE [Auth].ScopeClaimsUpdate
(
	@ScopeId int,
	@Name nvarchar(200),
	@Description nvarchar(1000),
	@AlwaysIncludeInIdToken bit,
	@Original_ScopeClaimId int,
	@Original_ScopeId int,
	@Original_Name nvarchar(200),
	@IsNull_Description Int,
	@Original_Description nvarchar(1000),
	@Original_AlwaysIncludeInIdToken bit,
	@ScopeClaimId int
)
AS
BEGIN
	SET NOCOUNT OFF;
	UPDATE [Auth].[ScopeClaims] 
	SET 
		[ScopeId] = @ScopeId
		, [Name] = @Name
		, [Description] = @Description
		, [AlwaysIncludeInIdToken] = @AlwaysIncludeInIdToken 
	WHERE 
	(
		([ScopeClaimId] = @Original_ScopeClaimId) 
		AND ([ScopeId] = @Original_ScopeId) 
		AND ([Name] = @Original_Name) 
		AND ((@IsNull_Description = 1 AND [Description] IS NULL) OR ([Description] = @Original_Description)) 
		AND ([AlwaysIncludeInIdToken] = @Original_AlwaysIncludeInIdToken))
	
	SELECT 
		ScopeClaimId as Id
		, ScopeId
		, Name
		, Description
		, AlwaysIncludeInIdToken 
	FROM Auth.ScopeClaims 
	WHERE 
		(ScopeClaimId = @ScopeClaimId)
END
