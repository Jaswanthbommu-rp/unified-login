Create PROCEDURE [Security].[GetRightDetailsByRightName] (@rightName varchar(100), @productId int)
AS
BEGIN
	Select r.RightId, 
		r.RightName, 
		r.Description,
		p.ProductId,
		p.Name as ProductName,
		t.ProductId as TargetProductId,
		t.Name as TargetProductName,
		s.StatusTypeId,
		s.Name as StatusTypeName,
		v.StatusTypeId as VisibilityStatusId,
		v.Name as VisibilityStatusName
	FROM Security.[Right] r
	JOIN Enterprise.Product p on p.ProductId = r.ProductId
	JOIN Enterprise.Product t on t.ProductId = r.TargetProductId
	JOIN Enterprise.StatusType s on s.StatusTypeId = r.StatusTypeId
	JOIN Enterprise.StatusType v on v.StatusTypeId = r.VisibilityStatusId
	WHERE r.RightName = @rightName
	AND r.ProductId = @productId
	
END
