CREATE Procedure [Enterprise].[ImportProduct]
 @ProductId INT,    -- ProductId              
 @productGUID NVARCHAR(50),  -- null for new product        
 @ProductTypeId INT, -- null for new product              
 @ParentProductTypeId INT, -- null for new product              
 @productTypeGUID uniqueidentifier, -- null for new product             
 @ProductName NVARCHAR(100),    -- Product Name used in tiles (UL Responsibility)              
 @ProductDescription NVARCHAR(2000),            
 @BooksProductCode NVARCHAR(10),   -- Books product code (Books Responsibility)                
 @AssignToAllUsers bit, -- Assign to all users      
 @UDMSourceCode nvarchar(20),      
 @LoginURI NVARCHAR(500),     --Product login page (Product Responsibility)
 @SigningCertificateThumbprint NVARCHAR(50)   -- for SAML products (UL Responsibility)          
 ,@ProductConfiguration ProductConfigurationType READONLY       
 ,@ProductPages [UserManagement].[ProductPageType] READONLY      
 ,@ProductControls [UserManagement].[ProductControlType] READONLY      
 ,@ProductControlsAttribute [UserManagement].[ProductControlAttributeType] READONLY      
 ,@ProductControlDependencies [UserManagement].[ProductControlDependencyType] READONLY  
 ,@CreatedBy BIGINT,    
 @Active bit    
AS            
                
