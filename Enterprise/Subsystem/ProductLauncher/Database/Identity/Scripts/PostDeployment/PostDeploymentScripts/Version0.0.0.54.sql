
--DECLARE @RoleId INT
DECLARE @TitleUniqueId UNIQUEIDENTIFIER
--DECLARE @ParentProductTypeId INT
--DECLARE @ActionValueId INT
--DECLARE @RightValueTypeId INT
--DECLARE @ProductTypeId INT
--DECLARE @ProductId INT
--DECLARE @FromDate DATETIME
DECLARE @SamlProductSettingId INT
--DECLARE @ConfigurationId INT
--DECLARE @ClientId INT
--DECLARE @ProductSettingId INT
--DECLARE @ProductSettingTypeId INT
--DECLARE @PartyId BIGINT
--DECLARE @PersonaId INT
--DECLARE @RightId INT
--DECLARE @ActionId INT
--DECLARE @RightActionId INT
--DECLARE @RouteActionId INT
--DECLARE @UserActionId INT
--DECLARE @Status INT
--DECLARE @RightCategoryId INT
-- Step 1 - In case product type is missing, add one

-- Removed because not needed

-- Step 2 - Add product

SET @ProductId = 27

IF NOT EXISTS(SELECT 1 FROM Enterprise.Product WHERE Name = 'Migration Tool Application')
BEGIN
	EXEC Enterprise.CreateProduct @ProductId, NULL, 'Migration Tool Application', 'Migration Tool Application', NULL
END

---- Step 3 - Add all settings

SET @FromDate = GETUTCDATE()

IF NOT EXISTS(SELECT 1 FROM Ident.SamlProductSettings WHERE ProductId = @ProductId)
BEGIN
	EXEC Ident.CreateSamlProductSetting @ProductId, 'https://ulmtdev.corp.realpage.com', 'NA', 'productUsername', @SamlProductSettingId OUTPUT
END

IF NOT EXISTS(SELECT 1 FROM Enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId)
BEGIN
	EXEC Enterprise.CreateProductConfiguration @ConfigurationId OUTPUT

	SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'migration'

	--***** Start a Product setting loop for each attribute / value that needs set. *****
	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ClientId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = @clientID,                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

		------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ClassName',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'migrationtool',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'TitleId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'Migration Tool',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	SET @TitleUniqueId = NEWID()

	EXEC Enterprise.GetProductSettingType
		@Name = 'TitleUniqueId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = @TitleUniqueId,                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'IsNewTab',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'1',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'MetatagUniqueId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'MigrationTool',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'IsFavorite',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'0',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'IsResource',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'1',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'LearnMore',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'https://www.realpage.com/',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ProductStatus',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'8',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ProductUrl',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'/product/migrationtool',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime


	------------------------------------------------------------------------------------------------------------------------------------------------------
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId

END

-- Step 4 - Add new Route




SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Login';

SELECT @ActionValueID = [ActionValueTypeID]
FROM Enterprise.ActionValueType
WHERE Value = 'Route'; 

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'MigrationTool' AND 
		  ParentActionId IS NULL AND 
		  Description = 'SuperUser'
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'MigrationTool', @ActionTarget = N'Route', @ActionbValueTypeId = @ActionValueID, @Description = '', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Migration Tool' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Migration Tool', @ActionTarget = N'Right', @ActionbValueTypeId = @ActionValueID, @Description = '', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;



DECLARE @DashboardParentId INT=
(
    SELECT ActionId
    FROM Enterprise.ACTION
    WHERE ObjectValue = 'Dashboard'
          AND ObjectType = 'Route'
          AND Description = 'SuperUser'
);
DECLARE @DashboardAVT INT=
(
    SELECT ActionValueTypeId FROM Enterprise.ActionValueType WHERE Value = 'Route'
);
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.ACTION
    WHERE ObjectValue = 'Migration Tool'
          AND ParentActionId = @DashboardParentId
)
    EXEC Enterprise.CreateAction
         @ParentActionId = @DashboardParentId,
         @ProductId = @ProductId,
         @Action = 'Migration Tool',
         @ActionTarget ='Right',
	   @ActionbValueTypeId = @DashboardAVT,
	   @Description = Null,
	   @ActionId = @ActionID OUTPUT;


