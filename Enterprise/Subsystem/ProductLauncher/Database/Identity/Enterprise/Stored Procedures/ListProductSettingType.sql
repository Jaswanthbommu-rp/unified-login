CREATE PROCEDURE [Enterprise].[ListProductSettingType]
AS
BEGIN  
	SELECT	ProductSettingTypeId,
			Name,
			Description,
			SensitiveData
	FROM	Enterprise.ProductSettingType
END;