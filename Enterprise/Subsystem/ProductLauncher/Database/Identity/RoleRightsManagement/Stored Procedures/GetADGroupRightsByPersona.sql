CREATE PROCEDURE [Security].[GetADGroupRightsByPersona]
	@PersonaId bigint
AS
BEGIN
	SELECT  
		Convert(int,r.RightID) [RightId]
		,r.Value [Right]
		,r.RightName [RightNickName]
		,Convert(int,r.RightId) [RightValueTypeId]
		,r.IsExcludeRightFromImpersonation
	FROM Security.ADGroupUser  ADU
	INNER JOIN Security.ADGroup ADG ON ADG.ADGroupId = ADU.ADGroupId
	INNER JOIN security.ADGroupRight adr ON adr.ADGroupId = adg.ADGroupId
	INNER JOIN security.[Right] r ON r.RightId = adr.RightId
	INNER JOIN enterprise.Product p ON p.ProductId = r.ProductId
	INNER JOIN enterprise.product p1 ON p1.ProductId = r.TargetProductId
	WHERE PersonaId=@PersonaId AND ADG.IsActive = 1
END