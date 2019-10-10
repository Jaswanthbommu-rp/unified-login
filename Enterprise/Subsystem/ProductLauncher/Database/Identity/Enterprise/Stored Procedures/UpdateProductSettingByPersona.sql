CREATE PROCEDURE [Enterprise].[UpdateProductSettingByPersona]
    (
        @PersonaId BIGINT ,
        @ProductId INT ,
        @SettingName NVARCHAR(20) ,
        @Value NVARCHAR(1000)
    )
AS
    BEGIN


        UPDATE ps
        SET    [Value] = LTRIM(RTRIM(@Value))
        FROM   Enterprise.ProductSetting ps
               JOIN Enterprise.ProductConfiguration pc ON pc.ProductSettingId = ps.ProductSettingId
               JOIN Enterprise.ProductSettingType pt ON pt.ProductSettingTypeId = ps.ProductSettingTypeId
               JOIN Enterprise.PersonaConfiguration perc ON perc.ConfigurationId = pc.ConfigurationId
        WHERE  pt.[Name] = @SettingName
               AND perc.PersonaId = @PersonaId
               AND perc.ProductId = @ProductId;

    END;