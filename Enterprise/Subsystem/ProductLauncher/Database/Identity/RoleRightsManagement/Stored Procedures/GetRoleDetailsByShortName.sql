Create PROCEDURE [Security].[GetRoleDetailsByShortName](@roleName varchar(100), @productId int)
AS
    BEGIN
		SELECT R.RoleId AS RoleId,
		R.RoleName AS RoleName,
		R.ShortName AS ShortName,
		R.[Description] AS  [Description],
		R.ProductId AS ProductId,
		P.[Name] AS ProductName,
		R.RoleTypeID AS RoleTypeId,
		RT.[Value] AS RoleType,
		R.OrgPartyID AS OrgPartyId,
		O.[Name] AS Visibility
		FROM [Security].[Role] R
		INNER JOIN Enterprise.Product P ON R.ProductId = P.ProductId
		INNER JOIN [Security].RoleType RT ON R.RoleTypeID = RT.RoleTypeId
		LEFT JOIN [Enterprise].[Organization] O ON R.OrgPartyID = O.PartyId
		WHERE R.ShortName = @roleName
		And R.ProductId = @productId
    END;
