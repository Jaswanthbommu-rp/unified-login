--exec [Enterprise].[ListULMappingPersonaIdForProductUserId] 1308,1,'1192422|jreames1,1192422|mmacatee,1192422|FLastName,1192422'
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

	--Preferred mobile number logic
	DECLARE @ContactPreference TABLE( PersonaId INT
									, PreferredPhoneNumber VARCHAR(30))
	INSERT INTO @ContactPreference(PersonaId,PreferredPhoneNumber)
	SELECT AP.PersonaId AS PersonaId, ISNULL(TM.CountryCode,'') + TM.AreaCode + TM.PhoneNumber FROM	
			Enterprise.TelecommunicationsNumber tm 
			INNER JOIN Enterprise.PartyContactMechanism pcm ON tm.ContactMechanismID = pcm.ContactMechanismID
			INNER JOIN Person.ActivePersona AP ON AP.PartyId = PCM.PartyId
			INNER JOIN Person.Persona p on p.PersonaId = AP.PersonaId
			INNER JOIN Ident.UserLoginPersona ulp on p.UserLoginPersonaId = ulp.UserLoginPersonaId
			INNER JOIN Enterprise.DataImportMapping dim on dim.PartyId = ulp.OrganizationPartyId 
							AND dim.SourceId = @CompanyId AND dim.DataImportApplicationId = 2
			INNER JOIN Enterprise.[ContactMechanismPreference] CMP 
			ON CMP.ContactMechanismID = PCM.ContactMechanismId AND (PCM.ThruDate IS NULL OR PCM.ThruDate > GETUTCDATE())

	SELECT @SamlAttributeId = SamlAttributeId FROM Ident.SamlAttribute
	WHERE Name = 'UserId'

	SELECT	sua.Value as ProductUserId, 
			sua.PersonaId as UnifiedLoginPersonaId ,
			cp.PreferredPhoneNumber
	from Ident.SamlUserAttribute sua
	inner join @ProductUserIdList puid on sua.Value = puid.ProductUserId	
	inner join Person.Persona p on p.PersonaId = sua.PersonaId
	inner join Ident.UserLoginPersona ulp on p.UserLoginPersonaId = ulp.UserLoginPersonaId
	inner join Enterprise.DataImportMapping dim on dim.PartyId = ulp.OrganizationPartyId
	LEFT OUTER JOIN @ContactPreference CP ON CP.PersonaId = P.PersonaId
	WHERE dim.SourceId = @CompanyId
	AND dim.DataImportApplicationId = 2
	AND ProductId = @ProductId
	AND SamlAttributeId = @SamlAttributeId
	
END;

GO