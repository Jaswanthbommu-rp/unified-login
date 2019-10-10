CREATE PROCEDURE [Enterprise].[CreateProductConfigurationbyPersonaId](    
  @PersonaId BIGINT
 ,@ConfigurationId INT
 ,@ProductId int    
 ,@ProductSettingID int    
 ,@FromDate datetime = NULL    
 ,@ThruDate datetime = NULL    
)    
    
AS    
BEGIN    
    
  SET NOCOUNT ON;    
  DECLARE @NOW datetime = GETUTCDATE();     
  DECLARE @ProductConfigurationId int = null    
  DECLARE @ProductSettingTypeId int = null   
  DECLARE @SettingValue int = null   
  DECLARE @ProductConfigurationIds TABLE ( ProductConfigurationId INT )  
  
  IF @Fromdate IS NULL    
  SET @FromDate = @NOW;    
    
  IF @ConfigurationId IS NULL    
  BEGIN    
   SELECT 0 AS Id, 'A valid record in Persona configuration with this PersonaId and ProductId must exist to extract the ConfigurationId before this procedure is called.' AS ErrorMessage    
  END    
   ELSE
       BEGIN 
         SELECT @ProductSettingTypeId = ProductSettingTypeId  
         FROM Enterprise.ProductSetting  
         WHERE ProductSettingId = @ProductSettingID  
  
		 -- now check for any other settings under the same configuration that have the same setting type and end them  
         INSERT INTO @ProductConfigurationIds ( ProductConfigurationId )  
         SELECT ProductConfigurationId  
         FROM Enterprise.ProductConfiguration PC  
         INNER JOIN Enterprise.ProductSetting PS  
          ON PC.ProductSettingId = PS.ProductSettingId  
         WHERE   
         PS.ProductSettingTypeId = @ProductSettingTypeId  
         AND PC.ConfigurationId = @ConfigurationId    
		 AND (PC.ThruDate IS NULL OR PC.ThruDate >= @NOW)		 

          BEGIN TRY    
          IF EXISTS ( SELECT TOP 1 'X' FROM @ProductConfigurationIds )  
          BEGIN    
           UPDATE PC   
           SET ThruDate = @NOW   
           FROM  
           Enterprise.ProductConfiguration PC   
           INNER JOIN @ProductConfigurationIds PCID  
            ON PC.ProductConfigurationId = PCID.ProductConfigurationId		
          END    
    
          INSERT INTO Enterprise.ProductConfiguration (ConfigurationId,ProductSettingId,FromDate,ThruDate)     
          OUTPUT Inserted.ProductConfigurationId AS Id, '' AS ErrorMessage    
          VALUES (@ConfigurationId, @ProductSettingID,@FromDate,@ThruDate);    
   
        END TRY 
         BEGIN CATCH    
  DECLARE @ErrorLogID INT;    
  EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;    
    
  SELECT 0 AS Id ,    
    ErrorMessage    
  FROM   dbo.ErrorLog    
  WHERE  ErrorLogID = @ErrorLogID;    
    
 END CATCH;      
END


END;