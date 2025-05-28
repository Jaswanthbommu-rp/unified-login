CREATE PROCEDURE [Enterprise].[GetProductsUserAccessSummary]
	@OrganizationPartyId INT,
	@PersonaId BIGINT,
	@ExcludeProductList ProductIdType READONLY,
	@OnlyProductList ProductIdType READONLY
AS
BEGIN	

 	DECLARE @companyproductlist TABLE (Productid INT NOT NULL )
	INSERT INTO @companyproductlist (productid)
	SELECT DISTINCT op.productid FROM Enterprise.OrganizationProduct OP 
				INNER JOIN Enterprise.GlobalProductConfiguration gpc ON gpc.ProductId = OP.ProductId AND op.ConfigurationId = gpc.ConfigurationId
			WHERE op.PartyId = @OrganizationPartyId AND op.ThruDate IS null AND gpc.ThruDate IS NULL

   DROP TABLE IF EXISTS #DependentProducts    
   CREATE TABLE #DependentProducts (ProductId int,BaseProductId int)      
   INSERT INTO #DependentProducts (ProductId ,BaseProductId)
   SELECT DISTINCT PS.ProductId,Ps.[Value] FROM Enterprise.productsettingtype PST     
   INNER JOIN Enterprise.ProductSetting PS on PST.productSettingTypeId = PS.productSettingTypeId AND PST.[Name] = 'SharedProductId' 
   INNER JOIN @companyproductlist COP on COP.ProductId <> PS.[Value] and PS.ProductId = COP.ProductId

   INSERT INTO @companyproductlist
   SELECT BaseProductId FROM #DependentProducts 

	
	IF EXISTS (SELECT TOP (1) 1 FROM @OnlyProductList )
	BEGIN
		DELETE FROM @companyproductlist WHERE productid NOT IN (SELECT ProductId FROM @OnlyProductList)
	END
	
	IF EXISTS ( SELECT TOP (1) 1 FROM @companyproductlist WHERE productid = 4 )
		BEGIN
			INSERT INTO @companyproductlist (productid)
				SELECT productid FROM Enterprise.Product WHERE UDMSourceCode = 'AO'
			DELETE FROM @companyproductlist WHERE productid = 4
		END
		
		SELECT DISTINCT 
			pc.personaid [PersonaId], pc.productid [ProductId], p.Name [ProductName], ISNULL(sam.Value, 'Not Used') [ProductUserLogin] 
			FROM Enterprise.PersonaConfiguration pc
			INNER JOIN Enterprise.ProductConfiguration pc2 ON pc2.ConfigurationId = pc.ConfigurationId
			INNER JOIN Enterprise.ProductSetting PS ON PS.ProductSettingId = pc2.ProductSettingId
			INNER JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId
			INNER JOIN Enterprise.Product P ON P.ProductId = pc.ProductId
			LEFT JOIN Ident.SamlUserAttribute sam ON sam.ProductId = pc.ProductId AND sam.PersonaId = pc.PersonaId AND sam.ThruDate IS NULL AND sam.SamlAttributeId = 1
			INNER JOIN Person.Persona Per ON Per.PersonaId = pc.PersonaId
			INNER JOIN Ident.UserLoginPersona ulp ON ulp.UserLoginPersonaId = per.UserLoginPersonaId
			INNER JOIN @companyproductlist cp ON cp.productid = pc.ProductId
		WHERE 
			pc.PersonaId = @PersonaId
			AND pc.ProductId NOT IN (SELECT ProductId FROM @ExcludeProductList)
			AND pst.Name = 'ProductStatus'
			AND ps.Value = '8'
			AND pc.ThruDate IS NULL AND pc2.ThruDate IS NULL
		UNION ALL
		SELECT @PersonaId, 3, 'Unified Platform', 'Not Used'
		ORDER BY p.Name
END
