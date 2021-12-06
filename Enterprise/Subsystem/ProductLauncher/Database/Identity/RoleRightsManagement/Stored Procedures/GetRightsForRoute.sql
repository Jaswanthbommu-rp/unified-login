CREATE PROCEDURE [Security].[GetRightsForRoute](
    @RouteId int
)
AS
BEGIN
	SELECT 
		RT.RightId [RightId],
		RT.RightName [RightName],
		RT.[Value] [RightValue],
		RT.[Description] [Description],
		RT.ProductId [ProductId],
		P1.[Name] [ProductName],
		RT.TargetProductId [TargetProductId],
		P2.[Name] [TargetProductName]
	
	FROM [Security].[RightRoute] RR
		INNER JOIN [Security].[Right] RT ON RT.RightId = RR.RightId
		INNER JOIN [Enterprise].[Product] P1 ON RT.ProductId = P1.ProductId
		INNER JOIN [Enterprise].[Product] P2 ON RT.TargetProductId = P2.ProductId
	WHERE 
		RR.RouteId = @RouteId
END
