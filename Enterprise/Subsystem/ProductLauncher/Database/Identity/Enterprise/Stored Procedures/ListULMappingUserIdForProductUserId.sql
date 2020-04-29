CREATE PROCEDURE [Enterprise].[ListULMappingUserIdForProductUserId]
(
    @CompanyId INT,
    @ProductId INT,
    @TargetProductUserIds nvarchar(max)
)
AS
BEGIN

	DECLARE @SamlAttributeId int;
	DECLARE @ProductUserIdList TABLE(ProductUserId nvarchar(50));

	INSERT INTO @ProductUserIdList(ProductUserId)
	(
		SELECT *
		FROM STRING_SPLIT(@TargetProductUserIds, ',')
	);

	SELECT @SamlAttributeId = SamlAttributeId FROM Ident.SamlAttribute
	WHERE Name = 'UserId'

	SELECT sua.Value AS ProductUserId, sua.PersonaId UnifiedLoginUserId 
	FROM Ident.SamlUserAttribute sua
	INNER JOIN @ProductUserIdList puid ON sua.Value = puid.ProductUserId	
	INNER JOIN Person.Persona p ON p.PersonaId = sua.PersonaId
	INNER JOIN Ident.UserLoginPersona ulp ON p.UserLoginPersonaId = ulp.UserLoginPersonaId
	WHERE ulp.OrganizationPartyId = @CompanyId
	AND ProductId = @ProductId
	AND SamlAttributeId = @SamlAttributeId
	
END;

GO