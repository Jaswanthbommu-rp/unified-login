CREATE PROCEDURE [Enterprise].[ListULMappingPersonaIdForProductUserId]
(
    @CompanyId INT,
    @ProductId INT,
    @TargetProductUserIds nvarchar(max)
)
AS
BEGIN

	Declare @SamlAttributeId int;
	DECLARE @ProductUserIdList TABLE(ProductUserId nvarchar(50));

	INSERT INTO @ProductUserIdList(ProductUserId)
	(
		SELECT *
		FROM STRING_SPLIT(@TargetProductUserIds, ',')
	);

	SELECT @SamlAttributeId = SamlAttributeId FROM Ident.SamlAttribute
	WHERE Name = 'UserId'

	SELECT sua.Value as ProductUserId, sua.PersonaId as UnifiedLoginPersonaId 
	from Ident.SamlUserAttribute sua
	inner join @ProductUserIdList puid on sua.Value = puid.ProductUserId	
	inner join Person.Persona p on p.PersonaId = sua.PersonaId
	inner join Ident.UserLoginPersona ulp on p.UserLoginPersonaId = ulp.UserLoginPersonaId
	inner join Enterprise.DataImportMapping dim on dim.PartyId = ulp.OrganizationPartyId
	WHERE dim.SourceId = @CompanyId
	AND dim.DataImportApplicationId = 2
	AND ProductId = @ProductId
	AND SamlAttributeId = @SamlAttributeId
	
END;

GO