CREATE PROCEDURE [Enterprise].[ListProductSettingsByPersonaId] @PersonaId     INT,
                                                              @ProductStatus NVARCHAR(2000) = NULL
AS
     BEGIN
         DECLARE @NOW DATETIME= GETUTCDATE();

          DECLARE @CompanyOrganizationProduct TABLE ( ProductId INT ) 
		 INSERT INTO @CompanyOrganizationProduct ( ProductId )                
         SELECT ProductId FROM Enterprise.OrganizationProduct OP                 
         INNER JOIN Ident.UserLoginPersona ULP ON ULP.OrganizationPartyId = OP.PartyId                
         INNER JOIN Person.Persona per ON ULP.UserLoginPersonaId = per.UserLoginPersonaId and per.PersonaId = @PersonaId
         WHERE @NOW >= op.FromDate AND op.ThruDate IS NULL

         drop table if exists #TempSharedProducts 
         create table #TempSharedProducts(ProductConfigurationId int,ConfigurationId int,[Name] nvarchar(200),[value] nvarchar(25),SensitiveData tinyint,
         ProductId int ,BooksProductCode nvarchar(20) ,ProductName nvarchar(200) ,Active bit)
         insert into #TempSharedProducts(ProductConfigurationId,ConfigurationId,[Name],[value],SensitiveData,ProductId,ProductName,BooksProductCode,Active)
         exec [Enterprise].[ListProductGlobalSettingsBySettingType] 'SharedProductId'


		 DROP TABLE IF EXISTS #DependentProducts
         CREATE TABLE #DependentProducts (ProductId int,BaseProductId int)  
         INSERT INTO #DependentProducts
         SELECT DISTINCT PS.ProductId,Ps.[Value] FROM #TempSharedProducts PS
         INNER JOIN @CompanyOrganizationProduct COP on COP.ProductId <> PS.[Value] and PS.ProductId = COP.ProductId
       
    
         DROP TABLE IF EXISTS #TempFinalResult 
         CREATE TABLE #TempFinalResult([ProductId] [int] NOT NULL,ProductSettingId [int] NOT NULL,[Name] NVARCHAR(200) NULL,[Value] NVARCHAR(1000) NOT NULL)

         INSERT INTO #TempFinalResult ([ProductId],ProductSettingId,[Name],[Value])



         SELECT p.ProductId,
                ps.ProductSettingId,
                pst.Name,
                CASE
				WHEN (pst.Name = 'UsePrimaryProperties')
				THEN p.UsePrimaryProperties
				ELSE ps.value
				END AS Value
         FROM Enterprise.PersonaConfiguration p
              LEFT JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = p.ConfigurationId
              LEFT JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
              LEFT JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
              LEFT JOIN Enterprise.Product prod ON prod.ProductId = p.ProductId
         WHERE p.PersonaId = @PersonaId
               AND (ps.Value = @ProductStatus
                    OR @ProductStatus IS NULL)
               AND ((@NOW BETWEEN p.FromDate AND p.ThruDate)
                    OR (@NOW >= p.FromDate
                        AND p.ThruDate IS NULL))
               AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate)
                    OR (@NOW >= pc.FromDate
                        AND pc.ThruDate IS NULL));

        UPDATE TF set TF.ProductId = DP.ProductId from  #TempFinalResult TF 
		INNER JOIN #DependentProducts DP on DP.BaseProductId  = TF.ProductId

         SELECT DISTINCT [ProductId] ,ProductSettingId ,[Name] ,[Value] from #TempFinalResult

		 DROP TABLE IF EXISTS #TempFinalResult 
     END;
