CREATE PROCEDURE [Security].[GetRightsAssignedToRole]
    @RoleId int
AS
    BEGIN
		SELECT R.RightId AS RightId,
		R.RightName AS RightName,
		P1.ProductId AS ProductId,
		P1.[Name] AS ProductName,
		P2.ProductId AS TargetProductId,
		P2.[Name] AS TargetProductName
		FROM [Security].[Right] R
		INNER JOIN [Security].RoleRight RT ON R.RightId = RT.RightId
		INNER JOIN Enterprise.Product P1 ON R.ProductId = P1.ProductId
		INNER JOIN Enterprise.Product P2 ON R.TargetProductId = P2.ProductId
		WHERE RT.RoleId = @RoleId
    END;
