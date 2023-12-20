CREATE PROCEDURE [Auth].[ApiResourceSecretsSelect]
(
	@Id INT = 0,
	@ApiResourceId INT = 0
)
AS
BEGIN

	SELECT 
		 [Id]
		,[ApiResourceId]
		,[Description]
		,[Value]
		,[Expiration]
		,[Type]
		,[Created]
	FROM 
		[Auth].[ApiResourceSecrets]
	WHERE
		(@Id = 0 OR [Id] = @Id)
		AND
		(@ApiResourceId = 0 OR [ApiResourceId] = @ApiResourceId)
END
GO
