IF OBJECT_ID('[Enterprise].[ListOrganizationByRealPageId]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListOrganizationByRealPageId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListOrganizationByRealPageId] (
	@RealPageId UNIQUEIDENTIFIER,
	@RelationshipTypeName NVARCHAR(50) = NULL
)
AS  
BEGIN  
	SELECT	DISTINCT
			o.PartyId ,
			o.Name ,
			po.RealPageId ,
			po.CreateDate ,
			rtf.Name RoleNameFrom,
			rtt.Name RoleNameTo,
			rt.Name AS RelationshipType
	FROM	Enterprise.PartyRelationship pr
			INNER JOIN Enterprise.Party pp ON (pr.PartyIdFrom = pp.PartyId)
			INNER JOIN Person.Person p ON (pp.PartyId = p.PartyId)
			INNER JOIN Enterprise.Organization o ON (pr.PartyIdTo = o.PartyId)
			INNER JOIN Enterprise.Party po ON (o.PartyId = po.PartyId)
			INNER JOIN Enterprise.[RelationshipType] rt ON (pr.PartyRelationshipTypeId = rt.RelationshipTypeId)
			INNER JOIN Enterprise.RoleType rtf ON (pr.RoleTypeIdFrom = rtf.PartyRoleTypeId)
			INNER JOIN Enterprise.RoleType rtt ON (pr.RoleTypeIdTo = rtt.PartyRoleTypeId)
	WHERE	pp.RealPageId = @RealPageId
			AND (rt.Name = @RelationshipTypeName OR @RelationshipTypeName IS NULL)   
END;
GO
