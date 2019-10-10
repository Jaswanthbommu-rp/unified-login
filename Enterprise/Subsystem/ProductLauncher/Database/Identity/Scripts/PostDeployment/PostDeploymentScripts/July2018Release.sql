GO
IF NOT EXISTS(SELECT 1 FROM Enterprise.Role WHERE DefaultRole = 1)
BEGIN
UPDATE R
  SET
      DefaultROle = 1
FROM Enterprise.Role R
     INNER JOIN Enterprise.RoleValueType RT ON RT.RoleValueTypeId = R.ROleValueTYpeId
WHERE RT.Value = 'Basic End User';
END

GO
UPDATE Enterprise.Product SET BooksProductCode = 'PUPDATE' WHERE ProductID = 28 AND BooksProductCode != 'PUPDATE'

GO

/************************
PROPERTY PHOTOS
************************/
GO
PRINT 'Begin Property Photos'
DECLARE @ProductName nvarchar(100) = 'Property Photos',
	@ProductId int,
	@FromDate datetime,
	@ProductSettingId int,
	@ProductSettingTypeId int,
	@ConfigurationId int

SET @ProductId = 37
SET @FromDate = GETUTCDATE()

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.Product WHERE ProductId = @ProductId AND Name = @ProductName)
BEGIN
	INSERT INTO Enterprise.Product (
		ProductId,
		ProductGUID,
		Name,
		Description,
		ProductTypeId,
		BooksProductCode
	)
	SELECT	@ProductId, --ProductId
					N'CEB45B15-B4D9-4DA8-B6E2-C26FC68E4552', --ProductGUID
					@ProductName, --Name
					@ProductName, --Description
					NULL AS ProductTypeID,
					'PHOTO'
	
	EXEC Enterprise.CreateProductConfiguration @ConfigurationId = @ConfigurationId OUTPUT -- int

	--***** Start a Product setting loop for each attribute / value that needs set. *****
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
		@Value = @ProductName,                               -- nvarchar(1000)
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
		@Name = 'TitleUniqueId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'83AB5A0D-1592-413A-A111-475FB30A7A08',                               -- nvarchar(1000)
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
		@Value = @ProductName,                               -- nvarchar(1000)
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
		@Value = N'/product/propertyphotos',                            -- nvarchar(1000)
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
		@Name = 'ClientId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'propertyphotos',                               -- nvarchar(1000)
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
		@Value = N'propertyphotos',                               -- nvarchar(1000)
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
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Ident.SamlProductSettings WHERE ProductId = @ProductId)
BEGIN
	INSERT INTO Ident.SamlProductSettings (
		ProductId,
		LoginUri,
		SigningCertificateThumbprint,
		SubjectIdSamlAttribute
	)
	VALUES (
		@ProductId, --ProductId
		N'https://beta.propertyphotos.com/greenbook/login', --LoginUri
		N'NA', --SigningCertificateThumbprint
		N'productUsername' --SubjectIdSamlAttribute
	)
END
PRINT 'End Property Photos'
GO
--Property Photos RIghts

DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;
DECLARE @TRightShortName NVARCHAR(100)
IF OBJECT_ID('tempdb..#RightsUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #RightsUnifiedSettings;
END;

IF OBJECT_ID('tempdb..#HoldPartyForUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyForUnifiedSettings;
END;

CREATE TABLE #RightsUnifiedSettings
( 
			 RightId int, Name nvarchar(500), description nvarchar(500), shortname varchar(100)
);

INSERT INTO #RightsUnifiedSettings( rightid, name, description, shortname )
VALUES( 1, 'Access to Property Photos', 'Access to Property Photos', 'AccessPropertyPhotos' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

SELECT @ActionValueID = [ActionValueTypeID]
FROM Enterprise.ActionValueType
WHERE Value = 'Right';

SELECT @RoleCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'Default';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'Default';

SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
     JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
     JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
     JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'All'
      AND StatusTypeCategoryType.Name = 'Security';



IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Property Photos' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Property Photos', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = 'Access to property photos', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'DashBoard' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Property Photos' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Property Photos', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;





SELECT 
	DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
WHERE O.Name IN ( 'Fairfield Residential Company LLC', 'Summit Management LLC');

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = 'User Administrator' AND 
		  R.PartyId = @PartyId;
	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #RightsUnifiedSettings;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_PropertyPhotos', @ShortName = 'PropertyPhotos', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = 'Access to property photos', @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = '', @Description = @TRightDesc, @RightId = @RightId OUTPUT;
		
		
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Property Photos' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_PropertyPhotos';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Property Photos' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		
		

		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

DECLARE @Dashboard INT
DECLARE @PropertyPhotos INT

SELECT  @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType 
WHERE value = 'Default_Dashboard_Users';

SELECT  @PropertyPhotos = RightValueTypeId
FROM Enterprise.RightValueType 
WHERE value = 'Default_PropertyPhotos';


SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Access to Property Photos');



IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @PropertyPhotos
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @DashBoard );
		INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @PropertyPhotos );
END;

-- GB-1187 - Add default record to BatchType table

--IF NOT EXISTS (SELECT 1 FROM Enterprise.BatchType WHERE BatchTypeId = 0)
--BEGIN
--	INSERT INTO Enterprise.BatchType (BatchTypeId, Description)
--	VALUES (0, 'Default Product Batch')
--END
--GO


/************************
VENDOR MARKETPLACE
************************/
GO
PRINT 'Begin Vendor Marketplace'
DECLARE @ProductName nvarchar(100) = 'Vendor Marketplace',
	@ProductId int,
	@FromDate datetime,
	@ProductSettingId int,
	@ProductSettingTypeId int,
	@ConfigurationId int

SET @ProductId = 38
SET @FromDate = GETUTCDATE()

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.Product WHERE ProductId = @ProductId AND Name = @ProductName)
BEGIN
	INSERT INTO Enterprise.Product (
		ProductId,
		ProductGUID,
		Name,
		Description,
		ProductTypeId,
		BooksProductCode
	)
	SELECT	@ProductId, --ProductId
					N'4BE65AA4-4B79-4C1C-B1EF-A00A2EB36926', --ProductGUID
					@ProductName, --Name
					@ProductName, --Description
					NULL AS ProductTypeID,
					'VMP'
	
	EXEC Enterprise.CreateProductConfiguration @ConfigurationId = @ConfigurationId OUTPUT -- int

	--***** Start a Product setting loop for each attribute / value that needs set. *****
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
		@Value = @ProductName,                               -- nvarchar(1000)
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
		@Name = 'TitleUniqueId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'52EA5E2A-BC99-40A8-A640-1DCB58B915D6',                               -- nvarchar(1000)
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
		@Value = @ProductName,                               -- nvarchar(1000)
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
		@Value = N'/product/vendormarketplace',                            -- nvarchar(1000)
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
		@Name = 'ClientId',                                         -- varchar(50)
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT -- int

	-- Create the Value and assign it to the Product and ProductSettingType
	EXEC Enterprise.CreateProductSetting
		@ProductId = @ProductId,                             -- int
		@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
		@Value = N'vendormarketplace',                               -- nvarchar(1000)
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
		@Value = N'vendormarketplace',                               -- nvarchar(1000)
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
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

	EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Ident.SamlProductSettings WHERE ProductId = @ProductId)
BEGIN
	INSERT INTO Ident.SamlProductSettings (
		ProductId,
		LoginUri,
		SigningCertificateThumbprint,
		SubjectIdSamlAttribute
	)
	VALUES (
		@ProductId, --ProductId
		N'https://vmpuidev.compliancedepot.com', --LoginUri
		N'NA', --SigningCertificateThumbprint
		N'productUsername' --SubjectIdSamlAttribute
	)
END
PRINT 'End VendorMarketplace'
GO

SET NOCOUNT ON

DECLARE @ProductId INT, 
		@ShowInUserDetails TINYINT, 
		@RequiresUnifiedLoginRight VARCHAR(100),
		@ShowInRolesAndRights TINYINT, 
		@ShowInAppSwitcher TINYINT, 
		@ShowInUserListFilter TINYINT, 
		@ProductAPIRequiresUser TINYINT,
		@CurrentProductConfigurationID INT,
		@ProductSettingTypeId INT,
		@ProductSettingId INT

