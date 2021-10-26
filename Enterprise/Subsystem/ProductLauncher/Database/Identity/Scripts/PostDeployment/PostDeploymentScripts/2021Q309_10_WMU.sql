GO

IF NOT EXISTS (SELECT TOP (1)  1 FROM Enterprise.ProductUserDependency WHERE ProductId = 75 and DependentProductId = 1)
BEGIN
	INSERT INTO Enterprise.ProductUserDependency(ProductId,DependentProductId)
	VALUES(75,1)
END
GO