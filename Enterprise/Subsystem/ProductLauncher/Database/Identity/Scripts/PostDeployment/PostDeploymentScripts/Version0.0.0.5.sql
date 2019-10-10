EXEC [Enterprise].[CreateProductType] 
	 @ProductTypeId = 500   
	,@ParentProductTypeId = NULL   
	,@Name = 'Platform Services'  
	,@Description = 'Platform Services'  
	,@ProductTypeGUID = '96F29A46-1CDD-4BDF-8B26-617DCD9A7D02'

EXEC [Enterprise].[CreateProductType] 
	 @ProductTypeId = 501   
	,@ParentProductTypeId = 500   
	,@Name = 'Client Portal'  
	,@Description = 'Client Portal'  
	,@ProductTypeGUID = '276A03C1-4A3A-45A9-896B-431246511A7B'

EXEC [Enterprise].[CreateProductType] 
	 @ProductTypeId = 502   
	,@ParentProductTypeId = 500   
	,@Name = 'User Management'  
	,@Description = 'User Management'  
	,@ProductTypeGUID = '76E4E0F3-7117-4C39-B828-C7C95DD22F80'

EXEC [Enterprise].[UpdateProduct]
	 @ProductID = 14
	,@ProductGUID = '054e24b5-005f-4c76-b15d-2f661f10f407'
	,@Name = 'Client Portal'
	,@Description = 'ClientPortal'
	,@ProductTypeId = 501

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='6'