IF OBJECT_ID('[Ident].[GetProductsByPersonaId]') IS NOT NULL
	DROP PROCEDURE [Ident].[GetProductsByPersonaId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[GetProductsByPersonaId] (
	@personaId bigint,
	@productId int = 0
	, @productSelectType nvarchar(10) = null -- Added by Gilbert so that product can be filtered by "type" (IsFavorite, IsResource)
)
AS
BEGIN

	DECLARE @NOW DATETIME = GETUTCDATE();

	SELECT	pvt.PersonaId,
			pvt.PersonPartyId,
			pvt.RealPageId,
			pvt.OrganizationPartyId,
			pvt.OrganizationName,
			pvt.ProductId,
			pvt.ProductName,
			pvt.ProductDescription,
			pvt.PersonPartyId,
			pvt.TotalAccounts,
			pvt.ClientId,
			pvt.ClassName,
			pvt.SettingsUrl,
			pvt.ProductUrl,
			pvt.TitleId,
			CONVERT(uniqueidentifier, CASE WHEN LEN(LTRIM(RTRIM(pvt.TitleUniqueId))) = 0 THEN NULL ELSE pvt.TitleUniqueId END ) AS TitleUniqueId,
			CONVERT(tinyint, pvt.IsNewTab) AS IsNewTab,
			pvt.MetatagUniqueId,
			CONVERT(tinyint, pvt.IsResource) AS IsResource,
			CONVERT(tinyint, pvt.IsFavorite) AS IsFavorite,
			pvt.Subsolution,
			pvt.Family,
			pvt.FamilyId,
			pvt.SolutionId,
			pvt.Solution,
			pvt.LearnMore
	FROM	(
		SELECT	p.PersonaId,
				pa.RealPageId,
				o.PartyId AS OrganizationPartyId,
				o.Name AS OrganizationName,
				pr.ProductId,
				pr.Name AS ProductName,
				pr.Description AS ProductDescription,
				p.PersonPartyId AS PersonPartyId,
				pst.Name AS ProductSettingTypeName,
				1 AS TotalAccounts,
			
	ps.Value,
				pts.ProductTypeId as SolutionId, 
				pts.Name as Solution, 
				ptf.ProductTypeId AS FamilyId,  
				ptf.Name as Family
		FROM	Person.Persona p
				INNER JOIN Enterprise.Party pa ON (p.PersonPartyId = pa.PartyId)
				INNER JOIN Enterprise.Organization o ON (p.OrganizationPartyId = o.PartyId)
				INNER JOIN Ident.SamlUserAttribute sua ON (p.PersonaId = sua.PersonaId)
				INNER JOIN Enterprise.Product pr ON (sua.ProductId = pr.ProductId)
				INNER JOIN Enterprise.ProductSetting ps ON (pr.ProductId = ps.ProductId)
				INNER JOIN Enterprise.ProductSettingType pst ON (ps.ProductSettingTypeId = pst.ProductSettingTypeId)
				-- Changed to left join so that products with no parents (IsResource type are included in result set)

				LEFT JOIN Enterprise.ProductType pts ON pts.ProductTypeId = pr.ProductTypeId                    
				LEFT JOIN Enterprise.ProductType ptf ON ptf.ProductTypeId = pts.ParentProductTypeId
				-- GILBERT: PLEASE ADD MISSING JOINS TO PERSONA PRODUCT CONFIGURATIONS TO GET PERSONA PRODUCT FAVORITES
		WHERE	p.PersonaId = @personaId
		AND		(@productId = 0 OR pr.ProductId = @productId)
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
	) p
	PIVOT	(
		MAX(Value) FOR ProductSettingTypeName IN (
			[ClientId],
			[ClassName],
			[SettingsUrl],
			[ProductUrl],
		
	[TitleId],
			[TitleUniqueId],
			[IsNewTab],
			[MetatagUniqueId],
			[IsResource],
			[IsFavorite],
			[Subsolution],
			[LearnMore]
		)
	) AS pvt

	-- Added by Gilbert so that product can be filtered by "type" (IsFavorite, IsResource)
	WHERE	((@productSelectType IS NULL AND pvt.IsResource IS NULL) OR pvt.IsResource = 0)
			OR
			(@productSelectType IS NOT NULL AND @productSelectType = 'ProductWithFavorites' AND pvt.IsResource IS NULL OR pvt.IsResource = 0)
			OR 
			(@productSelectType IS NOT NULL AND @productSelectType = 'IsResource' AND pvt.IsResource = 1)
			OR
			(@productSelectType IS NOT NULL AND @productSelectType = 'IsFavorite' AND pvt.IsFavorite = 1)
	
	;
END
GO
