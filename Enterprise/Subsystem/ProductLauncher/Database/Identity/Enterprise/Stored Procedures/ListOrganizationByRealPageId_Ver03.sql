CREATE PROCEDURE [Enterprise].[ListOrganizationByRealPageId_Ver03] (
	@RealPageId uniqueidentifier,
	@RelationshipTypeName varchar(50) = NULL
)
AS  
BEGIN
	DECLARE @NOW  DATETIME = GETUTCDATE();
	IF @RelationshipTypeName IS NULL
	BEGIN
		SELECT	DISTINCT
						o.PartyId,
						o.Name,
						po.RealPageId,
						po.CreateDate,
						rtf.Name RoleNameFrom,
						rtt.Name RoleNameTo,
						rt.Name AS RelationshipType,
						ULP.PrimaryOrganization,
						COALESCE(ISNULL(edim.MasterId, 0), 0) AS 'BooksMasterId',
						COALESCE(ISNULL(Edim.CompanyMasterId, 0), 0) AS 'BooksCustomerMasterId',
						O.OrganizationTypeId,
						O.OrganizationDomainId,
						OD.[Name] AS 'OrganizationDomainName',
						O.IsActive
		FROM		Enterprise.PartyRelationship pr
						INNER JOIN Enterprise.Party pp ON (pr.PartyIdFrom = pp.PartyId)
						INNER JOIN Person.Person p ON (pp.PartyId = p.PartyId)
						INNER JOIN Ident.UserLogin UL ON p.PartyId = UL.PersonPartyId
						INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId 
						INNER JOIN Person.Persona PE ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId and pr.PartyIdTo = ULP.OrganizationPartyId
						INNER JOIN Enterprise.Organization o ON (pr.PartyIdTo = o.PartyId)
						INNER JOIN Enterprise.OrganizationDomain OD ON (OD.OrganizationDomainId = O.OrganizationDomainId)
						INNER JOIN Enterprise.Party po ON (o.PartyId = po.PartyId)
						INNER JOIN Enterprise.[RelationshipType] rt ON (pr.PartyRelationshipTypeId = rt.RelationshipTypeId)
						INNER JOIN Enterprise.RoleType rtf ON (pr.RoleTypeIdFrom = rtf.PartyRoleTypeId)
						INNER JOIN Enterprise.RoleType rtt ON (pr.RoleTypeIdTo = rtt.PartyRoleTypeId)
						LEFT OUTER JOIN Enterprise.VW_DataImportMapping edim ON (o.PartyId = edim.PartyId)
		WHERE	
			pp.RealPageId = @RealPageId
			AND @NOW >= pr.FromDate AND pr.ThruDate IS NULL
	END
	ELSE
	BEGIN
		SELECT	DISTINCT
						o.PartyId,
						o.Name,
						po.RealPageId,
						po.CreateDate,
						rtf.Name RoleNameFrom,
						rtt.Name RoleNameTo,
						rt.Name AS RelationshipType,
						ULP.PrimaryOrganization,
						COALESCE(ISNULL(edim.MasterId, 0), 0) AS 'BooksMasterId',
						COALESCE(ISNULL(Edim.CompanyMasterId, 0), 0) AS 'BooksCustomerMasterId',
						O.OrganizationTypeId,
						O.OrganizationDomainId,
						OD.[Name] AS 'OrganizationDomainName',
						O.IsActive
		FROM		Enterprise.PartyRelationship pr
						INNER JOIN Enterprise.Party pp ON (pr.PartyIdFrom = pp.PartyId)
						INNER JOIN Person.Person p ON (pp.PartyId = p.PartyId)
						INNER JOIN Ident.UserLogin UL ON p.PartyId = UL.PersonPartyId
						INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId 
						INNER JOIN Person.Persona PE ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId and pr.PartyIdTo = ULP.OrganizationPartyId
						INNER JOIN Enterprise.Organization o ON (pr.PartyIdTo = o.PartyId)
						INNER JOIN Enterprise.OrganizationDomain OD ON (OD.OrganizationDomainId = O.OrganizationDomainId)
						INNER JOIN Enterprise.Party po ON (o.PartyId = po.PartyId)
						INNER JOIN Enterprise.[RelationshipType] rt ON (pr.PartyRelationshipTypeId = rt.RelationshipTypeId)
						INNER JOIN Enterprise.RoleType rtf ON (pr.RoleTypeIdFrom = rtf.PartyRoleTypeId)
						INNER JOIN Enterprise.RoleType rtt ON (pr.RoleTypeIdTo = rtt.PartyRoleTypeId)
						LEFT OUTER JOIN Enterprise.VW_DataImportMapping edim ON (o.PartyId = edim.PartyId)
		WHERE	
			pp.RealPageId = @RealPageId
			AND rt.Name = @RelationshipTypeName
			AND @NOW >= pr.FromDate AND pr.ThruDate IS NULL
	END
END;