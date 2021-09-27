CREATE PROCEDURE [Security].[GetADGroupsByProductId] (
	@ProductId int)
AS
BEGIN
	SELECT AP.ADGroupId, G.DisplayName AS ADGroupName
	FROM [Security].ADGroupProduct AP
	JOIN [Security].ADGroup G on G.ADGroupId = AP.ADGroupId
	WHERE ProductId = @ProductId
END