BEGIN            
 BEGIN TRY         
  BEGIN TRAN      
      
    DECLARE @FromDate DATETIME = GETUTCDATE();        
    DECLARE @Rownum INT;        
    DECLARE @PStatus BIT,      
      @ProductPageId [int],      
     @DisplayName [nvarchar](510),      
     @ControlsAttributes [UserManagement].[ProductControlAttributeType]; 
	 
	 Create table #myTempTable1	 ( OldControlId int, NewControlId int)         
   ----------------------------------      
    --Create the product      
   ----------------------------------      
    exec [Enterprise].[SetProduct]      
      @ProductId output,      
      @productGUID output,            
      @ProductTypeId,       
      @ParentProductTypeId,      
      @productTypeGUID,          
      @ProductName,              
      @ProductDescription,      
      @BooksProductCode,        
      @AssignToAllUsers,      
      @UDMSourceCode,      
      @LoginURI,              
      @SigningCertificateThumbprint,    
      @Active    
   ----------------------------------      
    --Set Product Settings           
   ----------------------------------      
    IF (exists(SELECT top 1 1 FROM @ProductConfiguration) )      
    BEGIN        
          
     SELECT IDENTITY(INT, 1, 1) AS RowNum,        
      SettingName,        
      SettingDescription,        
      SettingValue,        
      SettingSensitiveData,        
      0 AS PStatus,        
      0 AS SStatus        
     INTO #ProdConfig        
     FROM @ProductConfiguration;        
      
          
     DECLARE @SettingName NVARCHAR(50);        
     DECLARE @SettingDescription NVARCHAR(100);        
     DECLARE @SettingValue NVARCHAR(1000);        
     DECLARE @ProductSettingID BIGINT;        
     DECLARE @ProductSettingTypeID INT;        
     DECLARE @SettingSensitiveData TINYINT;       
        
     WHILE EXISTS (SELECT 1 FROM #ProdConfig WHERE PStatus = 0)        
     BEGIN         
      SELECT TOP 1 @Rownum = RowNum,        
       @SettingName = SettingName,        
       @SettingDescription = SettingDescription,        
       @SettingValue = SettingValue,        
       @SettingSensitiveData = SettingSensitiveData,        
       @PStatus = PStatus        
      FROM #ProdConfig        
      WHERE PStatus = 0;        
          
      --The the type id      
      SET @ProductSettingId = NULL;        
      SET @ProductSettingTypeId = NULL;        
      SELECT @ProductSettingTypeId = ProductSettingTypeId        
       FROM   Enterprise.ProductSettingType        
       WHERE  [Name] = @SettingName;        
      
      --if the setting type doesn't exist create a new one      
      IF @ProductSettingTypeID IS NULL        
       EXEC Enterprise.CreateProductSettingType      
        @ProductSettingTypeName = @SettingName,        
        @ProductSettingTypeDescription = @SettingDescription,        
        @ProductSettingTypeSensitiveData = @SettingSensitiveData,        
        @ProductSettingTypeID = @ProductSettingTypeID OUTPUT;        
        
      --add the setting      
      EXEC [Enterprise].[SetProductSetting] @ProductSettingId= @ProductSettingId output,      
       @ProductId = @ProductId,       
       @ProductSettingTypeId = @ProductSettingTypeID,       
       @Value = @SettingValue      
       
      UPDATE #ProdConfig        
       SET PStatus = 1        
       WHERE RowNum = @RowNum;        
     END;       
     drop table #ProdConfig;      
    END;      
   ----------------------------------      
    --Create the temp table for product controls      
   ----------------------------------      
    IF (exists(SELECT top 1 1 FROM @ProductControls) )      
    BEGIN        
     SELECT IDENTITY(INT, 1, 1) AS RowNum,        
      ControlId,        
      ParentControlId,      
      ProductPageId,      
      ControlTypeId,      
      UIId,      
      DisplayName,      
      DataSource,      
      Sequence,      
      0 AS PStatus,        
      0 AS SStatus        
     INTO #ProdControl      
     FROM @ProductControls      
     order by ParentControlId;        
    end      
      
         
   ----------------------------------      
   --DELETE all the pages and controls before adding           
   ----------------------------------      
   exec [UserManagement].DeleteProductPagesAndControls @ProductId      
      
   ----------------------------------      
    --Set Product Pages           
   ----------------------------------      
    IF (exists(SELECT top 1 1 FROM @ProductPages) )      
    BEGIN            
     SELECT IDENTITY(INT, 1, 1) AS RowNum,       
      ProductPageId,      
      DisplayName,      
      IsActive,      
      ProductPageTypeId,      
      0 AS PStatus,        
      0 AS SStatus        
     INTO #ProdPages      
     FROM @ProductPages;      
          
     DECLARE        
      @IsActive bit,      
      @ProductPageTypeId int,      
      @OrginalProductPageId int      
     WHILE EXISTS (SELECT 1 FROM #ProdPages WHERE PStatus = 0)        
     BEGIN         
            
      SELECT TOP 1 @Rownum = RowNum,        
       @OrginalProductPageId = ProductPageId,      
       @DisplayName = DisplayName,      
       @IsActive = IsActive,      
       @ProductPageTypeId = ProductPageTypeId      
      FROM #ProdPages        
      WHERE PStatus = 0;        
          
      SET @ProductPageId = null            
      --add the setting      
      EXEC [UserManagement].[SetProductPage] @ProductPageId = @ProductPageId OUTPUT             
       ,@ProductId =@ProductId      
       ,@DisplayName =@DisplayName       
       ,@CreatedBy=@CreatedBy       
       ,@IsActive=@IsActive       
       ,@ProductPageTypeId=@ProductPageTypeId      
      
      --update the page id with the new page id      
      if(OBJECT_ID('tempdb..#ProdControl') is not null)      
      begin      
       update #ProdControl set ProductPageId = @ProductPageId where ProductPageId=@OrginalProductPageId      
      end      
             
       
      UPDATE #ProdPages        
       SET PStatus = 1        
       WHERE RowNum = @RowNum;        
     END;      
     drop table #ProdPages      
    END;      
      
   ----------------------------------      
    --Set Product Controls        
   ----------------------------------      
    IF (exists(SELECT top 1 1 FROM @ProductControls) )      
    BEGIN        
     DECLARE        
      @ControlId [int],      
      @NewControlId [int],      
      @ParentControlId [int],      
      @ControlTypeId [int],      
      @UIId [nvarchar](510),      
      @DataSource nvarchar(max),      
      @Sequence [tinyint]      
     WHILE EXISTS (SELECT 1 FROM #ProdControl WHERE PStatus = 0)        
     BEGIN         
            
		SELECT TOP 1 @Rownum = RowNum,        
       @ControlId = ControlId,        
       @ParentControlId = ParentControlId,      
       @ProductPageId = ProductPageId,      
       @ControlTypeId = ControlTypeId,      
       @UIId = UIId,      
       @DisplayName = DisplayName,      
       @DataSource = DataSource,      
       @Sequence = Sequence,      
       @PStatus = PStatus        
      FROM #ProdControl        
      WHERE PStatus = 0;        
          
		delete from @ControlsAttributes      
		insert into @ControlsAttributes      
		select * from @ProductControlsAttribute where ControlId=@ControlId   
		
		SET @NewControlId = NULL      
		--add the control      
		EXEC [UserManagement].[SetProductControl] @ControlId = @NewControlId OUTPUT        
       ,@ParentControlId =@ParentControlId      
       ,@ProductPageId=@ProductPageId      
       ,@ControlTypeId=@ControlTypeId       
       ,@UIId=@UIId       
       ,@DisplayName =@DisplayName       
       ,@DataSource=@DataSource       
       ,@Sequence=@Sequence       
       ,@CreatedBy=@CreatedBy      
      ,@ProductControlsAttribute = @ControlsAttributes  
      
	  
	  Insert into #myTempTable1
	  values (@ControlId, @NewControlId)

      -- update the old parent control id with the new control      
      update #ProdControl set ParentControlId=@NewControlId where ParentControlId=@ControlId      
       
      UPDATE #ProdControl        
       SET PStatus = 1        
       WHERE RowNum = @RowNum;        
     
	 END      
    END      
      
	Select * 
	into #myTempTable2 
	from @ProductControlDependencies


	Update #myTempTable2
	set #myTempTable2.MasterControlId = #myTempTable1.NewControlId
	from #myTempTable1 
	inner join #myTempTable2 on #myTempTable1.OldControlId = #myTempTable2.MasterControlId

	Update #myTempTable2
	set #myTempTable2.SlaveControlID = #myTempTable1.NewControlId
	from #myTempTable1 
	inner join #myTempTable2 on #myTempTable1.OldControlId = #myTempTable2.SlaveControlID

	insert into UserManagement.ControlDependency(MasterControlId,SlaveControlID,MasterControlValue, ComparatorId, CreatedBy, CreatedDate)      
	select MasterControlId, SlaveControlID, MasterControlValue, ComparatorId, @CreatedBy, GETDATE() from #myTempTable2      

	drop table #myTempTable2
	drop table #myTempTable1

    COMMIT      
    END TRY                
    BEGIN CATCH           
  print ERROR_MESSAGE()       
        ROLLBACK;            
        DECLARE @ErrorLogID INT;                
  EXEC dbo.LogError                
   @ErrorLogID = @ErrorLogID OUTPUT;              
        SELECT 0 AS Id ,                
                ErrorMessage                
        FROM   dbo.ErrorLog                
        WHERE  ErrorLogID = @ErrorLogID;                
                         
    END CATCH;                
END;