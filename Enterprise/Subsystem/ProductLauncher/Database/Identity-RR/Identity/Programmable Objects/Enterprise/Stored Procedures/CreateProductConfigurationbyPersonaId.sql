IF OBJECT_ID('[Enterprise].[CreateProductConfigurationbyPersonaId]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[CreateProductConfigurationbyPersonaId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[CreateProductConfigurationbyPersonaId](    
  @PersonaId int    
 ,@ProductId int    
 ,@ProductSettingID int    
 ,@FromDate datetime = NULL    
 ,@ThruDate datetime = NULL    
)    
    
AS    
BEGIN    
    
  SET NOCOUNT ON;    
  DECLARE @NOW datetime = GETUTCDATE();    
  DECLARE @ConfigurationId int = NULL    
  DECLARE @ProductConfigurationId int = null    
  DECLARE @ProductSettingTypeId int = null    
     DECLARE @ProductConfigurationIds TABLE ( ProductConfigurationId INT )  
  
  IF @Fromdate IS NULL    
  SET @FromDate = @NOW;    
    
  --the persona configuration MUST be set up before this procedure is called    
  SELECT @ConfigurationId = ConfigurationId     
  FROM Enterprise.PersonaConfiguration     
  WHERE PersonaId = @PersonaId AND ProductId = @ProductId    
  AND ((@NOW BETWEEN FromDate AND ThruDate) OR (@NOW >= dateadd(mi, -1, FromDate) AND ThruDate IS NULL))    
    
  IF @ConfigurationId IS NULL    
  BEGIN    
   SELECT 0 AS Id, 'A valid record in Persona configuration with this PersonaId and ProductId must exist to extract the ConfigurationId before this procedure is called.' AS ErrorMessage    
  END    
   ELSE
       BEGIN 
         SELECT @ProductSettingTypeId = ProductSettingTypeId  
         FROM Enterprise.ProductSetting  
         WHERE ProductSettingId = @ProductSettingID  
  
         -- check for the existing product setting  
         INSERT INTO @ProductConfigurationIds ( ProductConfigurationId )  
         SELECT ProductConfigurationId     
         FROM Enterprise.ProductConfiguration    
         WHERE   
         ProductSettingId = @ProductSettingID    
         AND ConfigurationId = @ConfigurationId    
  
         -- now check for any other settings under the same configuration that have the same setting type and end them  
         INSERT INTO @ProductConfigurationIds ( ProductConfigurationId )  
         SELECT ProductConfigurationId  
         FROM Enterprise.ProductConfiguration PC  
         INNER JOIN Enterprise.ProductSetting PS  
          ON PC.ProductSettingId = PS.ProductSettingId  
         WHERE   
         PS.ProductSettingTypeId = @ProductSettingTypeId  
         AND PC.ConfigurationId = @ConfigurationId     

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
GO
