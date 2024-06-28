
Go
Declare @createdate datetime, @CreatedBy bigint, @RightId int, @ProductsettingTypeid int;
Declare @propertytopicname varchar(100), @companytopicname varchar(100)

select @createdate = GETUTCDATE()
SELECT	@CreatedBy = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

SET @companytopicname = 'company-udm-update-dev';
SET @propertytopicname = 'property-udm-update-dev';

Declare @ServerName SYSNAME = @@SERVERNAME

		IF @ServerName IN ('RCDUSODBSQL001')  --DEV
		BEGIN
			SET @companytopicname = 'company-udm-update-dev';
			SET @propertytopicname = 'property-udm-update-dev';
		END
		IF @ServerName IN ('rctusodbsql001') --QA
		BEGIN
			SET @companytopicname = 'company-udm-update-qa';
			SET @propertytopicname = 'property-udm-update-qa';
		END
		IF @ServerName IN ('rcausodbsql001') --SAT
		BEGIN
			SET @companytopicname = 'company-udm-update-stg';
			SET @propertytopicname = 'property-udm-update-stg';
		END
		IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') --PROD
		BEGIN
			SET @companytopicname = 'company-udm-update';
			SET @propertytopicname = 'property-udm-update';
		END

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'CompanyUDMUpdateTopic')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('CompanyUDMUpdateTopic', 'Consume Company Update UDM Topic Name', 0);
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'CompanyUDMUpdateTopic'
    exec [Enterprise].[SetProductSetting] 0,3,@ProductsettingTypeid,@companytopicname
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'PropertyUDMUpdateTopic')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('PropertyUDMUpdateTopic', 'Consume Property Update UDM Topic Name', 0);
	SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'PropertyUDMUpdateTopic'
    exec [Enterprise].[SetProductSetting] 0,3,@ProductsettingTypeid,@propertytopicname
END
