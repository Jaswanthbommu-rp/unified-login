if NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'CurrentSigningCertThumbprint' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'CurrentSigningCertThumbprint', 'The currently in use signing certificate', 0)
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'CurrentSigningCertFilePath' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'CurrentSigningCertFilePath', 'The currently in use signing certificate filename', 0)
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'CurrentSigningCertPwd' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'CurrentSigningCertPwd', 'The currently in use signing certificate password', 1)
END

IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE name = 'AdditionalValidationCertsThumbprint' )
BEGIN
	INSERT INTO enterprise.ProductSettingType ( name, Description, SensitiveData ) VALUES ( 'AdditionalValidationCertsThumbprint', 'Additional certificate(s) available', 0)
END

IF NOT EXISTS ( select top 1 1 from Enterprise.ProductSettingType where name = 'AdditionalValidationCertsFilePath' )
begin
	insert into enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'AdditionalValidationCertsFilePath', 'Additional certificate(s) available filenames', 0)
END

GO

DECLARE @ServerName SYSNAME = @@SERVERNAME
DECLARE @currentcertificate VARCHAR(40) = '0c1c5fb7dd7e5b2f03deac733010dabca055761c'

IF @ServerName IN ('RCDUSODBSQL001')  --DEV
BEGIN
	SET @currentcertificate = '64b1365f881c1127c107d1762a31db71cc09feca';
END
IF @ServerName IN ('rctusodbsql001') --QA
BEGIN
	SET @currentcertificate = '58ebdb8e92be544cdb0b276636dfd7f28004a28f';
END
IF @ServerName IN ('rcausodbsql001', 'REAGBKDBSQL001', 'RCTUSODBSQL001A', 'RCTUSODBSQL001B', 'RCIUSODBSQL002') --SAT, UAT, PREPROD, EUSAT -- new e687f5f5674248c5746b756e256c286ff1144daf
BEGIN
	SET @currentcertificate = '0C1C5FB7DD7E5B2F03DEAC733010DABCA055761C';
END

IF @ServerName IN ('RCVGBKDBSQL001', 'RCPGBKDBSQL005A', 'RCPGBKDBSQL005B', 'REPGBKDBSQL001A', 'REPGBKDBSQL001B') --DEMO, PROD, EUPROD
BEGIN
	SET @currentcertificate = '0C1C5FB7DD7E5B2F03DEAC733010DABCA055761C'; -- new fda847634845940374900cbedbc8b545df4dee04
END

IF @ServerName IN ('RCTUSODBTUL001') --TRAINING
BEGIN
	SET @currentcertificate = '0C1C5FB7DD7E5B2F03DEAC733010DABCA055761C'; -- new 46dc07a80e33cb98f1ead0fcd1354dddedfd74cf
END

--dev 
--qa  
--sat 0c1c5fb7dd7e5b2f03deac733010dabca055761c
--current 
DECLARE @NOW DATETIME = GETUTCDATE()

if NOT EXISTS (
	select TOP (1) 1 
		FROM Enterprise.GlobalProductConfiguration gpc  
		JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
		JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
		JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
			WHERE  gpc.ProductId = 3
		AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		AND pst.Name = 'CurrentSigningCertThumbprint'
	)
	BEGIN
		declare @currentproductconfigurationid INT
		select distinct TOP (1) @currentproductconfigurationid = pc.configurationid
			FROM Enterprise.GlobalProductConfiguration gpc  
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
				WHERE  gpc.ProductId = 3
			AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		order by pc.ConfigurationId DESC

		if (@currentproductconfigurationid is not null)
		begin
			insert into enterprise.ProductSetting ( productid, ProductSettingTypeId, value, FromDate )
				select 3, productsettingtypeid, @currentcertificate, GETUTCDATE()
					from enterprise.ProductSettingType where name = 'CurrentSigningCertThumbprint'
			insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				values ( @currentproductconfigurationid, SCOPE_IDENTITY(), GETUTCDATE(), null )
		end
	END
GO
