CREATE PROCEDURE Enterprise.GetProductSettingType
    @Name VARCHAR(50) ,
    @ProductSettingTypeId INT OUTPUT
AS
    BEGIN
        SELECT @ProductSettingTypeId = ProductSettingTypeId
        FROM   Enterprise.ProductSettingType
        WHERE  [Name] = @Name;
    END;