IF (SELECT 1 FROM Enterprise.ProductSettingType WHERE Name = 'ShowInUserDetails') IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'ShowInUserDetails', 'Should the product show in the New/Edit user pages', @ProductSettingTypeId OUTPUT;
END;
IF (SELECT 1 FROM Enterprise.ProductSettingType WHERE Name = 'RequiresUnifiedLoginRight') IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'RequiresUnifiedLoginRight', 'What right(s) are required to use the product', @ProductSettingTypeId OUTPUT;
END;
IF (SELECT 1 FROM Enterprise.ProductSettingType WHERE Name = 'ShowInRolesAndRights') IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'ShowInRolesAndRights', 'Should the product show in the Role/Rights page', @ProductSettingTypeId OUTPUT;
END;
IF (SELECT 1 FROM Enterprise.ProductSettingType WHERE Name = 'ShowInAppSwitcher') IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'ShowInAppSwitcher', 'Should the product show in the application switcher', @ProductSettingTypeId OUTPUT;
END;
IF (SELECT 1 FROM Enterprise.ProductSettingType WHERE Name = 'ShowInUserListFilter') IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'ShowInUserListFilter', 'Should the product show in the user list product pick list', @ProductSettingTypeId OUTPUT;
END;
IF (SELECT 1 FROM Enterprise.ProductSettingType WHERE Name = 'ProductAPIRequiresUser') IS NULL
BEGIN
	EXEC Enterprise.CreateProductSettingType 'ProductAPIRequiresUser', 'Does the product require a user for api calls', @ProductSettingTypeId OUTPUT;
END;

DECLARE @ProductSetting TABLE ( ProductId INT, ShowInUserDetails tinyint, RequiresUnifiedLoginRight varchar(100), ShowInRolesAndRights tinyint, ShowInAppSwitcher tinyint, ShowInUserListFilter tinyint, ProductAPIRequiresUser tinyint)
INSERT INTO @ProductSetting ( 
				ProductId, ShowInUserDetails, RequiresUnifiedLoginRight, ShowInRolesAndRights, ShowInAppSwitcher, ShowInUserListFilter, ProductAPIRequiresUser )
	VALUES  (	1,			1,					'',							1,					1,					1,					0 ), -- OneSite
			(	3,			1,					'',							1,					0,					0,					0 ), -- Unified Platform
			(	4,			1,					'',							0,					0,					0,					0 ), -- Asset Optimization
			(	6,			1,					'',							0,					1,					1,					0 ), -- Lead2Lease
			(	7,			1,					'',							0,					1,					1,					0 ), -- NOT USED
			(	8,			1,					'',							1,					1,					1,					1 ), -- RealPage Accounting
			(	9,			1,					'',							0,					1,					1,					1 ), -- Marketing Center
			(	10,			1,					'',							0,					1,					1,					0 ), -- Prospect Contact Center
			(	13,			1,					'',							1,					1,					1,					1 ), -- Spend Management
			(	14,			1,					'',							0,					1,					1,					0 ), -- Client Portal
			(	15,			1,					'',							0,					1,					1,					1 ), -- Renters Insurance
			(	16,			1,					'',							0,					1,					1,					0 ), -- Vendor Services
			(	17,			1,					'',							0,					1,					1,					1 ), -- Resident Portals
			(	18,			1,					'',							0,					1,					1,					0 ), -- Utility Management
			(	19,			0,					'',							0,					0,					0,					0 ), -- Product Learning Portal
			(	20,			1,					'',							0,					1,					0,					0 ), -- RealPage Document Management
			(	21,			0,					'',							0,					0,					0,					0 ), -- OneSite Conversions
			(	23,			1,					'',							0,					1,					1,					0 ), -- On-Site
			(	24,			1,					'',							0,					0,					0,					0 ), -- Research Application
			(	25,			1,					'',							0,					0,					0,					0 ), -- Self-provisioning portal
			(	26,			1,					'',							0,					1,					1,					0 ), -- Unified Amenities
			(	27,			0,					'',							0,					0,					0,					0 ), -- Migration Tool Application
			(	28,			0,					'',							0,					0,					0,					0 ), -- Product Updates
			(	29,			1,					'',							0,					1,					1,					0 ), -- Business Intelligence
			(	30,			1,					'',							0,					1,					1,					0 ), -- Performance Analytics
			(	31,			1,					'',							0,					1,					1,					0 ), -- Investment Analytics
			(	32,			1,					'',							0,					1,					1,					0 ), -- Revenue Management
			(	33,			0,					'',							0,					1,					0,					0 ), -- Axiometrics
			(	34,			0,					'',							0,					0,					0,					0 ), -- Benchmarking
			(	35,			0,					'',							0,					0,					0,					0 ), -- Support Tool
			(	36,			0,					'',							0,					0,					0,					0 ), -- EasyLMS
			(	37,			0,					'ACCESSPROPERTYPHOTOS',		0,					1,					1,					0 ), -- PropertyPhotos
			(	38,			0,					'ACCESSVENDORMARKETPLACE',	0,					1,					1,					0 ) -- VendorMarketplace


