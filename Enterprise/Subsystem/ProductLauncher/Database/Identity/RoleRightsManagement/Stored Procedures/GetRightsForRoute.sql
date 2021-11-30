CREATE PROCEDURE [Security].[GetRightsForRoute](
    @RouteId int
)
AS
    BEGIN
		SELECT RT.RightId AS RightId,
		RT.RightName AS RightName,
		RT.[Value] AS [Value],
		RT.[Description] AS [Description],
		RT.ProductId AS ProductId,
		P1.[Name] AS ProductName,
		RT.TargetProductId AS TargetProductId,
		P2.[Name] AS TargetProductName
		FROM [Security].[RightRoute] RR
		INNER JOIN [Security].[Right] RT ON RT.RightId = RR.RightId
		INNER JOIN [Enterprise].[Product] P1 ON RT.ProductId = P1.ProductId
		INNER JOIN [Enterprise].[Product] P2 ON RT.TargetProductId = P2.ProductId
		WHERE RR.RouteId = @RouteId
	END
