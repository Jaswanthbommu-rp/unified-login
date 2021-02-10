CREATE PROCEDURE [Enterprise].[ListGlobalSettingsForProduct]
    @ProductId INT
AS
    BEGIN
		
		DECLARE @NOW DATETIME = GETUTCDATE();

        SELECT	pc.ProductConfigurationId,
				gpc.ConfigurationId,
				pst.Name,
				ps.Value,
				pst.SensitiveData, 
				pst.ProductSettingTypeId, 
				ps.ProductSettingId,
				pst.Description
        FROM	Enterprise.GlobalProductConfiguration gpc
				JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
				JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
				JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
        WHERE  gpc.ProductId = @ProductId
				AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))
				AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))
				AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
    END;

