
-- =============================================
-- Author:		Jhuman Chhetri
-- Create date: 
-- Description: Gets the product look up from ProductType, ControlType and ProductPageType
-- =============================================
CREATE procedure [Enterprise].[ProductLookup]  
as    
 select ProductTypeId, Name, Description from Enterprise.ProductType with (nolock) where ParentProductTypeId is null;  
 select ControlTypeId, Name, Description from [UserManagement].[ControlType] with (nolock);  
 select ProductPageTypeId, value, Description from [UserManagement].[ProductPageType] with (nolock);  