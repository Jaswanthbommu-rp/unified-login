CREATE PROCEDURE [Enterprise].[CreateOrganizationProductConfigurationbyPartyId](    
  @OrgPartyId BIGINT
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
    
  IF NOT EXISTS ( SELECT TOP 1 1 FROM Enterprise.Organization WHERE PartyId = @OrgPartyId )
  BEGIN
		PRINT 'Company not found'
		Return
  END

  IF @ConfigurationId IS NULL    
  BEGIN    
   SELECT 0 AS Id, 'A valid record in Product configuration with this PartyId and ProductId must exist to extract the ConfigurationId before this procedure is called.' AS ErrorMessage    
  END    
   ELSE
       BEGIN 
         SELECT @ProductSettingTypeId = ProductSettingTypeId  
         FROM Enterprise.ProductSetting  
         WHERE ProductSettingId = @ProductSettingID  
  		 
		INSERT INTO @ProductConfigurationIds ( ProductConfigurationId ) 
		SELECT PC.ProductConfigurationId
		FROM Enterprise.ProductSetting PS 
		INNER JOIN Enterprise.ProductConfiguration PC on 
			PS.ProductSettingId = PC.ProductSettingId
		INNER JOIN Enterprise.OrganizationProduct OP on 
			OP.ConfigurationId = PC.ConfigurationId 
			AND OP.PartyId = @OrgPartyId
		INNER JOIN Enterprise.ProductSettingType PST on 
			PS.ProductSettingTypeId = PST.ProductSettingTypeId 
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

			INSERT INTO Enterprise.OrganizationProduct ( PartyId, ConfigurationId, ProductId, FromDate )
			SELECT @OrgPartyID, @ConfigurationId, @ProductId, GETUTCDATE()
   
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