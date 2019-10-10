CREATE PROCEDURE [Enterprise].[ListProductSettingType]
AS
BEGIN  
	SELECT	ProductSettingTypeId,
			Name,
			Description
	FROM	Enterprise.ProductSettingType
END;