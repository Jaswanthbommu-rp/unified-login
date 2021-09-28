CREATE PROCEDURE [Security].[GetUserManagementADGroupsByProduct]
(
 @ProductId int = NULL
)
AS
BEGIN
	SELECT 
		AG.ADGroupId
		,AG.DisplayName AS ADGroupName
	FROM Security.ADGroupRight adr 
		INNER JOIN Security.[right] r on r.RightId = adr.RightId
		INNER JOIN Security.ADGroup ag on ag.ADGroupId = adr.ADGroupId
	WHERe (@ProductId IS NULL OR r.TargetProductId = @ProductId)  
END
