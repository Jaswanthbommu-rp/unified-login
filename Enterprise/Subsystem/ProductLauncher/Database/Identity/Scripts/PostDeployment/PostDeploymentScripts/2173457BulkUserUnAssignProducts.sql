


IF NOT EXISTS (SELECT 1 FROM [Batch].[BatchProcessConfigurationType] Where [Name] = 'BulkUserProcessApiEndpoint')
Begin
    Insert Into [Batch].[BatchProcessConfigurationType](BatchProcessConfigurationTypeId,[Name],[Description])
	Select 4,'BulkUserProcessApiEndpoint', 'API Endpoint to be invoked by batch processor'
End



IF NOT EXISTS(Select top 1 1 From [Batch].[BatchProcessConfiguration] Where BatchProcessConfigurationId = 4)
  BEGIN
  
	DECLARE @serverName VARCHAR(50);
	SELECT @serverName = @@SERVERNAME 
	
	DECLARE @endpoint VARCHAR(100)
	SET @endpoint = '';

	IF(@serverName = 'RCDUSODBSQL001') -- dev
	BEGIN 
		Set @endpoint = 'https://my2dev.realpage.com/api/bulkuserbatchprocessor';
	END
	IF(@serverName = 'RCTUSODBSQL001') -- qa
	BEGIN 
		Set @endpoint = 'https://my2qa.realpage.com/api/bulkuserbatchprocessor';
	END
	IF @ServerName IN ('RCAUSODBSQL001') --SAT
	BEGIN
		Set @endpoint = 'https://my2sat.realpage.com/api/bulkuserbatchprocessor';
	END
	IF @ServerName IN ('RCTUSODBSQL001A','RCTUSODBSQL001B') --UAT
	BEGIN
		Set @endpoint = 'https://my2uat.realpage.com/api/bulkuserbatchprocessor';
	END
	IF @ServerName IN ('RCIUSODBSQL002') --PREPROD
	BEGIN
		Set @endpoint = 'https://my2preprod.realpage.com/api/bulkuserbatchprocessor';
	END

	IF @ServerName IN ('RCVGBKDBSQL001') --DEMO
	BEGIN
		Set @endpoint = 'https://my2demo.realpage.com/api/bulkuserbatchprocessor';
	END

	IF @ServerName IN ('RCTUSODBTUL001') --TRAINING
	BEGIN
		Set @endpoint = 'https://my2qa.realpage.com/api/bulkuserbatchprocessor';
	END

	IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') --PROD
	BEGIN
		Set @endpoint = 'https://my2.realpage.com/api/bulkuserbatchprocessor';
	END

	IF @ServerName IN ( 'GNAGBKDBSQL001') --EUSAT
	BEGIN
		SET @endpoint = 'https://my2sat.realpage.co.uk/api/bulkuserbatchprocessor';
	END

	IF @ServerName IN ('GNPGBKDBSQL001A','gnpgbkdbsql001b') --EUPROD
	BEGIN
		SET @endpoint = 'https://my2.realpage.co.uk/api/bulkuserbatchprocessor';
	END


	INSERT INTO Batch.BatchProcessConfiguration(BatchProcessConfigurationId,BatchProcessConfigurationTypeId,[Value])
	Select 4,4,@endpoint
  END
  GO




    IF NOT EXISTS (SELECT 1 FROM [Batch].[BatchProcessType] WHERE [Name] = 'AssignOrUnasignProductsForBulkUsers')
  BEGIN
	INSERT INTO [Batch].[BatchProcessType]
	SELECT 16,4,'Batch to assign or un assign products for bulk users','AssignOrUnasignProductsForBulkUsers'
  END


