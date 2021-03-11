CREATE PROCEDURE [Enterprise].[SetProductSetting] (                
 @ProductSettingId INT OUTPUT,              
 @ProductId INT,                
 @ProductSettingTypeId INT,                
 @Value NVARCHAR(1000)              
)                
AS
BEGIN                
BEGIN TRY                  
BEGIN TRAN;
DECLARE   
 @ConfigurationId int,
@productConfigurationId int,               
 @now datetime,              
 @nowPlusSec datetime,              
 @TempSettingId INT              
             
 SET @now = getdate()              
 SET @nowPlusSec=DATEADD (ss, 1, @now)                    
  

--1. check if given setting exists, if not add one
IF NOT EXISTS(select top 1 * from Enterprise.ProductSetting Where ProductId = @ProductId AND ProductSettingTypeId = @ProductSettingTypeId AND Value = @Value)
       BEGIN
              --This setting does not exists so inserting new one
              INSERT INTO Enterprise.ProductSetting(ProductId, ProductSettingTypeId, Value, FromDate, ThruDate)              
                     VALUES(@ProductId, @ProductSettingTypeId, @Value, @nowPlusSec, null)              
              SELECT @ProductSettingId = SCOPE_IDENTITY()  
       END
ELSE
       BEGIN 
              SELECT @ProductSettingId = ProductSettingId 
              FROM Enterprise.ProductSetting
              WHERE ProductId = @ProductId 
              AND ProductSettingTypeId = @ProductSettingTypeId
              AND [Value] = @Value
              AND ThruDate is Null
       END

--2. check if product already has global config and get cofigId, if not add new and get the Id
IF EXISTS(SELECT TOP 1 * FROM Enterprise.GlobalProductConfiguration WHERE productId = @ProductId and ThruDate is Null)
       BEGIN
              --This product already has Global Config ID
              SELECT TOP 1 @ConfigurationId = ConfigurationId from Enterprise.GlobalProductConfiguration where productId = @ProductId and ThruDate is Null
       END
ELSE
       BEGIN
              --Insert New Global Config
              INSERT INTO Enterprise.Configuration(CreateDate) VALUES (@now)              
              SET @ConfigurationId = SCOPE_IDENTITY()              
                
              INSERT INTO Enterprise.GlobalProductConfiguration(ConfigurationId, ProductId, FromDate, ThruDate)              
              VALUES (@ConfigurationId, @ProductId, @now, null)       
       END

--3 check if there is already a setting for this type product combo   
SELECT TOP 1 @TempSettingId = a.ProductSettingId, @productConfigurationId = b.ProductConfigurationId
FROM Enterprise.ProductSetting a    
JOIN Enterprise.ProductConfiguration b    
       ON a.ProductSettingId = b.ProductSettingId 
       AND b.ConfigurationId = @ConfigurationId     
       AND b.ThruDate IS null    
WHERE ProductSettingTypeId = @ProductSettingTypeId               
       AND ProductId = @ProductId              
       AND @now BETWEEN a.FromDate AND Isnull(a.ThruDate, @nowPlusSec)    

--4 if exists then end the existing prodconfig and add new else, insert into prodconfig
IF(@TempSettingId IS NOT NULL)              
       BEGIN
              --Combo exists
              --Kill old product configuration
              UPDATE Enterprise.ProductConfiguration
              SET ThruDate = GETUTCDATE()
              WHERE ProductSettingId = @TempSettingId
                     AND ConfigurationId = @ConfigurationId
                     AND ProductConfigurationId = @productConfigurationId
    END 

--Inserting new configuration
INSERT INTO Enterprise.ProductConfiguration(ConfigurationId, ProductSettingId, FromDate, ThruDate)              
VALUES(@ConfigurationId, @ProductSettingId, @nowPlusSec, null)        

COMMIT;                 
    END TRY                  
    BEGIN CATCH                  
        DECLARE @ErrorLogID INT;                  
  EXEC dbo.LogError                  
   @ErrorLogID = @ErrorLogID OUTPUT;                
        SELECT 0 AS Id ,                  
                ErrorMessage                  
        FROM   dbo.ErrorLog                  
        WHERE  ErrorLogID = @ErrorLogID;                  
                  
        ROLLBACK;                  
    END CATCH;                  
END;

