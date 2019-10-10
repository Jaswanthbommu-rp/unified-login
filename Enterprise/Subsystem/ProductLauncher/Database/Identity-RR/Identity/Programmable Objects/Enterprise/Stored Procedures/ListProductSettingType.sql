IF OBJECT_ID('[Enterprise].[ListProductSettingType]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListProductSettingType];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListProductSettingType]
AS
BEGIN  
	SELECT	ProductSettingTypeId,
			Name,
			Description
	FROM	Enterprise.ProductSettingType
END;
GO
