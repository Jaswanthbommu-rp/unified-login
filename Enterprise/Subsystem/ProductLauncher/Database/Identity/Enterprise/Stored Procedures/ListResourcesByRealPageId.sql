CREATE PROCEDURE [Enterprise].[ListResourcesByRealPageId]
	@RealPageId UNIQUEIDENTIFIER

AS

	SET NOCOUNT ON;
    DECLARE @products TABLE
    (
        ProductGUID             UNIQUEIDENTIFIER,
	    ProductId   			int,
	    Name                    nvarchar(100), 
		HasAccess               bit,
	    Description 			nvarchar(100),	                   
	    ProductSettingId		int,
	    ProductSettingTypeId	int,
	    SettingName				nvarchar(50),
	    SettingValue			nvarchar(1000),
	    SettingDescription		nvarchar(100)
    )	

    INSERT INTO @products
		SELECT	ep.ProductGUID,
				ep.ProductId, 
				ep.Name, 							
				CASE WHEN epar.RealPageId = @RealPageId THEN 1 ELSE 0 END HasAccess,
				ep.Description, 		                    
				eps.ProductSettingId,
				epst.ProductSettingTypeId,
				epst.Name AS SettingName,
				eps.Value AS SettingValue,
				epst.Description AS SettingDescription	 
		FROM Enterprise.Product ep							
				LEFT JOIN [Enterprise].[ProductSetting] eps ON eps.ProductId = ep.ProductId
				LEFT JOIN [Enterprise].[ProductSettingType] epst ON epst.ProductSettingTypeId = eps.ProductSettingTypeId 
				LEFT JOIN [Ident].[SamlUserAttribute] isua ON isua.ProductId = eps.ProductId
				LEFT JOIN [Enterprise].[Party] epar ON epar.PartyId = isua.[PersonaId] 
				JOIN (
					SELECT ProductId FROM [Enterprise].[ProductSetting] eps2 
						JOIN [Enterprise].[ProductSettingType] epst2 ON epst2.ProductSettingTypeId = eps2.ProductSettingTypeId 
					WHERE eps2.Value = '1' AND epst2.Name='IsResource') epsst ON epsst.ProductId = eps.ProductId;

    SELECT	DISTINCT(ProductId), 
            ProductGUID,		                     
		    Name,                             
		    Description		                    
    FROM	@products;

    SELECT	ProductId, 	
		    ProductSettingId,
		    ProductSettingTypeId,
		    SettingName AS Name,
		    SettingValue AS Value,
		    SettingDescription AS Description,
            HasAccess
    FROM	@products;   
