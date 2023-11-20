CREATE PROCEDURE [Security].[GetRightDetailsByRightId] (@rightId int)
AS
BEGIN  
 Select r.RightId,   
  r.RightName,   
  r.Description,  
  ISNULL(r.Value,r.RightName) As Value,  
  p.ProductId,  
  p.Name as ProductName,  
  t.ProductId as TargetProductId,  
  t.Name as TargetProductName,  
  s.StatusTypeId,  
  s.Name as StatusTypeName,  
  v.StatusTypeId as VisibilityStatusId,  
  v.Name as VisibilityStatusName,
  r.IsExcludeRightFromImpersonation  
 FROM Security.[Right] r  
 JOIN Enterprise.Product p on p.ProductId = r.ProductId  
 JOIN Enterprise.Product t on t.ProductId = r.TargetProductId  
 JOIN Enterprise.StatusType s on s.StatusTypeId = r.StatusTypeId  
 JOIN Enterprise.StatusType v on v.StatusTypeId = r.VisibilityStatusId  
 WHERE r.RightId = @rightId  
  
 SELECT rl.RoleId, rl.RoleName, p.ProductId, p.Name as ProductName  
 FROM SECURITY.[RoleRight] rr  
  JOIN SECURITY.[Role] rl on rl.RoleId = rr.RoleId  
  INNER JOIN Security.Role R ON R.RoleId = RR.RoleId  
  JOIN Enterprise.Product p on p.ProductId = rl.ProductId  
 WHERE rr.RightId = @rightId   
   
   
 SELECT r.RouteId, RouteValue as RouteName  
 FROM SECURITY.[RightRoute] rr  
  JOIN SECURITY.[Route] r on r.RouteId = rr.RouteId  
 WHERE rr.RightId = @rightId  
   
   
 SELECT org.PartyId as OrganizationId, org.Name as CompanyName  
 FROM Security.OrganizationOverRideRight r  
 JOIN Enterprise.Organization org on org.PartyId = r.OrgPartyId  
 WHERE r.RightId = @rightId  
END