CREATE PROCEDURE [Ident].[ListPersonaProductsSamlDetails]
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
		NWPUserType varchar(255) NULL,
		ParentProductTypeId INT NULL,
		ProductEnabled TINYINT NOT NULL,
		LearnerId VARCHAR(255) NULL,
		ManagerId VARCHAR(255) NULL,
		DualRole VARCHAR(255) NULL
	)

	Declare @userStatus varchar(50);
    DECLARE @NOW DATETIME= GETUTCDATE();
	Declare @CompanyOrganizationProduct TABLE ( ProductId INT, ParentProductTypeId INT NULL ) 
 
		 INSERT INTO @CompanyOrganizationProduct ( ProductId )
			SELECT ProductId from Enterprise.OrganizationProduct OP 
				INNER JOIN Ident.UserLoginPersona ULP ON ULP.OrganizationPartyId = OP.PartyId
				INNER JOIN Person.Persona per ON (ULP.UserLoginPersonaId = per.UserLoginPersonaId and per.PersonaId = @PersonaId)
					AND ((@NOW BETWEEN op.FromDate AND op.ThruDate)
					OR (@NOW >= op.FromDate
					AND op.ThruDate IS NULL))

		IF EXISTS ( SELECT TOP 1 1 FROM @CompanyOrganizationProduct Where ProductID = 4 )
		BEGIN
			INSERT INTO @CompanyOrganizationProduct ( ProductId, ParentProductTypeId )
			SELECT ProductId, ParentProductTypeId 
			FROM Enterprise.Product p 
				INNER JOIN Enterprise.ProductType pt on pt.ProductTypeId = p.ProductTypeId and pt.ParentProductTypeId =400
		END		
	    
		SELECT  @userStatus = ST.Name
		   FROM Ident.UserLoginPersona ULP
		       INNER JOIN Person.Persona PP  on ULP.UserLoginPersonaId = PP.UserLoginPersonaId
		       INNER JOIN Enterprise.StatusType ST  on ST.StatusTypeId = ULP.StatusTypeId
		   where PP.PersonaId = @PersonaId

        INSERT INTO @productData(ProductId,ProductName,ProductDescription,ProductStatus, ParentProductTypeId,ProductEnabled)
		SELECT DISTINCT                
                p.ProductId,
                prod.[Name] ,
                prod.Description ,
			    CASE  WHEN  @userStatus ='Disabled' then 'Deactivated' else ST.Name END,
				pt.ParentProductTypeId,
				CASE WHEN op.productid IS NOT NULL THEN 1 ELSE 0 END [ProductEnabled]
				
         FROM Enterprise.PersonaConfiguration p
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = p.ConfigurationId
			INNER JOIN Enterprise.productSetting PS ON PC.productsettingid = ps.productsettingid
			INNER JOIN Enterprise.ProductSettingType PST ON PS.productsettingtypeid = PST.ProductSettingTypeId AND PST.Name = 'ProductStatus'
			INNER JOIN Enterprise.Product prod ON prod.ProductId = p.ProductId
			INNER JOIN Enterprise.ProductType pt on pt.ProductTypeId = prod.ProductTypeId  
			INNER JOIN Person.Persona per ON(p.PersonaId = per.PersonaId)
			
			INNER JOIN [Enterprise].[StatusType] ST ON ST.StatusTypeId = CONVERT(INT,PS.value)	
			LEFT JOIN @CompanyOrganizationProduct OP ON OP.ProductId = prod.ProductId
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
       
	   IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.PersonaConfiguration where PersonaID = @PersonaId and ProductId = 36)
		BEGIN
			INSERT INTO @productData(ProductId,ProductName,ProductDescription,ProductStatus, ParentProductTypeId,ProductEnabled)  
			SELECT saml.ProductId  
			,prod.[Name]  
			,prod.[Description]  
			,(case when @userStatus = 'Disabled' then 'Deactivated' else 'Success' end)
			,pt.ParentProductTypeId  
			,CASE WHEN op.productid IS NOT NULL THEN 1 ELSE 0 END [ProductEnabled]    
			from Ident.SamlUserAttribute saml  
			inner join Enterprise.Product prod on saml.ProductId = prod.ProductId  
			inner join Enterprise.ProductType pt on prod.ProductTypeId= pt.ProductTypeId  
			LEFT JOIN @CompanyOrganizationProduct OP ON OP.ProductId = prod.ProductId  
			where saml.PersonaId = @PersonaId and prod.ProductId = 36  
		END

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

	   Update @productData SET LearnerId = sua.Value  
		From Ident.SamlUserAttribute sua  
		INNER JOIN @productData P ON    P.ProductId = sua.ProductId  
		INNER JOIN Ident.SamlAttribute sa    ON (sua.SamlAttributeId = sa.SamlAttributeId)   
		WHERE sua.PersonaId = @PersonaId  
		And sa.Name = 'LearnerId'  
		AND ((@NOW BETWEEN sua.FromDate AND sua.ThruDate) OR   (@NOW >= sua.FromDate AND sua.ThruDate IS NULL)) 

		Update @productData SET ManagerId = sua.Value  
		From Ident.SamlUserAttribute sua  
		INNER JOIN @productData P ON    P.ProductId = sua.ProductId  
		INNER JOIN Ident.SamlAttribute sa    ON (sua.SamlAttributeId = sa.SamlAttributeId)   
		WHERE sua.PersonaId = @PersonaId  
		And sa.Name = 'ManagerId'  
		AND ((@NOW BETWEEN sua.FromDate AND sua.ThruDate) OR   (@NOW >= sua.FromDate AND sua.ThruDate IS NULL))
		
		Update @productData SET DualRole = sua.Value    
		From Ident.SamlUserAttribute sua    
		INNER JOIN @productData P ON    P.ProductId = sua.ProductId    
		INNER JOIN Ident.SamlAttribute sa    ON (sua.SamlAttributeId = sa.SamlAttributeId)     
		WHERE sua.PersonaId = @PersonaId    
		And sa.Name = 'DualRole'    
		AND ((@NOW BETWEEN sua.FromDate AND sua.ThruDate) OR   (@NOW >= sua.FromDate AND sua.ThruDate IS NULL)) 
	
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

	   Select 
		ProductId
		, ProductName	
		, ProductDescription
		, ProductStatus
		, UserID
		, ProductUserName	
		, PMCID	
		, RoleType	
		, PortalId	
		, OrganizationId	
		, NWPUserType	
		, ParentProductTypeId	
		, ProductEnabled
		, LearnerId
		, ManagerId
		, DualRole
	from @productData	
	   order by ProductEnabled desc,ProductName asc
END;
