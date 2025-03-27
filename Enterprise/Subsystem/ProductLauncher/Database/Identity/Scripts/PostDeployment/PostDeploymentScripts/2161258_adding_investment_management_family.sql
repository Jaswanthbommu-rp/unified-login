Go

IF NOT EXISTS(select TOP 1 1 from Enterprise.ProductType  where ProductTypeId = 800)
BEGIN
 INSERT INTO Enterprise.ProductType(ProductTypeId,ParentProductTypeId, [Name],[Description],ProductTypeGuid )
 VALUES(800, NULL,'Investment Management','Investment Management','B3E956D7-98BB-48AF-B11D-773D07A69518')
END

Go