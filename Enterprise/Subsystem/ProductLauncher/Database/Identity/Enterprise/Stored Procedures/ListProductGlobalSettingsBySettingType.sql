CREATE PROCEDURE [Enterprise].[ListProductGlobalSettingsBySettingType]
    @ProductSettingType NVARCHAR(100)
AS
BEGIN
		
	DECLARE @NOW DATETIME = GETUTCDATE();

	SELECT	pc.ProductConfigurationId,
			gpc.ConfigurationId,
			pst.Name,
			ps.Value,
			pst.SensitiveData,
			p.ProductId,
			p.name [ProductName],
			p.BooksProductCode,
			P.Active ProductActive
    FROM	Enterprise.GlobalProductConfiguration gpc
			JOIN Enterprise.Product P ON gpc.ProductId = P.ProductId
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
			
    WHERE  
			pst.Name = @ProductSettingType
			AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
	ORDER BY
		P.Name
END;
