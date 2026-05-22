CREATE PROCEDURE [Enterprise].[ProductConfigurationSetup] 
	@ProductId INT,
    @LoginUri NVARCHAR(500),
	@SigningCertificateThumbprint NVARCHAR(50),
	@ProductConfiguration ProductConfigurationType READONLY
AS
BEGIN
	DECLARE @FromDate DATETIME = GETUTCDATE();
	DECLARE @Rownum INT;
	DECLARE @SettingName NVARCHAR(50);
	DECLARE @SettingDescription NVARCHAR(100);
	DECLARE @SettingValue NVARCHAR(1000);
	DECLARE @PStatus BIT;
	DECLARE @ConfigurationID BIGINT;
	DECLARE @ProductSettingID BIGINT;
	DECLARE @ProductSettingTypeID INT;
	DECLARE @SettingSensitiveData TINYINT;

	IF EXISTS (SELECT 1 FROM Enterprise.Product WHERE ProductId = @ProductId)
	BEGIN
		IF (@LoginUri IS NOT NULL AND @SigningCertificateThumbprint IS NOT NULL)
		BEGIN
			IF NOT EXISTS  (SELECT 1
							FROM Ident.SamlProductSettings
							WHERE ProductId = @ProductId)
			BEGIN	
				INSERT INTO Ident.SamlProductSettings (
					ProductId,
					LoginUri,
					SigningCertificateThumbprint,
					SubjectIdSamlAttribute)
				VALUES (
					@ProductId,
					@LoginUri,
					@SigningCertificateThumbprint,
					'productUsername');
			END
			ELSE
			BEGIN
				UPDATE Ident.SamlProductSettings
				SET	LoginUri = @LoginUri,
					SigningCertificateThumbprint = @SigningCertificateThumbprint
				WHERE ProductId = @ProductId;
			END
		END;

		IF ((CASE WHEN @LoginUri IS NULL THEN 0 ELSE 1 END) ^ (CASE WHEN @SigningCertificateThumbprint IS NULL THEN 0 ELSE 1 END)) = 1
		BEGIN
			UPDATE Ident.SamlProductSettings
			SET	LoginUri = ISNULL(@LoginUri, LoginUri),
				SigningCertificateThumbprint = ISNULL(@SigningCertificateThumbprint, SigningCertificateThumbprint)
			WHERE ProductId = @ProductId;
		END;
	
		IF (SELECT COUNT(*)	FROM @ProductConfiguration) > 0
		BEGIN
			SELECT IDENTITY(INT, 1, 1) AS RowNum,
				@ProductId AS ProductId,
				SettingName,
				SettingDescription,
				SettingValue,
				SettingSensitiveData,
				0 AS PStatus,
				0 AS SStatus
			INTO #ProdConfig
			FROM @ProductConfiguration;

			IF EXISTS (
				SELECT 1
				FROM Enterprise.GlobalProductConfiguration
				WHERE ProductId = @ProductId
					AND (GETUTCDATE() BETWEEN FromDate AND ThruDate
					OR ThruDate IS NULL))
			BEGIN
				SELECT @ConfigurationId = ConfigurationId
				FROM Enterprise.GlobalProductConfiguration
				WHERE ProductId = @ProductId
					AND (GETUTCDATE() BETWEEN FromDate AND ThruDate
					OR ThruDate IS NULL);
			END;
			ELSE
			BEGIN
				EXEC Enterprise.CreateProductConfiguration
						@ConfigurationId = @ConfigurationId OUTPUT; -- int
			END;
		END;

		WHILE EXISTS (SELECT 1 FROM #ProdConfig WHERE PStatus = 0)
		BEGIN 
			SELECT TOP 1 @Rownum = RowNum,
						 @ProductId = ProductId,
						 @SettingName = SettingName,
						 @SettingDescription = SettingDescription,
						 @SettingValue = SettingValue,
						 @SettingSensitiveData = SettingSensitiveData,
						 @PStatus = PStatus
			FROM #ProdConfig
			WHERE PStatus = 0;
		
			SET @ProductSettingId = NULL;
			SET @ProductSettingTypeId = NULL;
			EXEC Enterprise.GetProductSettingType
					@Name = @SettingName, -- varchar(50)
					@ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

			IF @ProductSettingTypeID IS NULL
			EXEC Enterprise.CreateProductSettingType
					@ProductSettingTypeName = @SettingName,
					@ProductSettingTypeDescription = @SettingDescription,
					@ProductSettingTypeSensitiveData = @SettingSensitiveData,
					@ProductSettingTypeID = @ProductSettingTypeID OUTPUT;

			IF EXISTS (
				SELECT 1
				FROM enterprise.ProductSetting AS PS
				INNER JOIN Enterprise.productsettingtype AS pp 
					ON PS.productsettingtypeid = pp.productsettingtypeid
				WHERE productid = @ProductId
					AND PP.ProductSettingTypeId = @ProductSettingTypeId)
				BEGIN
					UPDATE PS
					SET value = @SettingValue
					FROM Enterprise.ProductSetting PS
					INNER JOIN Enterprise.productsettingtype pp 
						ON PS.productsettingtypeid = pp.productsettingtypeid
					WHERE productid = @ProductId
						AND PP.ProductSettingTypeId = @ProductSettingTypeId;
				END;
				ELSE
				BEGIN
					EXEC Enterprise.CreateProductSetting
							@ProductId = @ProductId, -- int
							@ProductSettingTypeId = @ProductSettingTypeId, -- int
							@Value = @SettingValue, -- nvarchar(1000)
							@FromDate = @FromDate, -- datetime
							@ThruDate = NULL, -- datetime
							@ProductSettingId = @ProductSettingId OUTPUT; -- int

					EXEC Enterprise.LinkProductSettingToConfiguration
							@ConfigurationId = @ConfigurationId, -- int
							@ProductSettingId = @ProductSettingId, -- int
							@FromDate = @FromDate, -- datetime
							@ThruDate = NULL;   -- datetime
				END;

			UPDATE #ProdConfig
			SET PStatus = 1
			WHERE RowNum = @RowNum;
		END;

		IF NOT EXISTS (SELECT 1 FROM Enterprise.GlobalProductConfiguration
						WHERE @ConfigurationID = ConfigurationId
						  AND @ProductId = ProductId)
		EXEC Enterprise.LinkGlobalConfigurationToProduct
				@ConfigurationId = @ConfigurationId, -- int
				@ProductId = @ProductId, -- int
				@FromDate = @FromDate, -- datetime
				@ThruDate = NULL;   -- datetime

		EXEC Enterprise.ListGlobalSettingsForProduct
				@productid = @ProductId;
	END;
END;
GO


