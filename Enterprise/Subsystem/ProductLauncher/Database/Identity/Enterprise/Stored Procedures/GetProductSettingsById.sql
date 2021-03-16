CREATE PROCEDURE [Enterprise].[GetProductSettingsById]    
    @ProductId INT    
AS    
    BEGIN    
      
  DECLARE @NOW DATETIME = GETUTCDATE();    
    
        SELECT pc.ProductConfigurationId,    
    gpc.ConfigurationId,    
    pst.Name,    
    ps.Value,    
    pst.SensitiveData,     
    pst.ProductSettingTypeId,     
    ps.ProductSettingId,    
    pst.Description, gpc.ProductId   
 FROM Enterprise.ProductSetting ps  
  JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId    
  JOIN Enterprise.ProductConfiguration pc ON pc.ProductSettingId = ps.ProductSettingId    
  JOIN Enterprise.GlobalProductConfiguration gpc ON gpc.ConfigurationId = pc.ConfigurationId  
    WHERE  ps.ProductId = @ProductId  
  AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))    
  AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))    
  AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))    
    END;  