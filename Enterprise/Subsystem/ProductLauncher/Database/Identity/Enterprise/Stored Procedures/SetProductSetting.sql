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
   --Check the config id              
   DECLARE               
   @ConfigurationId int,              
   @now datetime,              
   @nowPlusSec datetime,              
   @TempSettingId INT              
              
   set @now = getdate()              
   SET @nowPlusSec=DATEADD (ss, 1, @now)             
     
   select top 1 @ConfigurationId = ConfigurationId from Enterprise.GlobalProductConfiguration               
    where productId =@ProductId and @now between FromDate and Isnull(ThruDate, @nowPlusSec)     
                
   if(not exists(select top 1 1 from   
 Enterprise.ProductSetting a  
 join Enterprise.ProductConfiguration b  
  on a.ProductSettingId=b.ProductSettingId and b.ConfigurationId=@ConfigurationId     
   and b.ThruDate is null    
    where a.ThruDate is null and Value = @Value and ProductSettingTypeId=@ProductSettingTypeId        
     and ProductId=@ProductId))        
   Begin      
               
    --if this is a new product that has no settings create a new config for this product              
    if(@ConfigurationId is null)              
    begin              
     insert into Enterprise.Configuration(CreateDate) values (@now)              
     SET @ConfigurationId=SCOPE_IDENTITY()              
                
     insert into Enterprise.GlobalProductConfiguration(ConfigurationId, ProductId, FromDate, ThruDate)              
     VALUES (@ConfigurationId, @ProductId, @now, null)              
    END              
                    
    --check if there is already a setting for this type product combo       
      
 select top 1 @TempSettingId=a.ProductSettingId     
 from Enterprise.ProductSetting a    
  join Enterprise.ProductConfiguration b    
   on a.ProductSettingId=b.ProductSettingId and b.ConfigurationId=@ConfigurationId     
    and b.ThruDate is null    
    where ProductSettingTypeId = @ProductSettingTypeId               
     and ProductId = @ProductId              
     and @now between a.FromDate and Isnull(a.ThruDate, @nowPlusSec)      
    
    
    
              
    --if exists then close it by setting the ThruDate to now              
    if(@TempSettingId is not null)              
    begin              
     update Enterprise.ProductSetting              
     set ThruDate = @now              
     where ProductSettingId=@TempSettingId              
                  
     update Enterprise.ProductConfiguration              
     set ThruDate = @now              
     where ProductSettingId=@TempSettingId and ConfigurationId=@ConfigurationId              
    END              
              
    --Add the Setting              
    INSERT INTO Enterprise.ProductSetting(ProductId, ProductSettingTypeId, Value, FromDate, ThruDate)              
    VALUES(@ProductId, @ProductSettingTypeId, @Value, @nowPlusSec, null)              
    SELECT @ProductSettingId = SCOPE_IDENTITY()              
              
    --Add the Setting config               
    INSERT INTO Enterprise.ProductConfiguration(ConfigurationId, ProductSettingId, FromDate, ThruDate)              
    VALUES(@ConfigurationId, @ProductSettingId, @nowPlusSec, null)              
   END        
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