DECLARE @NOW DATETIME = GETUTCDATE();

declare Products CURSOR 
FOR SELECT ProductId, ShowInUserDetails, RequiresUnifiedLoginRight, ShowInRolesAndRights, ShowInAppSwitcher, ShowInUserListFilter, ProductAPIRequiresUser 
	from @ProductSetting

OPEN Products
Fetch Products INTO @ProductId, @ShowInUserDetails, @RequiresUnifiedLoginRight, @ShowInRolesAndRights, @ShowInAppSwitcher, @ShowInUserListFilter, @ProductAPIRequiresUser 
WHILE @@FETCH_STATUS = 0
BEGIN
	SELECT top 1 @CurrentProductConfigurationID = ConfigurationId         
		FROM Enterprise.GlobalProductConfiguration gpc  
        WHERE  gpc.ProductId = @ProductId  
    AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
	ORDER BY GlobalProductConfigurationId DESC

	
	--- ShowInUserDetails 
	SET @ProductSettingTypeId = NULL
	EXEC Enterprise.GetProductSettingType
		@Name = 'ShowInUserDetails',
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT

	IF @ProductSettingTypeId IS NOT NULL AND NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSetting where ProductID = @productId AND ProductSettingTypeId = @ProductSettingTypeId AND ThruDate IS NULL)
	BEGIN
	
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting
			@ProductId = @ProductId,                             -- int
			@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
			@Value = @ShowInUserDetails,                               -- nvarchar(1000)
			@FromDate = @NOW,          -- datetime
			@ThruDate = NULL,          -- datetime
			@ProductSettingId = @ProductSettingId OUTPUT -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration
			@ConfigurationId = @CurrentProductConfigurationID,              -- int
			@ProductSettingId = @ProductSettingId,             -- int
			@FromDate = @NOW, -- datetime
			@ThruDate = NULL   -- datetime
	END
	--- ShowInUserDetails 

	--- RequiresUnifiedLoginRight 
	SET @ProductSettingTypeId = NULL
	EXEC Enterprise.GetProductSettingType
		@Name = 'RequiresUnifiedLoginRight',
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT

	IF @ProductSettingTypeId IS NOT NULL AND @RequiresUnifiedLoginRight != '' AND NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSetting where ProductID = @productId AND ProductSettingTypeId = @ProductSettingTypeId AND ThruDate IS NULL)
	BEGIN
		
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting
			@ProductId = @ProductId,                             -- int
			@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
			@Value = @RequiresUnifiedLoginRight,                               -- nvarchar(1000)
			@FromDate = @NOW,          -- datetime
			@ThruDate = NULL,          -- datetime
			@ProductSettingId = @ProductSettingId OUTPUT -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration
			@ConfigurationId = @CurrentProductConfigurationID,              -- int
			@ProductSettingId = @ProductSettingId,             -- int
			@FromDate = @NOW, -- datetime
			@ThruDate = NULL   -- datetime
	END
	--- RequiresUnifiedLoginRight
	
	--- ShowInRolesAndRights 
	SET @ProductSettingTypeId = NULL
	EXEC Enterprise.GetProductSettingType
		@Name = 'ShowInRolesAndRights',
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT

	IF @ProductSettingTypeId IS NOT NULL AND NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSetting where ProductID = @productId AND ProductSettingTypeId = @ProductSettingTypeId AND ThruDate IS NULL)
	BEGIN
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting
			@ProductId = @ProductId,                             -- int
			@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
			@Value = @ShowInRolesAndRights,                               -- nvarchar(1000)
			@FromDate = @NOW,          -- datetime
			@ThruDate = NULL,          -- datetime
			@ProductSettingId = @ProductSettingId OUTPUT -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration
			@ConfigurationId = @CurrentProductConfigurationID,              -- int
			@ProductSettingId = @ProductSettingId,             -- int
			@FromDate = @NOW, -- datetime
			@ThruDate = NULL   -- datetime
	END
	--- ShowInRolesAndRights	
	 
	--- ShowInAppSwitcher 
	SET @ProductSettingTypeId = NULL
	EXEC Enterprise.GetProductSettingType
		@Name = 'ShowInAppSwitcher',
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT

	IF @ProductSettingTypeId IS NOT NULL AND NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSetting where ProductID = @productId AND ProductSettingTypeId = @ProductSettingTypeId AND ThruDate IS NULL)
	BEGIN
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting
			@ProductId = @ProductId,                             -- int
			@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
			@Value = @ShowInAppSwitcher ,                               -- nvarchar(1000)
			@FromDate = @NOW,          -- datetime
			@ThruDate = NULL,          -- datetime
			@ProductSettingId = @ProductSettingId OUTPUT -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration
			@ConfigurationId = @CurrentProductConfigurationID,              -- int
			@ProductSettingId = @ProductSettingId,             -- int
			@FromDate = @NOW, -- datetime
			@ThruDate = NULL   -- datetime
	END
	--- ShowInAppSwitcher 	

	--- ShowInUserListFilter 
	SET @ProductSettingTypeId = NULL
	EXEC Enterprise.GetProductSettingType
		@Name = 'ShowInUserListFilter',
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT

	IF @ProductSettingTypeId IS NOT NULL AND NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSetting where ProductID = @productId AND ProductSettingTypeId = @ProductSettingTypeId AND ThruDate IS NULL)
	BEGIN
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting
			@ProductId = @ProductId,                             -- int
			@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
			@Value = @ShowInUserListFilter ,                               -- nvarchar(1000)
			@FromDate = @NOW,          -- datetime
			@ThruDate = NULL,          -- datetime
			@ProductSettingId = @ProductSettingId OUTPUT -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration
			@ConfigurationId = @CurrentProductConfigurationID,              -- int
			@ProductSettingId = @ProductSettingId,             -- int
			@FromDate = @NOW, -- datetime
			@ThruDate = NULL   -- datetime
	END
	--- ShowInUserListFilter 	

	--- ProductAPIRequiresUser 
	SET @ProductSettingTypeId = NULL
	EXEC Enterprise.GetProductSettingType
		@Name = 'ProductAPIRequiresUser',
		@ProductSettingTypeId = @ProductSettingTypeId OUTPUT

	IF @ProductSettingTypeId IS NOT NULL AND NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSetting where ProductID = @productId AND ProductSettingTypeId = @ProductSettingTypeId AND ThruDate IS NULL)
	BEGIN
		-- Create the Value and assign it to the Product and ProductSettingType
		EXEC Enterprise.CreateProductSetting
			@ProductId = @ProductId,                             -- int
			@ProductSettingTypeId = @ProductSettingTypeId,                  -- int
			@Value = @ProductAPIRequiresUser ,                               -- nvarchar(1000)
			@FromDate = @NOW,          -- datetime
			@ThruDate = NULL,          -- datetime
			@ProductSettingId = @ProductSettingId OUTPUT -- int

		-- Link the Product Setting to an actual configuration
		EXEC Enterprise.LinkProductSettingToConfiguration
			@ConfigurationId = @CurrentProductConfigurationID,              -- int
			@ProductSettingId = @ProductSettingId,             -- int
			@FromDate = @NOW, -- datetime
			@ThruDate = NULL   -- datetime
	END
	--- ProductAPIRequiresUser 	

	Fetch Products INTO @ProductId, @ShowInUserDetails, @RequiresUnifiedLoginRight, @ShowInRolesAndRights, @ShowInAppSwitcher, @ShowInUserListFilter, @ProductAPIRequiresUser 
