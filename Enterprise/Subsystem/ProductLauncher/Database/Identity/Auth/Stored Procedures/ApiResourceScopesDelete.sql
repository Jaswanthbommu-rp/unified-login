CREATE PROCEDURE [Auth].[ApiResourceScopesDelete]
(
	@Original_ApiResourceScopeId INT
	,@Original_ApiResourceId INT
	,@Original_Scope nvarchar(200)
)
AS
BEGIN
	SET NOCOUNT OFF;
	DELETE FROM [Auth].[ApiResourceScopes]
	WHERE
		[Id] = @Original_ApiResourceScopeId
		AND ApiResourceId = @Original_ApiResourceId
		AND Scope = @Original_Scope

	SELECT @@RowCount [RowsAffected]
END