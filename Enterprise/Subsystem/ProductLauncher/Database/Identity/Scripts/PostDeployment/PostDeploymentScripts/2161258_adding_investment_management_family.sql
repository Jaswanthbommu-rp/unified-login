Go

IF NOT EXISTS(select TOP 1 1 from Enterprise.ProductType  where ProductTypeId = 800)
BEGIN
 INSERT INTO Enterprise.ProductType(ParentProductTypeId, [Name],[Description],ProductTypeGuid )
 VALUES(NULL,'Investment Management','Investment Management','B3E956D7-98BB-48AF-B11D-773D07A69518')
END

Go