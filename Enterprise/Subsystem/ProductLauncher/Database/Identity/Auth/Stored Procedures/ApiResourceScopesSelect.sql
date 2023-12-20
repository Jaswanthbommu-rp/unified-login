CREATE PROCEDURE [Auth].[ApiResourceScopesSelect]
(
	@Id INT = 0,
	@ApiResourceId INT = 0
)
AS
BEGIN
	SELECT 
		 [Id]
		,[Scope]
		,[ApiResourceId]
	FROM 
		[Auth].[ApiResourceScopes]
	WHERE
		(@Id = 0 OR [Id] = @Id)
		AND
		(@ApiResourceId = 0 OR [ApiResourceId] = @ApiResourceId)
END

GO
