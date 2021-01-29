
-- =============================================
-- Author:		Jhuman Chhetri
-- Create date: 
-- Description: Gets the product look up from ProductType, ControlType and ProductPageType
-- =============================================
CREATE procedure [Enterprise].[ProductLookup]  
AS    
 SELECT ProductTypeId, Name, Description from Enterprise.ProductType with (nolock) where ParentProductTypeId is null;  
 SELECT ControlTypeId, Name, Description from [UserManagement].[ControlType] with (nolock);  
 SELECT ProductPageTypeId, value, Description from [UserManagement].[ProductPageType] with (nolock);  