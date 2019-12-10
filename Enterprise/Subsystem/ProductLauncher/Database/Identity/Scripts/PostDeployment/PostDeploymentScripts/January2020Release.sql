GO
/*
EXEC Enterprise.ListGlobalSettingsForProduct
	@ProductId = 3
*/

DECLARE	@Now datetime = GETUTCDATE(),
	@ProductId int,
	@IsSendGridEnabled nvarchar(100) = N'IsSendGridEnabled',
	@IsSendGridEnabledValue nvarchar(2000) = N'1',
	@SendGridApiEndPoint nvarchar(100) = N'SendGridApiEndPoint',
	@SendGridApiEndPointUrl nvarchar(2000) = N'https://ueapi-dev.realpage.com/emails/api/v1/sendEmail/',
	@ProductConfigurationId int

DECLARE @ProductSetting TABLE (
	ProductSettingId int
)

SELECT	@ProductId = ProductId
FROM		Enterprise.Product
WHERE	Name = 'Unified Platform'

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE Name = @IsSendGridEnabled)
BEGIN
	INSERT INTO Enterprise.ProductSettingType (
		Name,
		Description
	)
	VALUES (
		@IsSendGridEnabled,
		'Is SendGrid Email Enabled in Unified Platform'
	)
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE Name = @SendGridApiEndPoint)
BEGIN
	INSERT INTO Enterprise.ProductSettingType (
		Name,
		Description
	)
	VALUES (
		@SendGridApiEndPoint,
		'SendGrid Email Api Endpoint'
	)
END

SELECT DISTINCT TOP 1 @ProductConfigurationId = pc.ConfigurationId
FROM	Enterprise.GlobalProductConfiguration gpc  
			INNER JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
			INNER JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
			INNER JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
WHERE		gpc.ProductId = @ProductId
AND			((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
AND			((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
AND			((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
ORDER BY pc.ConfigurationId DESC

IF NOT EXISTS (
	SELECT TOP 1 1 
	FROM	Enterprise.GlobalProductConfiguration gpc  
				INNER JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
				INNER JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
				INNER JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
	WHERE	gpc.ProductId = @ProductId  
	AND			((@Now BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@Now >= gpc.FromDate AND gpc.ThruDate IS NULL))  
	AND			((@Now BETWEEN pc.FromDate AND pc.ThruDate) OR (@Now >= pc.FromDate AND pc.ThruDate IS NULL))  
	AND			((@Now BETWEEN ps.FromDate AND ps.ThruDate) OR (@Now >= ps.FromDate AND ps.ThruDate IS NULL))  
	AND			pst.Name = @IsSendGridEnabled
	AND			ps.Value = @IsSendGridEnabledValue
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
						@IsSendGridEnabledValue,
						@Now,
						NULL
		FROM		Enterprise.ProductSettingType
		WHERE	Name = @IsSendGridEnabled

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

IF NOT EXISTS (
	SELECT TOP 1 1 
	FROM	Enterprise.GlobalProductConfiguration gpc  
				INNER JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
				INNER JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
				INNER JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
	WHERE	gpc.ProductId = @ProductId  
	AND			((@Now BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@Now >= gpc.FromDate AND gpc.ThruDate IS NULL))  
	AND			((@Now BETWEEN pc.FromDate AND pc.ThruDate) OR (@Now >= pc.FromDate AND pc.ThruDate IS NULL))  
	AND			((@Now BETWEEN ps.FromDate AND ps.ThruDate) OR (@Now >= ps.FromDate AND ps.ThruDate IS NULL))  
	AND			pst.Name = @SendGridApiEndPoint
	AND			ps.Value = @SendGridApiEndPointUrl
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
						@SendGridApiEndPointUrl,
						@Now,
						NULL
		FROM		Enterprise.ProductSettingType
		WHERE	Name = @SendGridApiEndPoint

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