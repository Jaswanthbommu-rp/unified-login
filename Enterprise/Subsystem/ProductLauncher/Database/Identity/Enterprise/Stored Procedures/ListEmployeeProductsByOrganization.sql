Create PROCEDURE [Enterprise].[ListEmployeeProductsByOrganization]
(
	@PersonaId  BIGINT,
	@ProductStatusValue int = NULL
)
AS
BEGIN
	     DECLARE @NOW DATETIME= GETUTCDATE();
		 
		 Declare   @RealPageId UniqueIdentifier,
                   @Name Varchar(256),
				   @PartyId  bigint,
				   @PersonPartyId bigint,
				   @LoginName varchar(256),
				   @EmployeePersonaId bigint;

		 Declare @CompanyOrganizationProduct TABLE ( ProductId INT, ProductStatus varchar(100) NULL, SupportsEmployeeCreation bit  ) 
		 Declare @EmployeeADGroupProduct TABLE ( ProductId INT ) 

		 SELECT @PersonPartyId = UL.PersonPartyId,
				@PartyId =ULP.OrganizationPartyId,
				@LoginName = UL.LoginName
		 FROM Ident.UserLogin UL
		 INNER JOIN Ident.UserLoginPersona ULP ON UL.UserId = ULP.UserLoginId
		 INNER JOIN  Person.Persona per ON ULP.UserLoginPersonaId = per.UserLoginPersonaId
		 WHERE per.PersonaId = @PersonaId
		 
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
		INSERT INTO @CompanyOrganizationProduct ( ProductId , SupportsEmployeeCreation )
		SELECT distinct ProductId , 0 from Enterprise.OrganizationProduct 
		WHERE PartyId = @PartyId
		And ProductId NOT IN (3,36,56,67,46)
		AND ((@NOW BETWEEN FromDate AND ThruDate) OR (@NOW >= FromDate	AND ThruDate IS NULL))

		--AD Groups
		INSERT INTO @EmployeeADGroupProduct ( ProductId )
		SELECT DISTINCT adgp.ProductId 
		FROM Security.ADGroup adg
		INNER JOIN Security.ADGroupProduct adgp on
			adg.ADGroupId = adgp.ADGroupId
		INNER JOIN Security.ADGroupUser adgu on
			adg.ADGroupId = adgu.ADGroupId
		where adgu.PersonaId = @EmployeePersonaId  and adg.IsActive = 1

	  --Persona Product status
	  IF (@PersonaId > 0 AND @PersonaId IS NOT NULL)
	  Begin
		   Update OP SET ProductStatus = p.StatusTypeId
		   FROM Enterprise.PersonaConfiguration p
				INNER JOIN @CompanyOrganizationProduct OP ON OP.ProductId = p.ProductId
		   WHERE p.PersonaId = @PersonaId
		   AND ((@NOW BETWEEN p.FromDate AND p.ThruDate) OR (@NOW >= p.FromDate AND p.ThruDate IS NULL));       

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
			  WHERE ps.Value = '1'    
			  and ppc.PersonaId = @PersonaId    
			  and gpc.ThruDate is null    
			  and config.ThruDate is null    
			  and ps.ThruDate is null  
				 AND PC.ThruDate IS NULL   
			  AND (PC.StatusTypeId = @ProductStatusValue OR @ProductStatusValue IS NULL)  
		End		

		IF EXISTS ( SELECT TOP 1 1 FROM @CompanyOrganizationProduct Where ProductID = 4 )
		BEGIN
			INSERT INTO @CompanyOrganizationProduct ( ProductId, SupportsEmployeeCreation )
				Select ProductId,0 from Enterprise.Product where ProductTypeId IN ( SELECT ProductTypeId FROM Enterprise.ProductType where ParentProductTypeId = 400 )
		END	
		
		--Product is enabled for user creation (SI_SupportsEmployeeCreation)
		Update COP Set SupportsEmployeeCreation = ISNULL(ps.Value,0)
		FROM @CompanyOrganizationProduct COP
			JOIN Enterprise.GlobalProductConfiguration gpc ON gpc.ProductId = COP.ProductId
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
		WHERE pst.Name = 'SI_SupportsEmployeeCreation'
		AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))
		AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
		
		--product Status logic
		--AD Group and SupportsEmployeeCreation and product is not assigned then status is 5
		--AD Group and SupportsEmployeeCreation and product is  assigned then status is 8
		--AD Group and SupportsEmployeeCreation and product is assignment errored out then status is 7
		--AD Group is not assigned and SupportsEmployeeCreation  then status is 24
		--AD Group is assigned and NO SupportsEmployeeCreation  then status is 24
		--AD Group is not assigned and NO SupportsEmployeeCreation  then status is 24
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
              	CASE WHEN (ISNULL(EAD.ProductId, 0) > 0 AND ISNULL(COP.ProductStatus, 0) = 0) AND COP.SupportsEmployeeCreation = 1 THEN '5'
				WHEN (ISNULL(EAD.ProductId, 0) > 0 AND COP.SupportsEmployeeCreation = 1 AND COP.ProductStatus = 8) THEN '8'
				WHEN (ISNULL(EAD.ProductId, 0) > 0 AND COP.SupportsEmployeeCreation = 1 AND COP.ProductStatus = 7) THEN '7'
				WHEN (ISNULL(EAD.ProductId, 0) = 0 AND COP.SupportsEmployeeCreation = 1) THEN '24'
				WHEN (ISNULL(EAD.ProductId, 0) > 0 AND COP.SupportsEmployeeCreation = 0) THEN '24'
				WHEN (ISNULL(EAD.ProductId, 0) = 0 AND COP.SupportsEmployeeCreation = 0) THEN '24' 
				ELSE COP.ProductStatus END As ProductStatus
		FROM @CompanyOrganizationProduct COP
		INNER JOIN Enterprise.Product prod ON prod.ProductId = COP.ProductId and prod.Active = 1
		LEFT OUTER JOIN @EmployeeADGroupProduct EAD ON COP.ProductId = EAD.ProductId
END