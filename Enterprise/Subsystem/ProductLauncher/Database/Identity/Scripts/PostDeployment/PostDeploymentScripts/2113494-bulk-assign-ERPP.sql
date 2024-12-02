IF NOT EXISTS (Select 1 From [Batch].[BatchProcessType] Where BatchProcessTypeId = 15)
  BEGIN
	 Insert Into [Batch].[BatchProcessType]( BatchProcessTypeId,BatchProcessConfigurationId,[Description],[Name])
	 Select 15,2,'Batch to create EnterpriseRole Create-Update User in bulk','BulkAddUpdateEnterpriseRole'
  END
GO