-- Step 6 - Assign product and right to organizations
SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE [Value] = 'Ability to Migrate Users';
SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
     JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
     JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
     JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'ALL'
      AND StatusTypeCategoryType.Name = 'Security';

SELECT @RightCategoryId = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
	 JOIN
	 Enterprise.StatusTypeCategory
	 ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification
	 ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType
	 ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'DEfault' AND 
	  StatusTypeCategory.Name = 'Right Type' AND 
	  StatusTypeCategoryType.Name = 'Security';

DECLARE OrgProduct CURSOR FOR
SELECT DISTINCT 
          OrganizationPartyID
FROM Person.Persona; 
-- WHERE Name IN ('') -- List organization names here (Per DJ this should be added to all organizations)
SET @PartyId = NULL;
OPEN OrgProduct;
FETCH NEXT FROM OrgProduct INTO @PartyId;
WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @RightId = NULL;
        SET @ProductId = 27
		EXEC Enterprise.CreateOrganizationProduct
             @PartyId = @PartyId,
             @ConfigurationId = @ConfigurationId,
             @ProductId = @ProductId,
             @FromDate = @FromDate,
             @ThruDate = NULL;
        SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Unified Login';
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_MigrationTool_Route', @ShortName = 'MigrationToolRoute', @RightCategoryId = @RightCategoryId, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_MigrationTool_Right', @ShortName = 'MigrationToolRight', @RightCategoryId = @RightCategoryId, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
	--Create Default Right
	
    SELECT @RoleID = RoleId
	   FROM Enterprise.Role
	   R INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId  WHere value = 'User Administrator' and PartyId = @PartyId
	   Execute Enterprise.CreateRight @RoleId = @RoleId, @PartyId = @PartyId,  @ProductId=3,@RightName='Ability to Migrate Users', @RightCategoryId = @RightCategoryId,  @RightID = @RightID OUTPUT,  @Description = 'May default to Internal Only so we can suppress for new customers.Once customers are adopted + 90 days this should no longer be available #discuss'

	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Migration Tool' AND 
		  ObjectType = 'Route' AND 
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_MigrationTool_Route' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	
	
	SELECT @ActionID = ActionID
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Migratio Tool' AND 
		  ObjectType = 'Right' AND 
		  ParentActionId IS NULL;
	SELECT @RightID = RightId
	FROM Enterprise.[Right] AS R
		 INNER JOIN
		 Enterprise.RightValueType AS RVT
		 ON RVT.RightValueTypeId = R.RightValueTypeId
	WHERE Value = 'Default_MigrationTool_Right' AND 
		  RoleId = -1;
	EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

        FETCH NEXT FROM OrgProduct INTO @PartyID;
    END;
CLOSE OrgProduct;
DEALLOCATE OrgProduct;
-- Step 7 - Assign product to all superusers
SET @ProductId = 27
DECLARE PersonaConfiguration CURSOR
FOR SELECT P.PersonaId FROM Enterprise.PartyRelationship PR
    INNER JOIN Person.Persona P
	   ON PR.PartyIdFrom = P.PersonPartyId
    WHERE RoleTypeIdFrom = 402
SET @PersonaId = NULL;
OPEN PersonaConfiguration;
FETCH NEXT FROM PersonaConfiguration INTO @PersonaId;
WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC Enterprise.CreatePersonaConfiguration
             @PersonaId,
             @ProductId,
             @FromDate,
             NULL;
        FETCH NEXT FROM PersonaConfiguration INTO @PersonaId;
    END;
CLOSE PersonaConfiguration;
DEALLOCATE PersonaConfiguration;



DECLARE @MigrationToolRoute INT;
DECLARE @MigrationToolRight INT;

SELECT @MigrationToolRoute = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_MigrationTool_Route';

SELECT @MigrationToolRight = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_MigrationTool_Right';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN ('Ability to Migrate Users');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @MigrationToolRoute
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @MigrationToolRoute );
END;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @MigrationToolRight
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @MigrationToolRight );
END;

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='55'