
  IF NOT EXISTS (SELECT TOP 1 1 FROM Batch.BatchProcessConfigurationType WHERE [Name] = 'CompanyPropertiesApiEndpoint')
   BEGIN
	   INSERT INTO Batch.BatchProcessConfigurationType(BatchProcessConfigurationTypeId,[Name],[Description])
	   VALUES(5,'CompanyPropertiesApiEndpoint','API Endpoint to be invoked by batch processor')
   END
GO
DECLARE @DBName VARCHAR(20) = 'UPSandbox';
IF NOT EXISTS(Select top 1 1 From [Batch].[BatchProcessConfiguration] Where BatchProcessConfigurationId = 5)
  BEGIN
  
	DECLARE @serverName VARCHAR(50);
	SELECT @serverName = @@SERVERNAME 
	
	DECLARE @endpoint VARCHAR(100)
	SET @endpoint = '';

	IF(@serverName = 'RCDUSODBSQL001') -- dev
	BEGIN 
		Set @endpoint = 'https://www-dev2.realpage.com/api/CompanySetup/CompanyPropertiesUpdate';
	END
	IF(@serverName = 'RCTUSODBSQL001') -- qa
	BEGIN 
		Set @endpoint = 'https://my2qa.realpage.com/api/CompanySetup/CompanyPropertiesUpdate';
	END
	IF @ServerName IN ('RCAUSODBSQL001') --SAT
	BEGIN
		Set @endpoint = 'https://my2sat.realpage.com/api/CompanySetup/CompanyPropertiesUpdate';
	END
	IF @ServerName IN ('RCTUSODBSQL001A','RCTUSODBSQL001B') --UAT
	BEGIN
		Set @endpoint = 'https://my2uat.realpage.com/api/CompanySetup/CompanyPropertiesUpdate';
	END
	IF @ServerName IN ('RCIUSODBSQL002') --PREPROD
	BEGIN
		Set @endpoint = 'https://my2preprod.realpage.com/api/CompanySetup/CompanyPropertiesUpdate';
	END

	IF @ServerName IN ('RCVGBKDBSQL001') --DEMO
	BEGIN
		Set @endpoint = 'https://my2demo.realpage.com/api/CompanySetup/CompanyPropertiesUpdate';
	END

	IF @ServerName IN ('RCTUSODBTUL001') --TRAINING
	BEGIN
		Set @endpoint = 'https://my2qa.realpage.com/api/CompanySetup/CompanyPropertiesUpdate';
	END

	IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') --PROD
	BEGIN
		Set @endpoint = 'https://my2.realpage.com/api/CompanySetup/CompanyPropertiesUpdate';
	END

	IF @ServerName IN ( 'GNAGBKDBSQL001') --EUSAT
	BEGIN
		SET @endpoint = 'https://my2sat.realpage.co.uk/api/CompanySetup/CompanyPropertiesUpdate';
	END

	IF @ServerName IN ('GNPGBKDBSQL001A','gnpgbkdbsql001b') --EUPROD
	BEGIN
		SET @endpoint = 'https://my2.realpage.co.uk/api/CompanySetup/CompanyPropertiesUpdate';
	END
	IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = @DBName)
	BEGIN 
		-- Default to sandbox endpoint
		SET @DBName = 'UPSandbox';
		SET @endpoint = 'https://my2sat.realpage.com/api/CompanySetup/CompanyPropertiesUpdate';
	END

	INSERT INTO Batch.BatchProcessConfiguration(BatchProcessConfigurationId,BatchProcessConfigurationTypeId,[Value])
	Select 5,5,@endpoint
  END
  GO
    IF NOT EXISTS (SELECT 1 FROM [Batch].[BatchProcessType] WHERE [Name] = 'CompanyPropertiesUpdate')
  BEGIN
	INSERT INTO [Batch].[BatchProcessType]
	SELECT 17,5,'Batch to activate or deactivate company properties','CompanyPropertiesUpdate'
  END
  
GO
