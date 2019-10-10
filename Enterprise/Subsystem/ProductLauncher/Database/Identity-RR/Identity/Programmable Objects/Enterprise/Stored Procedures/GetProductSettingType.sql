IF OBJECT_ID('[Enterprise].[GetProductSettingType]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[GetProductSettingType];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[GetProductSettingType]
    @Name VARCHAR(50) ,
    @ProductSettingTypeId INT OUTPUT
AS
    BEGIN
        SELECT @ProductSettingTypeId = ProductSettingTypeId
        FROM   Enterprise.ProductSettingType
        WHERE  [Name] = @Name;
    END;
GO
