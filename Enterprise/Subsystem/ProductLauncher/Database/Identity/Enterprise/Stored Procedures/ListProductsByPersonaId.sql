CREATE PROCEDURE [Enterprise].[ListProductsByPersonaId]
(@PersonaId          BIGINT,
 @ProductStatusValue NVARCHAR(2000) = NULL
)
AS
          BEGIN
         DECLARE @NOW DATETIME= GETUTCDATE();
		 DECLARE @CompanyOrganizationProduct TABLE ( ProductId INT ) 		 

		 INSERT INTO @CompanyOrganizationProduct ( ProductId )
			SELECT ProductId from Enterprise.OrganizationProduct OP 
				INNER JOIN Ident.UserLoginPersona ULP ON ULP.OrganizationPartyId = OP.PartyId
				INNER JOIN Person.Persona per ON (ULP.UserLoginPersonaId = per.UserLoginPersonaId and per.PersonaId = @PersonaId)
					AND ((@NOW BETWEEN op.FromDate AND op.ThruDate)
					OR (@NOW >= op.FromDate
					AND op.ThruDate IS NULL))

		IF EXISTS ( SELECT TOP 1 1 FROM @CompanyOrganizationProduct Where ProductID = 4 )
		BEGIN
			INSERT INTO @CompanyOrganizationProduct ( ProductId )
				Select ProductId from Enterprise.Product where ProductTypeId IN ( SELECT ProductTypeId FROM Enterprise.ProductType where ParentProductTypeId = 400 )
		END		
	    
        SELECT DISTINCT
                prod.ProductGUID,
                p.ProductId,
                prod.[Name] AS ProductName,
                prod.ProductTypeId,
                prod.Description AS ProductDescription,
                per.PersonaId,
                ul.PersonPartyId,
                par.RealPageId,
                o.PartyId AS OrganizationPartyId,
                o.Name AS OrganizationName,
                PS.value AS ProductStatus
         FROM Enterprise.PersonaConfiguration p
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = p.ConfigurationId
			INNER JOIN Enterprise.productSetting PS ON PC.productsettingid = ps.productsettingid
			INNER JOIN Enterprise.ProductSettingType PST ON PS.productsettingtypeid = PST.ProductSettingTypeId AND PST.Name = 'ProductStatus'
			JOIN Enterprise.Product prod ON prod.ProductId = p.ProductId
			INNER JOIN Person.Persona per ON(p.PersonaId = per.PersonaId)
			INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = per.UserLoginPersonaId
			INNER JOIN Ident.UserLogin UL ON UL.UserId = ULP.UserLoginId
			INNER JOIN Enterprise.Party par ON(UL.PersonPartyId = par.PartyId)
			INNER JOIN Enterprise.Organization o ON(ULP.OrganizationPartyId = o.PartyId)
			INNER JOIN @CompanyOrganizationProduct OP ON OP.ProductId = prod.ProductId
         WHERE p.PersonaId = @PersonaId
               AND ((@NOW BETWEEN p.FromDate AND p.ThruDate)
                    OR (@NOW >= p.FromDate
                        AND p.ThruDate IS NULL))
               AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate)
                    OR (@NOW >= pc.FromDate
                        AND pc.ThruDate IS NULL))
               AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate)
                    OR (@NOW >= ps.FromDate
                        AND ps.ThruDate IS NULL))
               AND (ps.Value = @ProductStatusValue
                    OR @ProductStatusValue IS NULL);       
     END;