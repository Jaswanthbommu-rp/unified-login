PRINT 'Update Active Building Product Name to Resident Portal'


UPDATE Enterprise.Product SET Name = 'Resident Portal' WHERE Name = 'Active Building'

EXEC sys.sp_updateextendedproperty @name = N'Build' , @value = '17';
