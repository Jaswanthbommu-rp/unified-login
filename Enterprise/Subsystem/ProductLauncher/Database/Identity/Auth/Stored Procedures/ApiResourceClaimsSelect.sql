CREATE PROCEDURE [Auth].[ApiResourceClaimsSelect]
(
	@Id INT = 0,
	@ApiResourceId INT = 0
)
AS
BEGIN
	SELECT 
		 [Id]
		,[ApiResourceId]
		,[Type]
	FROM 
		[Auth].[ApiResourceClaims]
	WHERE
		(@Id = 0 OR [Id] = @Id)
		AND
		(@ApiResourceId = 0 OR [ApiResourceId] = @ApiResourceId)
END

GO

