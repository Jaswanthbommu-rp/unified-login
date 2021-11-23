CREATE   Procedure [Security].[GetRoutesWithRightCount]
As
Begin
select ro.RouteId, ro.RouteValue as RouteName, ro.Description as RouteDescription, count(r.RightId) as RightCount
from Security.[Right] r
join Security.[RightRoute] rr on rr.RightId = r.RightId
right join Security.[Route] ro on ro.RouteId = rr.RouteId
group by ro.RouteId, ro.RouteValue, ro.Description
End