IF NOT EXISTS ( SELECT TOP (1) (1) FROM Enterprise.ThirdPartyRelationship )
BEGIN
	INSERT INTO [Enterprise].[ThirdPartyRelationship] (ThirdPartyRelationshipId, ThirdPartyRelationship )
		VALUES (1, 'Operator'),(2,'Owner'),(3,'Third Party Vendor')

END

GO
--USER STORY 1109853
IF NOT EXISTS( SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'EnableEditofPrimaryPropertyToggle')
BEGIN
	INSERT INTO Enterprise.ProductSettingType(Name, Description, SensitiveData)
	VALUES ('EnableEditofPrimaryPropertyToggle', 'Enable Edit of Primary Property Toggle When Operator setting is Enabled', 0)
END

GO

  IF NOT EXISTS (Select 1 From [Batch].[BatchProcessConfigurationType] Where BatchProcessConfigurationTypeId = 3)
  BEGIN
	Insert into [Batch].[BatchProcessConfigurationType] (BatchProcessConfigurationTypeId ,Name,Description)
	Select 3, 'PrimaryPropertiesBulkUpdateApiEndpoint' ,'API Endpoint to be invoked by batch processor'
  END  
GO
IF NOT EXISTS (Select 1 From [Batch].[BatchProcessConfiguration] Where BatchProcessConfigurationId = 3)
  BEGIN
	Declare @ServerName SYSNAME = @@SERVERNAME,
			@apiendpoint varchar(256)
	SET @apiendpoint = '';
	IF @ServerName IN ('RCDUSODBSQL001')  --DEV
	BEGIN
		SET @apiendpoint = 'https://my2dev.realpage.com/api/ppbatchprocessor';
	END
	IF @ServerName IN ('rctusodbsql001') --QA
	BEGIN
		SET @apiendpoint = 'https://my2qa.realpage.com/api/ppbatchprocessor';
	END
	IF @ServerName IN ('rcausodbsql001') --SAT
	BEGIN
		SET @apiendpoint = 'https://my2sat.realpage.com/api/ppbatchprocessor';
	END
	IF @ServerName IN ('RCTUSODBSQL001A','RCTUSODBSQL001B') --UAT
	BEGIN
		Set @apiendpoint = 'https://my2uat.realpage.com/api/ppbatchprocessor';
	END
	IF @ServerName IN ('RCIUSODBSQL002') --PREPROD
	BEGIN
		Set @apiendpoint = 'https://my2preprod.realpage.com/api/ppbatchprocessor';
	END

	IF @ServerName IN ('RCVGBKDBSQL001') --DEMO
	BEGIN
		Set @apiendpoint = 'https://my2demo.realpage.com/api/ppbatchprocessor';
	END

	IF @ServerName IN ('RCTUSODBTUL001') --TRAINING
	BEGIN
		Set @apiendpoint = 'https://my2qa.realpage.com/api/ppbatchprocessor';
	END

	IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') --PROD
	BEGIN
		Set @apiendpoint = 'https://my2.realpage.com/api/ppbatchprocessor';
	END

	IF @ServerName IN ('reagbkdbsql001') --EUSAT
	BEGIN
		SET @apiendpoint = 'https://my2sat.realpage.co.uk/api/ppbatchprocessor';
	END

	IF @ServerName IN ('repgbkdbsql001a','repgbkdbsql001b') --EUPROD
	BEGIN
		SET @apiendpoint = 'https://my2.realpage.co.uk/api/ppbatchprocessor';
	END

	Insert into [Batch].[BatchProcessConfiguration](BatchProcessConfigurationId, BatchProcessConfigurationTypeId, Value)
	Select 3,3, @apiendpoint
  END  
GO
IF NOT EXISTS (Select 1 From [Batch].[BatchProcessType] Where BatchProcessTypeId = 13)
  BEGIN
	 Insert Into [Batch].[BatchProcessType]( BatchProcessTypeId,BatchProcessConfigurationId,Description,Name)
	 Select 13, 3, 'Batch to create Primary Properties Bulk Update Users','PrimaryPropertiesBulkUpdateProductUser'
  END


GO
IF NOT EXISTS (Select 1 From [Batch].[BatchProcessType] Where BatchProcessTypeId = 14)
  BEGIN
	 Insert Into [Batch].[BatchProcessType]( BatchProcessTypeId,BatchProcessConfigurationId,Description,Name)
	 Select 14, 1, 'Batch to update user Primary Properties','PrimaryPropertiesUpdateProductUser'
  END
