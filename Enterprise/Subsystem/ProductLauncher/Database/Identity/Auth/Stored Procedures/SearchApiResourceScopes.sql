CREATE PROCEDURE [Auth].[SearchApiResourceScopes]
	@ApiResourceId INT = NULL

AS
BEGIN
	SELECT [Id]
		  ,[Scope]
		  ,[ApiResourceId]
	FROM 
		[Auth].[ApiResourceScopes]
	WHERE
		@ApiResourceId IS NULL OR ApiResourceId = @ApiResourceId
END
GO

