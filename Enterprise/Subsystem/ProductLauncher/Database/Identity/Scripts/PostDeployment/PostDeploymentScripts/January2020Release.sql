GO
/*
EXEC Enterprise.ListGlobalSettingsForProduct
	@ProductId = 3
*/

DECLARE	@Now datetime = GETUTCDATE(),
	@ProductId int,
	@ProductSettingType nvarchar(100),
	@ProductSettingValue nvarchar(2000),
	@ProductConfigurationId int

DECLARE @ProductSetting TABLE (
	ProductSettingId int
)

SELECT	@ProductId = ProductId
FROM		Enterprise.Product
WHERE	Name = 'Unified Platform'

SET @ProductSettingType = N'IsSendGridEnabled'
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE Name = @ProductSettingType)
BEGIN
	INSERT INTO Enterprise.ProductSettingType (
		Name,
		Description
	)
	VALUES (
		@ProductSettingType,
		'Is SendGrid Email Enabled in Unified Platform?'
	)
END

SET @ProductSettingType = N'SendGridApiEndPoint'
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE Name = @ProductSettingType)
BEGIN
	INSERT INTO Enterprise.ProductSettingType (
		Name,
		Description
	)
	VALUES (
		@ProductSettingType,
		'SendGrid Email Api EndPoint'
	)
END

SET @ProductSettingType = N'SendGridSendEmailEndPoint'
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE Name = @ProductSettingType)
BEGIN
	INSERT INTO Enterprise.ProductSettingType (
		Name,
		Description
	)
	VALUES (
		@ProductSettingType,
		'SendGrid: SendEmail EndPoint'
	)
END

SELECT DISTINCT TOP 1 @ProductConfigurationId = epc.ConfigurationId
FROM	Enterprise.GlobalProductConfiguration egpc  
			INNER JOIN Enterprise.ProductConfiguration epc ON epc.ConfigurationId = egpc.ConfigurationId  
			INNER JOIN Enterprise.ProductSetting eps ON eps.ProductSettingId = epc.ProductSettingId  
			INNER JOIN Enterprise.ProductSettingType epst ON epst.ProductSettingTypeId = eps.ProductSettingTypeId  
WHERE		egpc.ProductId = @ProductId
AND			((@NOW BETWEEN egpc.FromDate AND egpc.ThruDate) OR (@NOW >= egpc.FromDate AND egpc.ThruDate IS NULL))  
AND			((@NOW BETWEEN epc.FromDate AND epc.ThruDate) OR (@NOW >= epc.FromDate AND epc.ThruDate IS NULL))  
AND			((@NOW BETWEEN eps.FromDate AND eps.ThruDate) OR (@NOW >= eps.FromDate AND eps.ThruDate IS NULL))  
ORDER BY epc.ConfigurationId DESC

SET @ProductSettingType = N'IsSendGridEnabled'
SET @ProductSettingValue = N'0'
IF NOT EXISTS (
	SELECT TOP 1 1 
	FROM	Enterprise.GlobalProductConfiguration egpc  
				INNER JOIN Enterprise.ProductConfiguration epc ON epc.ConfigurationId = egpc.ConfigurationId  
				INNER JOIN Enterprise.ProductSetting eps ON eps.ProductSettingId = epc.ProductSettingId  
				INNER JOIN Enterprise.ProductSettingType epst ON epst.ProductSettingTypeId = eps.ProductSettingTypeId  
	WHERE	egpc.ProductId = @ProductId  
	AND			((@Now BETWEEN egpc.FromDate AND egpc.ThruDate) OR (@Now >= egpc.FromDate AND egpc.ThruDate IS NULL))  
	AND			((@Now BETWEEN epc.FromDate AND epc.ThruDate) OR (@Now >= epc.FromDate AND epc.ThruDate IS NULL))  
	AND			((@Now BETWEEN eps.FromDate AND eps.ThruDate) OR (@Now >= eps.FromDate AND eps.ThruDate IS NULL))  
	AND			epst.Name = @ProductSettingType
	AND			eps.Value = @ProductSettingValue
)
BEGIN
	IF (@ProductConfigurationId IS NOT NULL)
	BEGIN
		DELETE
		FROM	@ProductSetting

		INSERT INTO Enterprise.ProductSetting (
			ProductId,
			ProductSettingTypeId,
			Value,
			FromDate,
			ThruDate
		)
		OUTPUT INSERTED.ProductSettingId INTO @ProductSetting (ProductSettingId)
		SELECT	@ProductId,
						ProductSettingTypeId,
						@ProductSettingValue,
						@Now,
						NULL
		FROM		Enterprise.ProductSettingType
		WHERE	Name = @ProductSettingType

		INSERT INTO Enterprise.ProductConfiguration (
			ConfigurationId,
			ProductSettingId,
			FromDate,
			ThruDate
		)
		SELECT		@ProductConfigurationId,
						ProductSettingId, 
						@Now,
						NULL
		FROM		@ProductSetting
	END