GO

DELETE FROM [UserManagement].[Control]
WHERE	UIId='UnifiedPlatformProductAccessAllowaccesstoallcurrentandfuturepropertiesPropertiesSwitchUIId'

GO
Declare @ServerName SYSNAME = @@SERVERNAME
	Declare @topicName1 varchar(256),@topicName2 varchar(256),@topicName3 varchar(256)
	SET @topicName1 = '';
	SET @topicName2 = '';
	IF @ServerName IN ('RCDUSODBSQL001')  --DEV
	BEGIN
		SET @topicName1 = 'RPUS-UNITY-USERS-INTERNAL-SYNC-DEV';
		SET @topicName2 = 'RPUS-UNITY-USERS-INTERNAL-PRODUCT-PROPERTY-SYNC-DEV';
		SET @topicName3 = 'RPUS-UNITY-USERS-DASHBOARD-DEV'
	END
	IF @ServerName IN ('rctusodbsql001') --QA
	BEGIN
		SET @topicName1 = 'RPUS-UNITY-USERS-INTERNAL-SYNC-QA';
		SET @topicName2 = 'RPUS-UNITY-USERS-INTERNAL-PRODUCT-PROPERTY-SYNC-QA';
		SET @topicName3 = 'RPUS-UNITY-USERS-DASHBOARD-QA'
	END
	IF @ServerName IN ('rcausodbsql001') --SAT
	BEGIN
		SET @topicName1 = 'RPUS-UNITY-USERS-INTERNAL-SYNC-SAT';
		SET @topicName2 = 'RPUS-UNITY-USERS-INTERNAL-PRODUCT-PROPERTY-SYNC-SAT';
		SET @topicName3 = 'RPUS-UNITY-USERS-DASHBOARD-SAT'
	END
	IF @ServerName IN ('RCTUSODBSQL001A','RCTUSODBSQL001B') --UAT
	BEGIN
		SET @topicName1 = 'RPUS-UNITY-USERS-INTERNAL-SYNC-UAT';
		SET @topicName2 = 'RPUS-UNITY-USERS-INTERNAL-PRODUCT-PROPERTY-SYNC-UAT';
		SET @topicName3 = 'RPUS-UNITY-USERS-DASHBOARD-UAT'
	END
	IF @ServerName IN ('RCIUSODBSQL002') --PREPROD
	BEGIN
		SET @topicName1 = 'RPUS-UNITY-USERS-INTERNAL-SYNC-PREPROD';
		SET @topicName2 = 'RPUS-UNITY-USERS-INTERNAL-PRODUCT-PROPERTY-SYNC-PREPROD';
		SET @topicName3 = 'RPUS-UNITY-USERS-DASHBOARD-PREPROD'
	END

	IF @ServerName IN ('RCVGBKDBSQL001') --DEMO
	BEGIN
		SET @topicName1 = 'RPUS-UNITY-USERS-INTERNAL-SYNC-DEMO';
		SET @topicName2 = 'RPUS-UNITY-USERS-INTERNAL-PRODUCT-PROPERTY-SYNC-DEMO';
		SET @topicName3 = 'RPUS-UNITY-USERS-DASHBOARD-DEMO'
	END

	IF @ServerName IN ('RCTUSODBTUL001') --TRAINING
	BEGIN
		SET @topicName1 = 'RPUS-UNITY-USERS-INTERNAL-SYNC-TRAINING';
		SET @topicName2 = 'RPUS-UNITY-USERS-INTERNAL-PRODUCT-PROPERTY-SYNC-TRAINING';
		SET @topicName3 = 'RPUS-UNITY-USERS-DASHBOARD-TRAINING'
	END

	IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') --PROD
	BEGIN
		SET @topicName1 = 'RPUS-UNITY-USERS-INTERNAL-SYNC-PROD';
		SET @topicName2 = 'RPUS-UNITY-USERS-INTERNAL-PRODUCT-PROPERTY-SYNC-PROD';
		SET @topicName3 = 'RPUS-UNITY-USERS-DASHBOARD-PROD'
	END

	IF @ServerName IN ('reagbkdbsql001') --EUSAT
	BEGIN
		SET @topicName1 = 'RPEU-UNITY-USERS-INTERNAL-SYNC-SAT';
		SET @topicName2 = 'RPEU-UNITY-USERS-INTERNAL-PRODUCT-PROPERTY-SYNC-SAT';
		SET @topicName3 = 'RPEU-UNITY-USERS-DASHBOARD-SAT'
	END

	IF @ServerName IN ('repgbkdbsql001a','repgbkdbsql001b') --EUPROD
	BEGIN
		SET @topicName1 = 'RPEU-UNITY-USERS-INTERNAL-SYNC-PROD';
		SET @topicName2 = 'RPEU-UNITY-USERS-INTERNAL-PRODUCT-PROPERTY-SYNC-PROD';
		SET @topicName3 = 'RPEU-UNITY-USERS-DASHBOARD-PROD'
	END

	  IF NOT EXISTS (Select 1 From [Enterprise].[UserSyncJobType] Where Name	= 'ProductCenterSync')
	  BEGIN
		 Insert Into [Enterprise].[UserSyncJobType](UserSyncJobTypeId,Description,Name,[KafkaTopicName])
		 Select 1, 'User Sync Process to update Product Centers','ProductCenterSync',@topicName1
	  END
	
	  IF NOT EXISTS (Select 1 From [Enterprise].[UserSyncJobType] Where Name	= 'PropertiesSync')
	  BEGIN
		 Insert Into [Enterprise].[UserSyncJobType](UserSyncJobTypeId,Description,Name,[KafkaTopicName])
		 Select 2, 'User Sync Process to translate and save product Primary Properties','PropertiesSync',@topicName2
	  END
	  IF NOT EXISTS (Select 1 From [Enterprise].[UserSyncJobType] Where Name	= 'UserDashboard')
	  BEGIN
		 Insert Into [Enterprise].[UserSyncJobType](UserSyncJobTypeId,Description,Name,[KafkaTopicName])
		 Select 3, 'User Sync Dashboard','UserDashboard',@topicName3
	  END
