CREATE PROCEDURE [Enterprise].[GetExternalUserRelationship]
(
	@UserLoginPersonaId BIGINT
)
AS
BEGIN
    SELECT 
		EUR.UserLoginPersonaId,
		EUR.ThirdPartyRelationshipId,
		TPR.ThirdPartyRelationship,
		CASE WHEN EUR.ThirdPartyCompanyPartyId IS NULL THEN EUR.CompanyName ELSE O.Name END AS ThirdPartyCompanyName,
		P.RealPageId AS ThirdPartyCompanyRealPageId,    
		CASE WHEN OperatorValue IS NULL THEN NULL ELSE SUBSTRING(OperatorValue, 1, CHARINDEX('|', OperatorValue + '|') - 1) END AS OperatorCode,
		CASE WHEN OperatorValue IS NULL THEN NULL ELSE SUBSTRING(OperatorValue, CHARINDEX('|', OperatorValue) + 1, LEN(OperatorValue)) END AS OperatorValue
		
	FROM enterprise.ExternalUserRelationship EUR 
		INNER JOIN [Enterprise].[ThirdPartyRelationship] TPR ON EUR.ThirdPartyRelationshipId = TPR.ThirdPartyRelationshipId
		LEFT JOIN Enterprise.Organization o ON EUR.ThirdPartyCompanyPartyId = o.PartyId
		LEFT JOIN Enterprise.Party P ON O.PartyId = P.PartyId
	WHERE
		EUR.UserLoginPersonaId = @UserLoginPersonaId
END