END

SET @ProductSettingType = N'SendGridApiEndPoint'
SET @ProductSettingValue = N'https://ueapi-dev.realpage.com'
IF NOT EXISTS (
	SELECT TOP 1 1 
	FROM	Enterprise.GlobalProductConfiguration egpc  
				INNER JOIN Enterprise.ProductConfiguration epc ON epc.ConfigurationId = egpc.ConfigurationId  
				INNER JOIN Enterprise.ProductSetting eps ON eps.ProductSettingId = epc.ProductSettingId  
				INNER JOIN Enterprise.ProductSettingType epst ON epst.ProductSettingTypeId = eps.ProductSettingTypeId  
	WHERE	egpc.ProductId = @ProductId  
	AND			((@Now BETWEEN egpc.FromDate AND egpc.ThruDate) OR (@Now >= egpc.FromDate AND egpc.ThruDate IS NULL))  
	AND			((@Now BETWEEN epc.FromDate AND epc.ThruDate) OR (@Now >= epc.FromDate AND epc.ThruDate IS NULL))  
	AND			((@Now BETWEEN eps.FromDate AND eps.ThruDate) OR (@Now >= eps.FromDate AND eps.ThruDate IS NULL))  
	AND			epst.Name = @ProductSettingType
	AND			eps.Value = @ProductSettingValue
)
BEGIN
	IF (@ProductConfigurationId IS NOT NULL)
	BEGIN
		DELETE
		FROM	@ProductSetting

		INSERT INTO Enterprise.ProductSetting (
			ProductId,
			ProductSettingTypeId,
			Value,
			FromDate,
			ThruDate
		)
		OUTPUT INSERTED.ProductSettingId INTO @ProductSetting (ProductSettingId)
		SELECT	@ProductId,
						ProductSettingTypeId,
						@ProductSettingValue,
						@Now,
						NULL
		FROM		Enterprise.ProductSettingType
		WHERE	Name = @ProductSettingType

		INSERT INTO Enterprise.ProductConfiguration (
			ConfigurationId,
			ProductSettingId,
			FromDate,
			ThruDate
		)
		SELECT		@ProductConfigurationId,
						ProductSettingId, 
						@Now,
						NULL
		FROM		@ProductSetting
	END
END

SET @ProductSettingType = N'SendGridSendEmailEndPoint'
SET @ProductSettingValue = N'/emails/api/v1/sendEmail/'
IF NOT EXISTS (
	SELECT TOP 1 1 
	FROM	Enterprise.GlobalProductConfiguration egpc  
				INNER JOIN Enterprise.ProductConfiguration epc ON epc.ConfigurationId = egpc.ConfigurationId  
				INNER JOIN Enterprise.ProductSetting eps ON eps.ProductSettingId = epc.ProductSettingId  
				INNER JOIN Enterprise.ProductSettingType epst ON epst.ProductSettingTypeId = eps.ProductSettingTypeId  
	WHERE	egpc.ProductId = @ProductId  
	AND			((@Now BETWEEN egpc.FromDate AND egpc.ThruDate) OR (@Now >= egpc.FromDate AND egpc.ThruDate IS NULL))  
	AND			((@Now BETWEEN epc.FromDate AND epc.ThruDate) OR (@Now >= epc.FromDate AND epc.ThruDate IS NULL))  
	AND			((@Now BETWEEN eps.FromDate AND eps.ThruDate) OR (@Now >= eps.FromDate AND eps.ThruDate IS NULL))  
	AND			epst.Name = @ProductSettingType
	AND			eps.Value = @ProductSettingValue
)
BEGIN
	IF (@ProductConfigurationId IS NOT NULL)
	BEGIN
		DELETE
		FROM	@ProductSetting

		INSERT INTO Enterprise.ProductSetting (
			ProductId,
			ProductSettingTypeId,
			Value,
			FromDate,
			ThruDate
		)
		OUTPUT INSERTED.ProductSettingId INTO @ProductSetting (ProductSettingId)
		SELECT	@ProductId,
						ProductSettingTypeId,
						@ProductSettingValue,
						@Now,
						NULL
		FROM		Enterprise.ProductSettingType
		WHERE	Name = @ProductSettingType

		INSERT INTO Enterprise.ProductConfiguration (
			ConfigurationId,
			ProductSettingId,
			FromDate,
			ThruDate
		)
		SELECT		@ProductConfigurationId,
						ProductSettingId, 
						@Now,
						NULL
		FROM		@ProductSetting
	END
END
GO