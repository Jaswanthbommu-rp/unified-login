
-- =============================================
-- Author:		Jhuman Chhetri
-- Create date: 
-- Description: Gets the product look up from ProductType, ControlType and ProductPageType
-- =============================================
CREATE procedure [Enterprise].[ProductLookup]  
AS  
BEGIN
 SELECT ProductTypeId, Name, Description from Enterprise.ProductType with (nolock) where ParentProductTypeId is null ORDER BY [Name];  
 SELECT ControlTypeId, Name, Description from [UserManagement].[ControlType] with (nolock) ORDER BY [Name];  
 SELECT ProductPageTypeId, value, Description from [UserManagement].[ProductPageType] with (nolock) ORDER BY [value];
 SELECT ProductSettingTypeId,Name,Description,SensitiveData from Enterprise.ProductSettingType with (nolock) ORDER BY [Name];
END