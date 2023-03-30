
GO
Declare @ServerName SYSNAME = @@SERVERNAME
Declare @topicName1 varchar(256);
SET @topicName1 = '';

IF @ServerName IN ('RCDUSODBSQL001')  --DEV
BEGIN
	SET @topicName1 = 'RPUS-UNITY-USERS-AUDIT-REPORT-DEV';
END
IF @ServerName IN ('rctusodbsql001') --QA
BEGIN
	SET @topicName1 = 'RPUS-UNITY-USERS-AUDIT-REPORT-QA';
END
IF @ServerName IN ('rcausodbsql001') --SAT
BEGIN
	SET @topicName1 = 'RPUS-UNITY-USERS-AUDIT-REPORT-SAT';
END
IF @ServerName IN ('RCTUSODBSQL001A','RCTUSODBSQL001B') --UAT
BEGIN
	SET @topicName1 = 'RPUS-UNITY-USERS-AUDIT-REPORT-UAT';
END
IF @ServerName IN ('RCIUSODBSQL002') --PREPROD
BEGIN
	SET @topicName1 = 'RPUS-UNITY-USERS-AUDIT-REPORT-PREPROD';
END
IF @ServerName IN ('RCVGBKDBSQL001') --DEMO
BEGIN
	SET @topicName1 = 'RPUS-UNITY-USERS-AUDIT-REPORT-DEMO';
END
IF @ServerName IN ('RCTUSODBTUL001') --TRAINING
BEGIN
	SET @topicName1 = 'RPUS-UNITY-USERS-AUDIT-REPORT-TRAINING';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') --PROD
BEGIN
	SET @topicName1 = 'RPUS-UNITY-USERS-AUDIT-REPORT-PROD';
END
IF @ServerName IN ('reagbkdbsql001') --EUSAT
BEGIN
	SET @topicName1 = 'RPEU-UNITY-USERS-AUDIT-REPORT-EUSAT';
END
IF @ServerName IN ('repgbkdbsql001a','repgbkdbsql001b') --EUPROD
BEGIN
	SET @topicName1 = 'RPEU-UNITY-USERS-AUDIT-REPORT-EUPROD';
END

IF NOT EXISTS (Select 1 From [Enterprise].[UserSyncJobType] Where [Name] = 'UserProductReport')
BEGIN
	Insert Into [Enterprise].[UserSyncJobType](UserSyncJobTypeId,[Description],[Name],[KafkaTopicName])
	Select 4, 'Generate User Audit Reports','UserProductReport',@topicName1
END

IF NOT EXISTS (Select 1 From [Enterprise].[ProductSettingType] Where [Name]	= 'UnifiedReportingEndPoint')
BEGIN
	Insert Into [Enterprise].[ProductSettingType]([Name],[Description],[SensitiveData])
	VALUES('UnifiedReportingEndPoint', 'The api uri for KONG Unified Reporting', 0)
END

IF NOT EXISTS (Select 1 From [Enterprise].[ProductSettingType] Where [Name]	= 'UserReport_ReportingKafkaTopicName')
BEGIN
	Insert Into [Enterprise].[ProductSettingType]([Name],[Description],[SensitiveData])
	VALUES('UserReport_ReportingKafkaTopicName','Unified Reporting Kafka topic name',0)
END

GO

Declare @ServerName SYSNAME = @@SERVERNAME
Declare @topicName1 varchar(256);
SET @topicName1 = '';

IF @ServerName IN ('RCDUSODBSQL001')  --DEV
BEGIN
	SET @topicName1 = 'RPUS-UNITY-REPORTING-RUR-STATUS-DEV';
END
IF @ServerName IN ('rctusodbsql001') --QA
BEGIN
	SET @topicName1 = 'RPUS-UNITY-REPORTING-RUR-STATUS-QA';
END
IF @ServerName IN ('rcausodbsql001') --SAT
BEGIN
	SET @topicName1 = 'RPUS-UNITY-REPORTING-RUR-STATUS-SAT';
END
IF @ServerName IN ('RCTUSODBSQL001A','RCTUSODBSQL001B') --UAT
BEGIN
	SET @topicName1 = 'RPUS-UNITY-REPORTING-RUR-STATUS-UAT';
END
IF @ServerName IN ('RCIUSODBSQL002') --PREPROD
BEGIN
	SET @topicName1 = 'RPUS-UNITY-REPORTING-RUR-STATUS-PREPROD';
END
IF @ServerName IN ('RCVGBKDBSQL001') --DEMO
BEGIN
	SET @topicName1 = 'RPUS-UNITY-REPORTING-RUR-STATUS-DEMO';
END
IF @ServerName IN ('RCTUSODBTUL001') --TRAINING
BEGIN
	SET @topicName1 = 'RPUS-UNITY-REPORTING-RUR-STATUS-TRAINING';
END
IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') --PROD
BEGIN
	SET @topicName1 = 'RPUS-UNITY-REPORTING-RUR-STATUS-PROD';
END
IF @ServerName IN ('reagbkdbsql001') --EUSAT
BEGIN
	SET @topicName1 = 'RPEU-UNITY-REPORTING-RUR-STATUS-EUSAT';
END
IF @ServerName IN ('repgbkdbsql001a','repgbkdbsql001b') --EUPROD
BEGIN
	SET @topicName1 = 'RPEU-UNITY-REPORTING-RUR-STATUS-EUPROD';
END
DECLARE @ProductId INT = 3, @ProductsettingTypeid int;
IF NOT EXISTS (SELECT TOP (1) (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
    WHERE PS.ProductId = @ProductId AND pst.Name = 'UserReport_ReportingKafkaTopicName' )
BEGIN
    SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'UserReport_ReportingKafkaTopicName'
    exec [Enterprise].[SetProductSetting] 0,@Productid,@ProductsettingTypeid, @topicName1
END
GO