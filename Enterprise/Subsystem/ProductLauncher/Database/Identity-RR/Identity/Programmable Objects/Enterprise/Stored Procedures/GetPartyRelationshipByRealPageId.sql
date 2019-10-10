IF OBJECT_ID('[Enterprise].[GetPartyRelationshipByRealPageId]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[GetPartyRelationshipByRealPageId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[GetPartyRelationshipByRealPageId]
	@RealPageIdFrom UNIQUEIDENTIFIER,
	@RealPageIdTo UNIQUEIDENTIFIER,
	@RoleTypeName varchar(50) = NULL,
	@RelationshipTypeName varchar(50) = NULL
AS
BEGIN
DECLARE @NOW DATETIME
SELECT @NOW = GETUTCDATE()
	SELECT	pr.PartyRelationshipId,
			pr.PartyIdFrom,
			pf.RealPageId AS RealPageIdFrom,  
			pr.PartyIdTo,
			pt.RealPageId AS RealPageIdTo,  
			pr.RoleTypeIdFrom,
			pr.RoleTypeIdTo,
			pr.PartyRelationshipTypeId,
			pr.FromDate,
			pr.ThruDate
	FROM	Enterprise.PartyRelationship pr
			INNER JOIN Enterprise.Party pf ON (pr.PartyIdFrom = pf.PartyId)
			INNER JOIN Enterprise.Party pt ON (pr.PartyIdTo = pt.PartyId)
			INNER JOIN Enterprise.RoleType rtf ON (pr.RoleTypeIdFrom = rtf.PartyRoleTypeId)
			INNER JOIN Enterprise.[RelationshipType] rt ON (pr.PartyRelationshipTypeId = rt.RelationshipTypeId)			
			LEFT OUTER JOIN Enterprise.RoleType prt ON (rtf.ParentPartyRoleTypeId = prt.PartyRoleTypeId)
	WHERE	pf.RealPageId = @RealPageIdFrom
	AND		pt.RealPageId = @RealPageIdTo
	AND		(@RelationshipTypeName IS NULL OR rt.Name = @RelationshipTypeName)
	AND		(@RoleTypeName IS NULL OR prt.Name = @RoleTypeName)
	AND ((@NOW BETWEEN pr.FromDate AND pr.ThruDate) OR (@NOW >= pr.FromDate AND pr.ThruDate IS NULL));
END
GO
