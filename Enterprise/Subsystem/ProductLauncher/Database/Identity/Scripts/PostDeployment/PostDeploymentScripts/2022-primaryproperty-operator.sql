IF NOT EXISTS ( SELECT TOP (1) (1) FROM Enterprise.ThirdPartyRelationship )
BEGIN
	INSERT INTO [Enterprise].[ThirdPartyRelationship] (ThirdPartyRelationshipId, ThirdPartyRelationship )
		VALUES (1, 'Operator'),(2,'Owner'),(3,'Third Party Vendor')

END

GO
--USER STORY 1109853
IF NOT EXISTS( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'EnableEditofPrimaryPropertyToggle')
BEGIN
	INSERT INTO Enterprise.ProductSettingType(Name, Description, SensitiveData)
	VALUES ('EnableEditofPrimaryPropertyToggle', 'Enable Edit of Primary Property Toggle When Operator setting is Enabled', 0)
END

GO

  IF NOT EXISTS (Select 1 From [Batch].[BatchProcessConfigurationType] Where BatchProcessConfigurationTypeId = 3)
  BEGIN
	Insert into [Batch].[BatchProcessConfigurationType] (BatchProcessConfigurationTypeId ,Name,Description)
	Select 3, 'PrimaryPropertiesBulkUpdateApiEndpoint' ,'API Endpoint to be invoked by batch processor'
  END  
GO
IF NOT EXISTS (Select 1 From [Batch].[BatchProcessConfiguration] Where BatchProcessConfigurationId = 3)
  BEGIN
	Declare @ServerName SYSNAME = @@SERVERNAME,
			@apiendpoint varchar(256)
	SET @apiendpoint = '';
	IF @ServerName IN ('RCDUSODBSQL001')  --DEV
	BEGIN
		SET @apiendpoint = 'https://my2dev.realpage.com/api/ppbatchprocessor';
	END
	IF @ServerName IN ('rctusodbsql001') --QA
	BEGIN
		SET @apiendpoint = 'https://my2qa.realpage.com/api/ppbatchprocessor';
	END
	IF @ServerName IN ('RCQUSODBSQL001') --SAT
	BEGIN
		SET @apiendpoint = 'https://my2sat.realpage.com/api/ppbatchprocessor';
	END
	IF @ServerName IN ('RCTUSODBSQL001A','RCTUSODBSQL001B') --UAT
	BEGIN
		Set @apiendpoint = 'https://my2uat.realpage.com/api/ppbatchprocessor';
	END
	IF @ServerName IN ('RCIUSODBSQL002') --PREPROD
	BEGIN
		Set @apiendpoint = 'https://my2preprod.realpage.com/api/ppbatchprocessor';
	END

	IF @ServerName IN ('RCVGBKDBSQL001') --DEMO
	BEGIN
		Set @apiendpoint = 'https://my2demo.realpage.com/api/ppbatchprocessor';
	END

	IF @ServerName IN ('RCTUSODBTUL001') --TRAINING
	BEGIN
		Set @apiendpoint = 'https://my2qa.realpage.com/api/ppbatchprocessor';
	END

	IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') --PROD
	BEGIN
		Set @apiendpoint = 'https://my2.realpage.com/api/ppbatchprocessor';
	END

	IF @ServerName IN ('reagbkdbsql001') --EUSAT
	BEGIN
		SET @apiendpoint = 'https://my2sat.realpage.co.uk/api/ppbatchprocessor';
	END

	IF @ServerName IN ('repgbkdbsql001a','repgbkdbsql001b') --EUPROD
	BEGIN
		SET @apiendpoint = 'https://my2.realpage.co.uk/api/ppbatchprocessor';
	END

	Insert into [Batch].[BatchProcessConfiguration](BatchProcessConfigurationId, BatchProcessConfigurationTypeId, Value)
	Select 3,3, @apiendpoint
  END  
GO
IF NOT EXISTS (Select 1 From [Batch].[BatchProcessType] Where BatchProcessTypeId = 13)
  BEGIN
	 Insert Into [Batch].[BatchProcessType]( BatchProcessTypeId,BatchProcessConfigurationId,Description,Name)
	 Select 13, 3, 'Batch to create Primary Properties Bulk Update Users','PrimaryPropertiesBulkUpdateProductUser'
  END


GO
IF NOT EXISTS (Select 1 From [Batch].[BatchProcessType] Where BatchProcessTypeId = 14)
  BEGIN
	 Insert Into [Batch].[BatchProcessType]( BatchProcessTypeId,BatchProcessConfigurationId,Description,Name)
	 Select 14, 1, 'Batch to update user Primary Properties','PrimaryPropertiesUpdateProductUser'
  END
GO

DELETE FROM [UserManagement].[Control]
WHERE	UIId='UnifiedPlatformProductAccessAllowaccesstoallcurrentandfuturepropertiesPropertiesSwitchUIId'

GO