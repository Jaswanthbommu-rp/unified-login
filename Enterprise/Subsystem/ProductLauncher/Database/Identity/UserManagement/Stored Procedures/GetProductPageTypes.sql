CREATE PROCEDURE [UserManagement].[GetProductPageType]
 AS 
SELECT [ProductPageTypeId]
      ,[Value]
      ,[Description]
  FROM [UserManagement].[ProductPageType] PPT
