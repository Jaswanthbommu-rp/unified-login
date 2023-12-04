CREATE PROCEDURE [Auth].[ApiResourcePropertySelect]
(
	@ApiResourceId INT = 0
)
AS
BEGIN
    SELECT [Id]
          ,[ApiResourceId]
          ,[Key]
          ,[Value]
    FROM 
        [Auth].[ApiResourceProperties]
    WHERE
		@ApiResourceId = 0 OR ApiResourceId = @ApiResourceId
END
GO