GO

	if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'UnifiedLoginApiBaseUri' )
	begin
		insert into enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'UnifiedLoginApiBaseUri', 'Unified Login Api Base Uri', 0)
	end
	Declare @ServerName SYSNAME = @@SERVERNAME,
			@apiendpoint varchar(256)
	SET @apiendpoint = '';
	IF @ServerName IN ('RCDUSODBSQL001')  --DEV
	BEGIN
		SET @apiendpoint = 'https://my2dev.realpage.com';
	END
	IF @ServerName IN ('rctusodbsql001') --QA
	BEGIN
		SET @apiendpoint = 'https://my2qa.realpage.com';
	END
	IF @ServerName IN ('rcausodbsql001') --SAT
	BEGIN
		SET @apiendpoint = 'https://my2sat.realpage.com';
	END
	IF @ServerName IN ('RCTUSODBSQL001A','RCTUSODBSQL001B') --UAT
	BEGIN
		Set @apiendpoint = 'https://my2uat.realpage.com';
	END
	IF @ServerName IN ('RCIUSODBSQL002') --PREPROD
	BEGIN
		Set @apiendpoint = 'https://my2preprod.realpage.com';
	END

	IF @ServerName IN ('RCVGBKDBSQL001') --DEMO
	BEGIN
		Set @apiendpoint = 'https://my2demo.realpage.com';
	END

	IF @ServerName IN ('RCTUSODBTUL001') --TRAINING
	BEGIN
		Set @apiendpoint = 'https://my2qa.realpage.com';
	END

	IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') --PROD
	BEGIN
		Set @apiendpoint = 'https://my2.realpage.com';
	END

	IF @ServerName IN ('reagbkdbsql001') --EUSAT
	BEGIN
		SET @apiendpoint = 'https://my2sat.realpage.co.uk';
	END

	IF @ServerName IN ('repgbkdbsql001a','repgbkdbsql001b') --EUPROD
	BEGIN
		SET @apiendpoint = 'https://my2.realpage.co.uk';
	END
	DECLARE @NOW DATETIME = GETUTCDATE();
	if not exists (
	select top 1 1 
		FROM Enterprise.GlobalProductConfiguration gpc  
		JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
		JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
		JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
			WHERE  gpc.ProductId = 3  
		AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		AND pst.Name = 'UnifiedLoginApiBaseUri'
		AND ps.Value = @apiendpoint
	)
	begin
		declare @currentproductconfigurationid INT
		select distinct top 1 @currentproductconfigurationid = pc.configurationid
			FROM Enterprise.GlobalProductConfiguration gpc  
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
				WHERE  gpc.ProductId = 3
			AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		order by pc.ConfigurationId desc

		if (@currentproductconfigurationid is not null)
		begin
			insert into enterprise.ProductSetting ( productid, ProductSettingTypeId, value, FromDate )
				select 3, productsettingtypeid, @apiendpoint, GETUTCDATE()
					from enterprise.ProductSettingType where name = 'UnifiedLoginApiBaseUri'

			insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				values ( @currentproductconfigurationid, @@IDENTITY, GETUTCDATE(), null )
		end
	end

	GO

	if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'ULInternalClientTokenScopes' )
	begin
		insert into enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'ULInternalClientTokenScopes', 'Unified Login Internal Api Client Token Scopes', 0)
	end
	DECLARE @NOW DATETIME = GETUTCDATE();
	if not exists (
	select top 1 1 
		FROM Enterprise.GlobalProductConfiguration gpc  
		JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
		JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
		JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
			WHERE  gpc.ProductId = 3  
		AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		AND pst.Name = 'ULInternalClientTokenScopes'
		AND ps.Value = 'enterpriseapi unifiedsettingsapi internalapi'
	)
	begin
		declare @currentproductconfigurationid INT
		select distinct top 1 @currentproductconfigurationid = pc.configurationid
			FROM Enterprise.GlobalProductConfiguration gpc  
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
				WHERE  gpc.ProductId = 3
			AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		order by pc.ConfigurationId desc

		if (@currentproductconfigurationid is not null)
		begin
			insert into enterprise.ProductSetting ( productid, ProductSettingTypeId, value, FromDate )
				select 3, productsettingtypeid, 'enterpriseapi unifiedsettingsapi internalapi rplandingapi', GETUTCDATE()
					from enterprise.ProductSettingType where name = 'ULInternalClientTokenScopes'

			insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				values ( @currentproductconfigurationid, @@IDENTITY, GETUTCDATE(), null )
		end
	end
	GO
