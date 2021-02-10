Create PROCEDURE [Ident].[ListPersonaProductsSamlDetails]
(@PersonaId          BIGINT)
AS
BEGIN
	Declare @productData table(
		ProductId int,
		ProductName varchar(255),
		ProductDescription varchar(1000),
		ProductStatus varchar(50),
		UserID varchar(255) NULL,
		ProductUserName varchar(255) NULL,
		PMCID varchar(255) NULL,
		RoleType varchar(255) NULL,
		PortalId varchar(255) NULL,
		OrganizationId varchar(255) NULL,
		NWPUserType varchar(255) NULL
	)

    DECLARE @NOW DATETIME= GETUTCDATE();
	Declare @CompanyOrganizationProduct TABLE ( ProductId INT ) 
 
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
	
        INSERT INTO @productData(ProductId,ProductName,ProductDescription,ProductStatus)
		SELECT DISTINCT                
                p.ProductId,
                prod.[Name] ,
                prod.Description ,               
                ST.Name 
         FROM Enterprise.PersonaConfiguration p
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = p.ConfigurationId
			INNER JOIN Enterprise.productSetting PS ON PC.productsettingid = ps.productsettingid
			INNER JOIN Enterprise.ProductSettingType PST ON PS.productsettingtypeid = PST.ProductSettingTypeId AND PST.Name = 'ProductStatus'
			JOIN Enterprise.Product prod ON prod.ProductId = p.ProductId
			INNER JOIN Person.Persona per ON(p.PersonaId = per.PersonaId)
			INNER JOIN @CompanyOrganizationProduct OP ON OP.ProductId = prod.ProductId
			INNER JOIN [Enterprise].[StatusType] ST ON ST.StatusTypeId = CONVERT(INT,PS.value)				
         WHERE p.PersonaId = @PersonaId
               AND ((@NOW BETWEEN p.FromDate AND p.ThruDate)
                    OR (@NOW >= p.FromDate
                        AND p.ThruDate IS NULL))  
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate)
                    OR (@NOW >= ps.FromDate
                        AND ps.ThruDate IS NULL))  
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate)
                    OR (@NOW >= pc.FromDate
                        AND pc.ThruDate IS NULL))  
   

	   Update @productData SET UserID = sua.Value
	   From Ident.SamlUserAttribute sua
	   INNER JOIN @productData P ON
		P.ProductId = sua.ProductId
	   INNER JOIN Ident.SamlAttribute sa 
		ON (sua.SamlAttributeId = sa.SamlAttributeId)	
	   WHERE sua.PersonaId = @PersonaId
	   And sa.Name = 'UserId'
	   AND ((@NOW BETWEEN sua.FromDate AND sua.ThruDate) OR 
		   (@NOW >= sua.FromDate AND sua.ThruDate IS NULL))
	
	   Update @productData SET ProductUserName = sua.Value
	   From Ident.SamlUserAttribute sua
	   INNER JOIN @productData P ON
		P.ProductId = sua.ProductId
	   INNER JOIN Ident.SamlAttribute sa 
		ON (sua.SamlAttributeId = sa.SamlAttributeId)	  
	   WHERE sua.PersonaId = @PersonaId
	   And sa.Name = 'productUsername'
	   AND ((@NOW BETWEEN sua.FromDate AND sua.ThruDate) OR (@NOW >= sua.FromDate AND sua.ThruDate IS NULL))
	
	   Update @productData SET PMCID = sua.Value
	   From Ident.SamlUserAttribute sua
	   INNER JOIN @productData P ON
		P.ProductId = sua.ProductId
	   INNER JOIN Ident.SamlAttribute sa 
		ON (sua.SamlAttributeId = sa.SamlAttributeId)
	   INNER JOIN Ident.SamlAttributeType sat 
		ON (sa.SamlAttributeTypeId = sat.SamlAttributeTypeId)
	   WHERE sua.PersonaId = @PersonaId
	   And sa.Name = 'PMCID'
	   AND ((@NOW BETWEEN sua.FromDate AND sua.ThruDate) OR (@NOW >= sua.FromDate AND sua.ThruDate IS NULL))
   
	   Update @productData SET RoleType = sua.Value
	   From Ident.SamlUserAttribute sua
	   INNER JOIN @productData P ON
		P.ProductId = sua.ProductId
	   INNER JOIN Ident.SamlAttribute sa 
		ON (sua.SamlAttributeId = sa.SamlAttributeId)	 
	   WHERE sua.PersonaId = @PersonaId
	   And sa.Name = 'RoleCode'
	   AND ((@NOW BETWEEN sua.FromDate AND sua.ThruDate) OR (@NOW >= sua.FromDate AND sua.ThruDate IS NULL))
	
	   Update @productData SET PortalId = sua.Value
	   From Ident.SamlUserAttribute sua
	   INNER JOIN @productData P ON
		P.ProductId = sua.ProductId
	   INNER JOIN Ident.SamlAttribute sa 
		ON (sua.SamlAttributeId = sa.SamlAttributeId)	  
	   WHERE sua.PersonaId = @PersonaId
	   And sa.Name = 'portal_id'
	   AND ((@NOW BETWEEN sua.FromDate AND sua.ThruDate) OR (@NOW >= sua.FromDate AND sua.ThruDate IS NULL))
	
	   Update @productData SET OrganizationId = sua.Value
	   From Ident.SamlUserAttribute sua
	   INNER JOIN @productData P ON
		P.ProductId = sua.ProductId
	   INNER JOIN Ident.SamlAttribute sa 
		ON (sua.SamlAttributeId = sa.SamlAttributeId)	  
	   WHERE sua.PersonaId = @PersonaId
	   And sa.Name = 'organization_id'
	   AND ((@NOW BETWEEN sua.FromDate AND sua.ThruDate) OR (@NOW >= sua.FromDate AND sua.ThruDate IS NULL))
	
	   Update @productData SET NWPUserType = sua.Value
	   From Ident.SamlUserAttribute sua
	   INNER JOIN @productData P ON
		P.ProductId = sua.ProductId
	   INNER JOIN Ident.SamlAttribute sa 
		ON (sua.SamlAttributeId = sa.SamlAttributeId)	 
	   WHERE sua.PersonaId = @PersonaId
	   And sa.Name = 'NWPUserType'
	   AND ((@NOW BETWEEN sua.FromDate AND sua.ThruDate) OR (@NOW >= sua.FromDate AND sua.ThruDate IS NULL))
	
	   Select * from @productData	
	   order by ProductName
END;
