Create PROCEDURE [Enterprise].[ListEmployeeProductsByOrganization]
(
	@LoginName varchar(256),
	@PartyId  bigint,
	@ProductStatusValue int = NULL
)
AS
BEGIN
	     DECLARE @NOW DATETIME= GETUTCDATE();
		 
		 Declare   @RealPageId UniqueIdentifier,
                   @Name Varchar(256),
				   @PersonaId  BIGINT,
				   @PersonPartyId bigint,
				   @EmployeePersonaId bigint;

		 Declare @CompanyOrganizationProduct TABLE ( ProductId INT, ProductStatus varchar(100) NULL ) 
		 Declare @EmployeeADGroupProduct TABLE ( ProductId INT ) 

		 SELECT @PersonPartyId = UL.PersonPartyId,@PersonaId = per.PersonaId
		 FROM Ident.UserLogin UL
		 INNER JOIN Ident.UserLoginPersona ULP ON UL.UserId = ULP.UserLoginId
		 INNER JOIN  Person.Persona per ON ULP.UserLoginPersonaId = per.UserLoginPersonaId
		 WHERE UL.LoginName = @LoginName
		 AND ULP.OrganizationPartyId = @PartyId
		 
		 SELECT @EmployeePersonaId = per.PersonaId
		 FROM Ident.UserLogin UL
		 INNER JOIN Ident.UserLoginPersona ULP ON UL.UserId = ULP.UserLoginId
		 INNER JOIN  Person.Persona per ON ULP.UserLoginPersonaId = per.UserLoginPersonaId
		 WHERE UL.LoginName = @LoginName
		 AND ULP.PrimaryOrganization = 1

		 SELECT @RealPageId = p.RealPageId, @Name = O.Name
		 FROM Enterprise.Organization O
		 INNER JOIN Enterprise.Party P ON O.PartyId = P.PartyId
		 WHERE O.PartyId = @PartyId
			
		 --Org Products
		 INSERT INTO @CompanyOrganizationProduct ( ProductId )
			SELECT ProductId from Enterprise.OrganizationProduct 
			WHERE PartyId = @PartyId
			And ProductId NOT IN (3,36,56)
			AND ((@NOW BETWEEN FromDate AND ThruDate) OR (@NOW >= FromDate	AND ThruDate IS NULL))

		--AD Groups
		INSERT INTO @EmployeeADGroupProduct ( ProductId )
		SELECT DISTINCT adgp.ProductId 
		FROM Security.ADGroup adg
		INNER JOIN Security.ADGroupProduct adgp on
			adg.ADGroupId = adgp.ADGroupId
		INNER JOIN Security.ADGroupUser adgu on
			adg.ADGroupId = adgu.ADGroupId
		where adgu.PersonaId = @EmployeePersonaId

	  --Persona Product status
	  IF (@PersonaId > 0 AND @PersonaId IS NOT NULL)
	  Begin
	   Update OP SET ProductStatus = p.StatusTypeId
	   FROM Enterprise.PersonaConfiguration p
			INNER JOIN @CompanyOrganizationProduct OP ON OP.ProductId = p.ProductId
         WHERE p.PersonaId = @PersonaId
               AND ((@NOW BETWEEN p.FromDate AND p.ThruDate)
                    OR (@NOW >= p.FromDate
                        AND p.ThruDate IS NULL))
               AND (p.StatusTypeId = @ProductStatusValue
                    OR @ProductStatusValue IS NULL);

			--Persona Product Centers
			INSERT INTO @CompanyOrganizationProduct ( ProductId )   
			  SELECT DISTINCT ps.ProductId      
			  FROM Enterprise.PersonaProductCenter ppc     
			  inner join Enterprise.ProductProductCenter p on ppc.ProductCenterId = p.ProductCenterId    
			  inner join Enterprise.GlobalProductConfiguration gpc on gpc.ProductId = p.productId    
			  inner join Enterprise.ProductConfiguration config on config.ConfigurationID = gpc.ConfigurationID   
			  inner join Enterprise.ProductSetting ps on ps.ProductSettingId = config.ProductSettingId    
			  inner join Enterprise.ProductSettingType pst on (ps.ProductSettingTypeId = pst.ProductSettingTypeId and pst.Name ='GetUserProductCenterEnabled')   
			  inner join Enterprise.ProductUserDependency pud on p.ProductId = pud.ProductId  
			  inner join Enterprise.PersonaConfiguration PC on pc.ProductId = pud.DependentProductId  
			  WHERE ps.Value = 1    
			  and ppc.PersonaId = @PersonaId    
			  and gpc.ThruDate is null    
			  and config.ThruDate is null    
			  and ps.ThruDate is null  
				 AND PC.ThruDate IS NULL   
			  AND (PC.StatusTypeId = @ProductStatusValue OR @ProductStatusValue IS NULL)  
		End		

		IF EXISTS ( SELECT TOP 1 1 FROM @CompanyOrganizationProduct Where ProductID = 4 )
		BEGIN
			INSERT INTO @CompanyOrganizationProduct ( ProductId )
				Select ProductId from Enterprise.Product where ProductTypeId IN ( SELECT ProductTypeId FROM Enterprise.ProductType where ParentProductTypeId = 400 )
		END	
		
		
		 SELECT DISTINCT
                prod.ProductGUID,
                prod.ProductId,
                prod.[Name] AS ProductName,
                prod.ProductTypeId,
                prod.Description AS ProductDescription,
                ISNULL(@PersonaId, 0) AS PersonaId,
                ISNULL(@PersonPartyId, 0) AS PersonPartyId,
                @RealPageId AS RealPageId,
                @PartyId AS OrganizationPartyId,
                @Name AS OrganizationName,
              	CASE WHEN (ISNULL(EAD.ProductId, 0) > 0 AND ISNULL(COP.ProductStatus, 0) = 0) THEN '5'
				WHEN (ISNULL(EAD.ProductId, 0) > 0 AND COP.ProductStatus = 8) THEN '8'
				WHEN (ISNULL(EAD.ProductId, 0) = 0 AND COP.ProductStatus = 8) THEN '24'
				WHEN ISNULL(EAD.ProductId, 0) = 0  THEN '24'
				ELSE COP.ProductStatus END As ProductStatus
		FROM @CompanyOrganizationProduct COP
		INNER JOIN Enterprise.Product prod ON prod.ProductId = COP.ProductId and prod.Active = 1
		LEFT OUTER JOIN @EmployeeADGroupProduct EAD ON COP.ProductId = EAD.ProductId
END