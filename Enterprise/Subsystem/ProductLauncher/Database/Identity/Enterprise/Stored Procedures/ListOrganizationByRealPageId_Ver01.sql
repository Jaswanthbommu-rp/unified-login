CREATE PROCEDURE [Enterprise].[ListOrganizationByRealPageId_Ver01] (
	@RealPageId uniqueidentifier,
	@RelationshipTypeName nvarchar(50) = NULL
)
AS  
BEGIN
	DECLARE @NOW  DATETIME = GETUTCDATE();

	SELECT	DISTINCT
					o.PartyId,
					o.Name,
					po.RealPageId,
					po.CreateDate,
					rtf.Name RoleNameFrom,
					rtt.Name RoleNameTo,
					rt.Name AS RelationshipType,
					COALESCE(ISNULL(edim.MasterId, 0), 0) AS 'BooksMasterId',
					COALESCE(ISNULL(Edim.CompanyMasterId, 0), 0) AS 'BooksCustomerMasterId'
	FROM		Enterprise.PartyRelationship pr
					INNER JOIN Enterprise.Party pp ON (pr.PartyIdFrom = pp.PartyId)
					INNER JOIN Person.Person p ON (pp.PartyId = p.PartyId)
					INNER JOIN Enterprise.Organization o ON (pr.PartyIdTo = o.PartyId)
					INNER JOIN Enterprise.Party po ON (o.PartyId = po.PartyId)
					INNER JOIN Enterprise.[RelationshipType] rt ON (pr.PartyRelationshipTypeId = rt.RelationshipTypeId)
					INNER JOIN Enterprise.RoleType rtf ON (pr.RoleTypeIdFrom = rtf.PartyRoleTypeId)
					INNER JOIN Enterprise.RoleType rtt ON (pr.RoleTypeIdTo = rtt.PartyRoleTypeId)
					LEFT OUTER JOIN Enterprise.VW_DataImportMapping edim ON (o.PartyId = edim.PartyId)
	WHERE	pp.RealPageId = @RealPageId
	AND			((rt.Name = @RelationshipTypeName) OR (@RelationshipTypeName IS NULL))
	AND			((@NOW BETWEEN pr.FromDate AND pr.ThruDate) OR (@NOW >= pr.FromDate AND pr.ThruDate IS NULL))
END;