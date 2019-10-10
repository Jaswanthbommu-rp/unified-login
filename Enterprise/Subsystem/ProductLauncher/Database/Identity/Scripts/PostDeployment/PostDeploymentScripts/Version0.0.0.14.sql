IF NOT EXISTS(SELECT 1 FROM Enterprise.Product WHERE Name = 'OmniCHannel')
BEGIN
	INSERT INTO Enterprise.Product (ProductId, ProductGUID,       Name, Description, ProductTypeId)
	VALUES (22,   'B5DF3F46-A721-434C-81C2-4DD2FFC25B1A',  'OmniChannel', 'OmniChannel', 300)
END


EXEC sys.sp_updateextendedproperty @name=N'Build', @value='15'
