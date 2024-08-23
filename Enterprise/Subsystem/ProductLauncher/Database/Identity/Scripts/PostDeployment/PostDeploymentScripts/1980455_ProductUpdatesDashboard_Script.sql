Go

IF NOT EXISTS(select TOP 1 1 from Enterprise.ProductRight  where ProductId = 98 and RightShortName = 'EditOwnProfile')
BEGIN
 INSERT INTO Enterprise.ProductRight(ProductId, RightShortName,DependantProductId)
 VALUES(98,'EditOwnProfile',NULL)
END

Go