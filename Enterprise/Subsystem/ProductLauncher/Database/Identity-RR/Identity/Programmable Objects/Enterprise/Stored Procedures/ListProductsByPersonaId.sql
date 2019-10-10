IF OBJECT_ID('[Enterprise].[ListProductsByPersonaId]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListProductsByPersonaId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListProductsByPersonaId] (  
    @PersonaId bigint  
)  
AS  
BEGIN  
	DECLARE @NOW DATETIME = GETUTCDATE();  
  
	SELECT	DISTINCT
			prod.ProductGUID,  
			p.ProductId,  
			prod.[Name] AS ProductName,  
			prod.ProductTypeId,  
			prod.Description AS ProductDescription,
			per.PersonaId,
			per.PersonPartyId,
			par.RealPageId,
			o.PartyId AS OrganizationPartyId,
			o.Name AS OrganizationName
	FROM	Enterprise.PersonaConfiguration p  
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = p.ConfigurationId  
			JOIN Enterprise.Product prod ON prod.ProductId = p.ProductId
			INNER JOIN Person.Persona per ON (p.PersonaId = per.PersonaId)
			INNER JOIN Enterprise.Party par ON (per.PersonPartyId = par.PartyId)
			INNER JOIN Enterprise.Organization o ON (per.OrganizationPartyId = o.PartyId)
	WHERE	p.PersonaId = @PersonaId  
	AND		((@NOW BETWEEN p.FromDate AND p.ThruDate) OR (@NOW >= p.FromDate AND p.ThruDate IS NULL))  
	AND		((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
END;
GO
