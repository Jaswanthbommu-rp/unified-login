-- Update UDMSourceCode for ILMLM and ILMLA Products
IF EXISTS(SELECT * FROM Enterprise.Product WHERE ProductId = 40 AND UDMSourceCode IS NULL)
	UPDATE Enterprise.Product SET UDMSourceCode = 'ILMLA' WHERE ProductId = 40
IF EXISTS(SELECT * FROM Enterprise.Product WHERE ProductId = 41 AND UDMSourceCode IS NULL)
	UPDATE Enterprise.Product SET UDMSourceCode = 'ILMLA' WHERE ProductId = 41