CREATE PROCEDURE [Security].[GetRoutesWithRightCount]
AS
BEGIN
SELECT ro.RouteId, ro.RouteValue as RouteName, ro.Description as RouteDescription, count(r.RightId) as RightCount
FROM SECURITY.[Right] r
JOIN SECURITY.[RightRoute] rr on rr.RightId = r.RightId
JOIN SECURITY.[Route] ro on ro.RouteId = rr.RouteId
GROUP BY ro.RouteId, ro.RouteValue, ro.Description
End