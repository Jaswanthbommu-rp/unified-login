CREATE PROCEDURE [Enterprise].[GetProductsUserAccessSummary]
	@OrganizationPartyId INT,
	@PersonaId BIGINT,
	@ExcludeProductList ProductIdType READONLY,
	@OnlyProductList ProductIdType READONLY
AS
BEGIN	

 	CREATE TABLE #CompanyProductList (Productid INT NOT NULL )
	INSERT INTO #CompanyProductList (productid)
	SELECT DISTINCT op.productid FROM Enterprise.OrganizationProduct OP 
				INNER JOIN Enterprise.GlobalProductConfiguration gpc ON gpc.ProductId = OP.ProductId AND op.ConfigurationId = gpc.ConfigurationId
			WHERE op.PartyId = @OrganizationPartyId AND op.ThruDate IS null AND gpc.ThruDate IS NULL

	DROP TABLE IF EXISTS #TempSharedProducts 
    create table #TempSharedProducts(ProductConfigurationId int,ConfigurationId int,[Name] nvarchar(200),[value] nvarchar(25),SensitiveData tinyint,
    ProductId int ,BooksProductCode nvarchar(20) ,ProductName nvarchar(200) ,Active bit)
    insert into #TempSharedProducts(ProductConfigurationId,ConfigurationId,[Name],[value],SensitiveData,ProductId,BooksProductCode,ProductName,Active)
    exec [Enterprise].[ListProductGlobalSettingsBySettingType] 'SharedProductId'


   DROP TABLE IF EXISTS #DependentProducts    
   CREATE TABLE #DependentProducts (ProductId int,BaseProductId int)      
   INSERT INTO #DependentProducts (ProductId ,BaseProductId)
   SELECT DISTINCT PS.ProductId,Ps.[Value] FROM #TempSharedProducts PS
   INNER JOIN #CompanyProductList COP on COP.ProductId <> PS.[Value] and PS.ProductId = COP.ProductId

   INSERT INTO #CompanyProductList
   SELECT BaseProductId FROM #DependentProducts 

	
	IF EXISTS (SELECT TOP (1) 1 FROM @OnlyProductList )
	BEGIN
		DELETE FROM #CompanyProductList WHERE productid NOT IN (SELECT ProductId FROM @OnlyProductList)
	END
	
	IF EXISTS ( SELECT TOP (1) 1 FROM #CompanyProductList WHERE productid = 4 )
		BEGIN
			INSERT INTO #CompanyProductList (productid)
				SELECT productid FROM Enterprise.Product WHERE UDMSourceCode = 'AO'
			DELETE FROM #CompanyProductList WHERE productid = 4
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
			INNER JOIN #CompanyProductList cp ON cp.productid = pc.ProductId
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