GO

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'RestrictProductToSingleProperty' )
begin
	insert into enterprise.ProductSettingType ( name, Description, SensitiveData ) values ( 'RestrictProductToSingleProperty', 'Restrict property selection in product to one', 0)
end

DECLARE @NOW DATETIME = GETUTCDATE(); 
declare @productlist table ( entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(2000))
insert into @productlist values 
(13,'RestrictProductToSingleProperty','1')

	
--select * from @productlist

declare @MAX_ID INT
declare @Current_ID INT = 1
declare @CurrentProductId INT = 1

select @MAX_ID = max(entid) from @productlist

while @Current_ID <= @MAX_ID
begin
	declare @currentSettingType varchar(500)
	declare @currentsettingValue varchar(2000)

	select @CurrentProductId = productid , @currentSettingType = productsettingtype, @currentSettingValue = productsettingvalue
		from @productlist where entid = @Current_ID

	--print 'productid = ' + convert(varchar,@currentproductid)

	if not exists (
	select top 1 1 
		FROM Enterprise.GlobalProductConfiguration gpc  
		JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
		JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
		JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
			WHERE  gpc.ProductId = @CurrentProductId  
		AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		AND pst.Name = @currentSettingType
		AND ps.Value = @currentsettingValue
	)
	begin
		declare @currentproductconfigurationid INT
		select distinct top 1 @currentproductconfigurationid = pc.configurationid
			FROM Enterprise.GlobalProductConfiguration gpc  
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
				WHERE  gpc.ProductId = @CurrentProductId
			AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		order by pc.ConfigurationId desc

		if (@currentproductconfigurationid is not null)
		begin
			insert into enterprise.ProductSetting ( productid, ProductSettingTypeId, value, FromDate )
				select @CurrentProductId, productsettingtypeid, @currentSettingValue, GETUTCDATE()
					from enterprise.ProductSettingType where name = @currentSettingType
			insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				values ( @currentproductconfigurationid, @@IDENTITY, GETUTCDATE(), null )
		end
	end
	
	set @Current_ID = @Current_ID + 1
end

GO