END
GO



DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;
DECLARE @TRightShortName NVARCHAR(100)
IF OBJECT_ID('tempdb..#RightsUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #RightsUnifiedSettings;
END;

IF OBJECT_ID('tempdb..#HoldPartyForUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyForUnifiedSettings;
END;

CREATE TABLE #RightsUnifiedSettings
( 
			 RightId int, Name nvarchar(500), description nvarchar(500), shortname varchar(100)
);

INSERT INTO #RightsUnifiedSettings( rightid, name, description, shortname )
VALUES( 1, 'Access to Vendor Marketplace', 'Access to Vendor Marketplace', 'AccessVendorMarketplace' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

SELECT @ActionValueID = [ActionValueTypeID]
FROM Enterprise.ActionValueType
WHERE Value = 'Right';

SELECT @RoleCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'Default';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'Default';

SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
     JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
     JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
     JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'All'
      AND StatusTypeCategoryType.Name = 'Security';



IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Vendor Marketplace' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Vendor Marketplace', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = 'Access to Vendor Marketplace', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'DashBoard' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Vendor Marketplace' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Vendor Marketplace', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;





SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
WHERE O.PartyId = 3;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = 'User Administrator' AND 
		  R.PartyId = @PartyId;
	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #RightsUnifiedSettings;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_VendorMarketplace', @ShortName = 'AccessVendorMarketplace', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = 'Access to Vendor Marketplace', @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @RightId = @RightId OUTPUT;
		
		
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Vendor Marketplace' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_VendorMarketplace';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Vendor Marketplace' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		
		

		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

DECLARE @Dashboard INT

SELECT  @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType 
WHERE value = 'Default_VendorMarketplace';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Access to Vendor Marketplace');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @DashBoard
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @DashBoard );
	
END;

GO

--Secoodary EMail

IF NOT EXISTS (SELECT 1 FROM [Enterprise].[ContactMechanismUsageType] WHERE ContactMechanismUsageTypeID = 302)
BEGIN
	INSERT INTO [Enterprise].[ContactMechanismUsageType] ([ContactMechanismUsageTypeID]
      ,[ParentContactMechanismUsageTypeID]
      ,[Name])
	VALUES (302, 300, 'Secondary Email')
END	
GO

--Audit trail related items

DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;
DECLARE @TRightShortName NVARCHAR(100)
IF OBJECT_ID('tempdb..#RightsUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #RightsUnifiedSettings;
END;

IF OBJECT_ID('tempdb..#HoldPartyForUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyForUnifiedSettings;
END;

CREATE TABLE #RightsUnifiedSettings
( 
			 RightId int, Name nvarchar(500), description nvarchar(500), shortname varchar(100)
);

INSERT INTO #RightsUnifiedSettings( rightid, name, description, shortname )
VALUES( 1, 'Ability to view audit trail on user data', 'Ability to view audit trail on user data', 'ViewAuditTrailUserData' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

SELECT @ActionValueID = [ActionValueTypeID]
FROM Enterprise.ActionValueType
WHERE Value = 'Right';

SELECT @RoleCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'Default';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'Default';

SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
     JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
     JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
     JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'All'
      AND StatusTypeCategoryType.Name = 'Security';



IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Audit Trail' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'View Audit Trail', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = 'Ability to view audit trail on user data', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'EditUser' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'View Audit Trail' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'View Audit Trail', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;





SELECT 
	DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
--WHERE O.Name IN ('RealPage' , 'RealPage Employee', 'Fairfield Residential Company LLC', 'Summit Management LLC');

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = 'User Administrator' AND 
		  R.PartyId = @PartyId;
	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #RightsUnifiedSettings;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ViewAuditTrailData', @ShortName = 'ViewAuditTrailUserData', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = 'Ability to view audit trail on user data', @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @RightId = @RightId OUTPUT;
		
		
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'View Audit Trail' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_ViewAuditTrailData';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'View Audit Trail' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		
		

		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

DECLARE @AuditTrail INT


SELECT  @AuditTrail = RightValueTypeId
FROM Enterprise.RightValueType 
WHERE value = 'Default_ViewAuditTrailData';



SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to edit users');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @AuditTrail
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @AuditTrail );
		
END;

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to view audit trail on user data');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @AuditTrail
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @AuditTrail );
		
END;
GO
DECLARE @AuditTrail INT
DECLARE @RightValueTypeId INT

SELECT  @AuditTrail = RightValueTypeId
FROM Enterprise.RightValueType 
WHERE value = 'Default_EditUser_Route';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to view audit trail on user data');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @AuditTrail
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @AuditTrail );
		
END;
GO
DECLARE @RtDep INT
DECLARE @DepStDep INT
SELECT @RtDep = RightValueTypeId FROM Enterprise.RIghtValueTYpe 
	WHERE Value = 'Ability to edit users'
SELECT @DepStDep = RightValueTypeId FROM Enterprise.RIghtValueTYpe 
	WHERE Value = 'Default_ViewAuditTrailData'
DELETE FROM Enterprise.RightDependency WHERE RIghtValueTYpeId = @RtDep AND DependentRightValueTypeId = @DepStDep

GO
update enterprise.action set objectvalue = 'View Audit Trail User Data'
	WHERE objectvalue = 'View Audit Trail'
	GO

-- GB-1821 - Add ProductType record for Property Photos and Vendor Marketplace

IF (SELECT 1 FROM Enterprise.Product P
	INNER JOIN Enterprise.ProductType PT
		ON P.Name = PT.Name
	WHERE P.Name = 'Property Photos'
		AND P.ProductTypeId = PT.ProductTypeId) IS NULL
BEGIN
	IF (SELECT 1 FROM Enterprise.ProductType
		WHERE Name = 'Property Photos') IS NULL
	BEGIN
	DECLARE @PPGUID UNIQUEIDENTIFIER = NEWID()
	EXEC Enterprise.CreateProductType 108, 100, 'Property Photos', 'Property Photos', @PPGUID
	END

	IF (SELECT 1 FROM Enterprise.Product
		WHERE Name = 'Property Photos'
			AND ProductTypeId = 108) IS NULL
	BEGIN
	UPDATE Enterprise.Product
	SET ProductTypeId = 108
	WHERE Name = 'Property Photos'
	END
END

IF (SELECT 1 FROM Enterprise.Product P
	INNER JOIN Enterprise.ProductType PT
		ON P.Name = PT.Name
	WHERE P.Name = 'Vendor Marketplace'
		AND P.ProductTypeId = PT.ProductTypeId) IS NULL
BEGIN
	IF (SELECT 1 FROM Enterprise.ProductType
		WHERE Name = 'Vendor Marketplace') IS NULL
	BEGIN
	DECLARE @VMGUID UNIQUEIDENTIFIER = NEWID()
	EXEC Enterprise.CreateProductType 109, 100, 'Vendor Marketplace', 'Vendor Marketplace', @VMGUID
	END

	IF (SELECT 1 FROM Enterprise.Product
		WHERE Name = 'Vendor Marketplace'
			AND ProductTypeId = 109) IS NULL
	BEGIN
	UPDATE Enterprise.Product
	SET ProductTypeId = 109
	WHERE Name = 'Vendor Marketplace'
	END
END

GO

--GB-1918 - Update administrator email to "a limited amount of time" wording

UPDATE Enterprise.CommunicationEmailTemplate 
SET Body = REPLACE(Body, '72 hours', 'a limited amount of time')
WHERE CommunicationEventAudienceTypeId = 1
AND CommunicationEventPurposeTypeId = 1

GO


--Unified Settings Activity Logging CLeint

DECLARE @ClientId int= NULL;

SELECT @ClientId = ClientId
FROM Auth.Clients
WHERE ClientCode = 'unified-settings-activity-logger' AND 
	  Flow = 3;

IF @ClientId IS NULL
BEGIN
	INSERT INTO Auth.Clients( ClientCode, ClientName, Flow, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes )
	VALUES( 'unified-settings-activity-logger', 'Unified Settings Activity Logging Client', 3, 360, 3600, 360, 86400, 3600, 0, 0, 0, 'True', 'True', 'False', 'False', 'True', 'True', 'False', 'True', 'True', 'True', 'True', 'True', 'True' );
	SELECT @ClientId = SCOPE_IDENTITY();
	INSERT INTO Auth.ClientScopes( ClientId, Scope )
	VALUES( @ClientId, 'rplandingapi' );
	INSERT INTO Auth.ClientScopes( ClientId, Scope )
	VALUES( @ClientId, 'activityreader' );
	INSERT INTO Auth.ClientSecrets( ClientId, Value, Description, Expiration )
	VALUES( @ClientId, 'HcyuV+Dbp3FBvXOSFSjDwR7jAmWZPQ+dH2xVvjPUgbI=', 'Unified Settings Activity Logging Client', '2099-12-31 00:00:00.0000000 -06:00' );
END;
--Post RIght Fix
/*****************************
Roles and Right 
(Thats going to production)
*****************************/

DECLARE @VisibilityStatus INT  
DECLARE @RightTypeId INT         
UPDATE RVT              
              SET RVT.ProductId =  A.ProductId,
				  RVT.VisibilityStatusId = UA.Status 
        FROM Enterprise.RightValueType RVT
            INNER JOIN  Enterprise.[Right] R ON R.RightValueTypeId = RVT.RightValueTypeId
       INNER JOIN Enterprise.UserActions UA ON UA.RightId = R.RightId
            INNER JOIN Enterprise.ACTION A ON A.ActionId = UA.ActionId
			INNER JOIN Enterprise.StatusType ST on ST.StatusTypeId = UA.Status
		WHERE  UA.status not in (13, 14)
			
SET @VisibilityStatus = NULL
SELECT @VisibilityStatus = StatusType.StatusTypeID
         FROM Enterprise.StatusTypeCategoryType
              JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
              JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
              JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
         WHERE StatusType.name = 'ALL'
               AND StatusTypeCategoryType.Name = 'Security';

UPDATE Enterprise.RightValueType
SET ProductId = 3,
	VisibilityStatusId = @VisibilityStatus
WHERE  Value IN 
	(
	'Ability to Activate/Deactivate User'
	,'Ability to clone users'
	,'Ability to create and manage users for Resident Portals'
	,'Ability to create users'
	,'Ability to edit my own profile'
	,'Ability to edit password'
	,'Ability to edit profile of other users'
	,'Ability to edit users'
	,'Ability to lock/unlock users'
	,'Ability to Migrate Users'
	,'Ability to Resend Invite'
	,'Ability to view audit trail on user data'
	,'Ability to view users'
	,'Access to Product Learning Portal'
	,'Ability to manage roles and rights'
	,'Ability to view roles and rights'
	,'Access to Vendor Marketplace'
	,'Access to Property Photos'
	)

SET @VisibilityStatus = NULL
SELECT  @VisibilityStatus = StatusType.StatusTypeID
        FROM Enterprise.StatusTypeCategoryType
            JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
            JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
            JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
        WHERE StatusType.name = 'Internal Only'
            AND StatusTypeCategoryType.Name = 'Security';

UPDATE Enterprise.RightValueType
SET ProductId = 3,
	VisibilityStatusId = @VisibilityStatus
WHERE  Value IN 
	(
	'Access to Unified Platform via Support Tool'
	,'Access to Unified Settings via Support Tool'
	,'Ability to Configure Custom Fields for Users'
	,'View Unified Settings'
	,'Ability to Manage Settings for Unified Platform'
	,'Ability to manage Unified Settings'
	)



SELECT @RightTypeId =  StatusType.StatusTypeID
         FROM Enterprise.StatusTypeCategoryType
              JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
              JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
              JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
         WHERE StatusType.name = 'Default'
				AND Enterprise.StatusTypeCategory.Name = 'Role Type'
               AND StatusTypeCategoryType.Name = 'Security';
UPDATE Enterprise.RightValueType
	SET StatusTypeId = @RightTypeId 
WHERE StatusTypeId IS NULL


