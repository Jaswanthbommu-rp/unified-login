CREATE PROCEDURE [Auth].[SearchApiResourceClaims]
	@ApiResourceId INT = NULL
AS
BEGIN

	SELECT [Id]
		  ,[ApiResourceId]
		  ,[Type]
	FROM 
		[Auth].[ApiResourceClaims]
	WHERE
		@ApiResourceId IS NULL OR @ApiResourceId = ApiResourceId		
END
GO
