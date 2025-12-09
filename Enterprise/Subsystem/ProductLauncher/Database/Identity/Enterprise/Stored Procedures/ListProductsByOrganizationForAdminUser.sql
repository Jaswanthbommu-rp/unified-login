CREATE PROCEDURE [Enterprise].[ListProductsByOrganizationForAdminUser]    
(    
 @OrganizationRealPageId UNIQUEIDENTIFIER = NULL,  
 @PartyId BIGINT = NULL  
)    
    
AS    
    
BEGIN    
   
	 DECLARE @NOW  DATETIME = GETUTCDATE();
	 Declare @OrgType varchar (50)
	 Declare @products Table (
		ProductGUID uniqueidentifier,
		ProductId int,
		ProductName varchar(255),
		SolutionId int,
		Solution varchar(255),
		FamilyId int,
		Family  nvarchar(255),
		ProductDescription nvarchar(1000),
		Active bit,
		ProductAvaliableForOrgType varchar(255),
		ShowInUserDetails bit,
		ProductCode varchar(10),
		UDMSourceCode varchar(10)
	 )
   
	   Select @OrgType = ot.Name FROM [Enterprise].Organization o
	   Join Enterprise.OrganizationType ot ON ot.OrganizationTypeId = o.OrganizationTypeId
	   JOIN Enterprise.Party par ON par.PartyId = o.PartyId  
	   WHERE (par.RealPageId = @OrganizationRealPageId OR @OrganizationRealPageId IS NULL)  
	   AND (Par.PartyId = @PartyId OR @PartyId IS NULL) 

		Insert into @products
		SELECT DISTINCT pr.ProductGUID ,    
				pr.ProductId ,    
				pr.[Name] AS ProductName ,    
				pts.ProductTypeId AS SolutionId ,    
				pts.[Name] AS Solution ,    
				ptf.ProductTypeId AS FamilyId ,    
				ptf.[Name] AS Family ,    
				pr.Description AS ProductDescription,  
				pr.[Active],
				'Multifamily',
				0,
				pr.BooksProductCode,
				pr.UDMSourceCode
		FROM [Enterprise].Organization o
				JOIN [Enterprise].OrganizationProduct op ON op.PartyId = o.PartyId   			
				JOIN [Enterprise].[Product] pr ON pr.ProductId = op.ProductId    
				LEFT JOIN [Enterprise].[ProductType] pts ON pts.ProductTypeId = pr.ProductTypeId    
				LEFT JOIN [Enterprise].[ProductType] ptf ON ptf.ProductTypeId = pts.ParentProductTypeId    
				JOIN Enterprise.Party par ON par.PartyId = o.PartyId    
		WHERE (par.RealPageId = @OrganizationRealPageId OR @OrganizationRealPageId IS NULL)  
		AND (Par.PartyId = @PartyId OR @PartyId IS NULL)  
		AND ((@NOW BETWEEN op.FromDate AND op.ThruDate) OR (@NOW >= op.FromDate AND op.ThruDate IS NULL))   

		--if ao product exists then get all AO sub products
		IF EXISTS (Select 1 From @products Where ProductId = 4)
		Begin
				Insert into @products
				 SELECT DISTINCT pr.ProductGUID ,    
					pr.ProductId ,    
					pr.[Name] AS ProductName ,    
					pts.ProductTypeId AS SolutionId ,    
					pts.[Name] AS Solution ,    
					ptf.ProductTypeId AS FamilyId ,    
					ptf.[Name] AS Family ,    
					pr.Description AS ProductDescription,  
					pr.[Active],
					'Multifamily',
					0,
					pr.BooksProductCode,
					pr.UDMSourceCode
				FROM [Enterprise].[Product] pr   
					LEFT JOIN [Enterprise].[ProductType] pts ON pts.ProductTypeId = pr.ProductTypeId    
					LEFT JOIN [Enterprise].[ProductType] ptf ON ptf.ProductTypeId = pts.ParentProductTypeId 
				Where pr.UDMSourceCode = 'AO'
		End

		Update t SET t.ShowInUserDetails = ps.Value
        FROM	Enterprise.GlobalProductConfiguration gpc
				Join @products t ON t.ProductId = gpc.ProductId
				JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
				JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
				JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
        WHERE  pst.Name = 'ShowInUserDetails'
				AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))
				AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))
				AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))

	Select  ProductGUID ,
			ProductId ,
			ProductName ,
			SolutionId ,
			Solution ,
			FamilyId ,
			Family  ,
			ProductDescription,
			Active ,
			ProductCode,
			UDMSourceCode
	From @products 
	Where ShowInUserDetails = 1 
	And ProductId <> 3 
END;
