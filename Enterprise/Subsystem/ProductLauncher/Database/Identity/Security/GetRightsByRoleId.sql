Create Procedure [Security].[GetRightsByRoleId] (@roleId int)
AS
Begin
	Select r.RightId, RightName, r.Description, p.ProductId, p.Name as ProductName, 
	r.TargetProductId, p1.Name as TargetProductName, 
	st.StatusTypeId, st.Name as StatusType, 
	r.VisibilityStatusId, st1.Name as Visibility
	from Security.[Right] r
		join Security.[RoleRight] rr on rr.RightId = r.RightId
		join Enterprise.Product p on p.ProductId = r.ProductId 
		join Enterprise.Product p1 on p1.ProductId = r.TargetProductId
		join Enterprise.StatusType st on st.StatusTypeId = r.StatusTypeId
		join Enterprise.StatusType st1 on st1.StatusTypeId = r.VisibilityStatusId
	where RoleId = @roleId
End














