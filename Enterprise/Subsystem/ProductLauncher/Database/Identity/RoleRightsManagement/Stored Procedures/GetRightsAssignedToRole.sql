CREATE PROCEDURE [Security].[GetRightsAssignedToRole]
    @RoleId int
AS
BEGIN
	SELECT 
		R.RightId [RightId],
		R.RightName [RightName],
		P1.ProductId [ProductId],
		P1.[Name] [ProductName],
		P2.ProductId [TargetProductId],
		P2.[Name] [TargetProductName],
		R.Value [RightValue]
	FROM [Security].[Right] R
		INNER JOIN [Security].RoleRight RT ON R.RightId = RT.RightId
		INNER JOIN Enterprise.Product P1 ON R.ProductId = P1.ProductId
		INNER JOIN Enterprise.Product P2 ON R.TargetProductId = P2.ProductId
		WHERE RT.RoleId = @RoleId
END;
