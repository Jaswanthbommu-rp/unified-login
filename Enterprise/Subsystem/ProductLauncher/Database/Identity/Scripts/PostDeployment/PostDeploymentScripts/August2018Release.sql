GO
IF EXISTS (SELECT 1 FROM Enterprise.Product WHERE Name = 'OneSite COnversions')
BEGIN
	UPDATE ENterprise.Product SET Name = 'Leasing & Rents Conversion Tool'
		WHERE Name = 'OneSite Conversions'
END
GO
--Setup Resource "COnversion Tools"

--Setup Resource "COnversion Tools"

DECLARE @ProductId INT;
DECLARE @ProductSettingId INT;
DECLARE @ProductSettingTypeId INT;
DECLARE @FromDate DATETIME;
DECLARE @ConfigurationId INT;
DECLARE @PartyId INT;
SET @ProductSettingId = NULL;
SET @ProductSettingTypeId = NULL;
SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Leasing & Rents Conversion Tool';
SELECT @ConfigurationId = ConfigurationId
FROM Enterprise.GlobalProductConfiguration
WHERE ProductId = @ProductId
      AND (GETDATE() BETWEEN FromDate AND ThruDate
           OR ThruDate IS NULL);
EXEC Enterprise.GetProductSettingType
     @Name = 'IsResource', -- varchar(50)
     @ProductSettingTypeId = @ProductSettingTypeId OUTPUT; -- int

	-- Create the Value and assign it to the Product and ProductSettingType
EXEC Enterprise.CreateProductSetting
     @ProductId = @ProductId, -- int
     @ProductSettingTypeId = @ProductSettingTypeId, -- int
     @Value = N'1', -- nvarchar(1000)
     @FromDate = @FromDate, -- datetime
     @ThruDate = NULL, -- datetime
     @ProductSettingId = @ProductSettingId OUTPUT; -- int

	-- Link the Product Setting to an actual configuration
EXEC Enterprise.LinkProductSettingToConfiguration
     @ConfigurationId = @ConfigurationId, -- int
     @ProductSettingId = @ProductSettingId, -- int
     @FromDate = @FromDate, -- datetime
     @ThruDate = NULL;   -- datetime



DECLARE ListParty CURSOR
FOR SELECT O.PartyId
    FROM Enterprise.Organization O
         INNER JOIN Enterprise.[OrganizationProduct] OP ON O.PartyId = OP.PartyId
         INNER JOIN Enterprise.Product P ON OP.ProductId = P.ProductId
    WHERE P.Name = 'OneSite' AND O.PartyId = 3;
OPEN ListParty;
FETCH ListParty INTO @PartyId;
WHILE @@FETCH_STATUS = 0
    BEGIN
        IF NOT EXISTS
		(
			SELECT 1
			FROM Enterprise.OrganizationProduct
			WHERE PartyId = @PartyId
				  AND COnfigurationId = @COnfigurationId
				  AND ProductId = @ProductId
		)
            BEGIN
                INSERT INTO Enterprise.OrganizationProduct
				(PartyId,
				 ConfigurationId,
				 ProductId,
				 FromDate
				)
                VALUES
				(@PartyId,
				 @ConfigurationId,
				 @ProductId,
				 GETUTCDATE()
				);
            END;
		FETCH ListParty INTO @PartyId;
    END;
CLOSE ListParty;
DEALLOCATE ListParty;

GO
--Add right

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
VALUES( 1, 'Access to Leasing & Rents Conversion Tool for OneSite users', 'Access to Leasing & Rents Conversion Tool for OneSite users', 'AccessOneSiteConversions' );

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
WHERE StatusType.name = 'ALL'
      AND StatusTypeCategoryType.Name = 'Security';



IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'OneSite Conversions' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'OneSite Conversions', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = 'Access to Leasing & Rents Conversion Tool for OneSite users', @ActionID = @ActionID OUTPUT;
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
	WHERE ObjectValue = 'OneSite Conversions' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'OneSite Conversions', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;





SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId


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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_OneSiteConversions', @ShortName = 'AccessOneSiteConversions', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = 'Access to Vendor Marketplace', @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @RightId = @RightId OUTPUT;
		
		
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'OneSite Conversions' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_OneSiteConversions';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'OneSite Conversions' AND 
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
WHERE value = 'Default_OneSiteConversions';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Access to Leasing & Rents Conversion Tool for OneSite users');

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
--SET DEFAULT TO SYSTEM
UPDATE ST
SET
	ST.Name = 'System'
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

--ROLE TYPE DEPENDENCIES

/*
IF EXISTS
(
	SELECT 1
	FROM Enterprise.RoleTypeDependency
)
BEGIN
	DELETE FROM Enterprise.RoleTypeDependency;
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 403, 401, 20 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 403, 402, 30 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 403, 403, 40 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 403, 404, 10 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 403, 405, 50 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 402, 401, 20 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 402, 402, 30 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 402, 404, 10 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 402, 405, 40 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 402, 405, 50 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 402, 403, 40 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 401, 401, 20 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 401, 404, 10 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 401, 405, 30 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 404, 401, 20 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 404, 404, 10 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 404, 405, 30 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 405, 401, 20 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 405, 404, 10 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 405, 405, 30 );
END
ELSE
BEGIN
INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 403, 401, 20 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 403, 402, 30 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 403, 403, 40 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 403, 404, 10 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 403, 405, 50 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 402, 401, 20 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 402, 402, 30 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 402, 405, 50 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 402, 404, 10 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 402, 405, 40 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 402, 403, 40 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 401, 401, 20 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 401, 404, 10 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 401, 405, 30 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 404, 401, 20 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 404, 404, 10 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 404, 405, 30 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 405, 401, 20 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 405, 404, 10 );
	INSERT INTO Enterprise.RoleTypeDependency( ParentRoleTypeId, ChildRoleTypeId, SortOrder )
	VALUES( 405, 405, 30 );
END
GO
*/

-- http://jira.realpage.com/browse/GB-1938
IF EXISTS (SELECT 1 FROM Enterprise.RightValueTYpe WHERE Value = 'Ability to edit password')
BEGIN
	UPDATE Enterprise.RightValueType SET VALUE = 'Ability to insert a temporary password for another user'
	WHERE Value = 'Ability to edit password'
END

GO
DECLARE @ClientId int = NULL 
SELECT @ClientId = ClientId FROM Auth.Clients WHERE ClientCode = 'settings-management' 
IF @ClientId IS NOT NULL
BEGIN 

INSERT INTO Auth.ClientScopes(ClientId, Scope) VALUES (@ClientId, 'rplandingapi')

END;

GO
--Edit Password Route


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
VALUES( 1, 'Ability to insert a temporary password for another user', 'Ability to insert a temporary password for another user', 'EditPassword' );

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
WHERE StatusType.name = 'ALL'
      AND StatusTypeCategoryType.Name = 'Security';



IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Edit Password' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Edit Password', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = 'Ability to insert a temporary password for another user', @ActionID = @ActionID OUTPUT;
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
	WHERE ObjectValue = 'Edit Password' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Edit Password', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;





SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
WHERE O.PartyId = 350;

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
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_EditPassword', @ShortName = 'EditPassword', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = 'Ability to insert a temporary password for another user', @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @RightId = @RightId OUTPUT;
		
		
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Edit Password' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_EditPassword';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Edit Password' AND 
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
WHERE value = 'Default_EditPassword';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to insert a temporary password for another user');

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
		INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @DashBoard, @RightValueTypeId );
END;


GO
UPDATE Enterprise.Product
SET Description = 'The OneSite environment provides access to L&R, Budgeting, Payments, Screening, Facilities, Purchasing, and Doc Management for your properties, depending upon the mix of products which are licensed.' 
WHERE Name = 'OneSite'

UPDATE PS
SET Value = 'OneSite L&R, Budgeting, Payments, Screening, Facilities, Purchasing, Doc. Mgmt'
FROM Enterprise.ProductSetting PS
INNER JOIN Enterprise.Product P ON PS.ProductId = P.ProductId AND P.Name = 'OneSite'
INNER JOIN Enterprise.ProductSettingType PST ON PS.ProductSettingTypeId = PST.ProductSettingTypeId AND PST.Name = 'Subsolution'

UPDATE PS
SET Value = 'L&R, Budgeting, Payments, Screening, Facilities, Purchasing, Doc. Mgmt'
FROM Enterprise.ProductSetting PS
INNER JOIN Enterprise.Product P ON PS.ProductId = P.ProductId AND P.Name = 'OneSite'
INNER JOIN Enterprise.ProductSettingType PST ON PS.ProductSettingTypeId = PST.ProductSettingTypeId AND PST.Name = 'SubDescription'

GO



DECLARE @userrightsid INT
DECLARE @statustypecategoryid INT
DECLARE @ident INT
DECLARE @statusTypeId INT

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.StatusTypeCategoryType
    WHERE Name = 'Unified Platform'
)
    BEGIN
        INSERT INTO Enterprise.StatusTypeCategoryType
        (ParentStatusTypeCategoryTypeId,
         Name
        )
        VALUES
        (NULL,
         'Unified Platform'
        );
END;
SELECT @userrightsid = StatusTypeCategoryTypeid
FROM Enterprise.StatusTypeCategoryType
WHERE name = 'Unified Platform';
IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.StatusTypeCategory
    WHERE name = 'Create User Source'
)
    BEGIN
        INSERT INTO Enterprise.StatusTypeCategory
        (ParentStatusTypeCategoryId,
         StatusTypeCategoryTypeId,
         Name
        )
        VALUES
        (NULL, -- ParentStatusTypeCategoryId - int
         @userrightsid, -- StatusTypeCategoryTypeId - int
         'Create User Source' -- Name - varchar(50)
        );
END;
SELECT @statustypecategoryid = StatusTypeCategoryId
FROM Enterprise.StatusTypeCategory
WHERE name = 'Create User Source';

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.StatusType
    WHERE name = 'Unity-GB'
)
    BEGIN
        INSERT INTO Enterprise.StatusType(name)
    VALUES('Unity-GB');
        SELECT @ident = @@IDENTITY;
        INSERT INTO Enterprise.StatusTypeCategoryClassification
        (StatusTypeId,
         StatusTypeCategoryId,
         FromDate,
         ThruDate
        )
        VALUES
        (@ident, -- StatusTypeId - int
         @statustypecategoryid, -- StatusTypeCategoryId - int
         GETDATE(), -- FromDate - datetime
         NULL  -- ThruDate - datetime
        );
END;
    ELSE
    BEGIN
        SELECT @statusTypeId = Enterprise.StatusType.StatusTypeId
        FROM Enterprise.StatusType
             INNER JOIN Enterprise.StatusTypeCategoryClassification ON Enterprise.StatusType.StatusTypeId = Enterprise.StatusTypeCategoryClassification.StatusTypeId
             INNER JOIN Enterprise.StatusTypeCategory ON Enterprise.StatusTypeCategoryClassification.StatusTypeCategoryId = Enterprise.StatusTypeCategory.StatusTypeCategoryId
             INNER JOIN Enterprise.StatusTypeCategoryType ON Enterprise.StatusTypeCategory.StatusTypeCategoryTypeId = Enterprise.StatusTypeCategoryType.StatusTypeCategoryTypeId
        WHERE Enterprise.StatusTypeCategoryType.name = 'Unified Platform'
              AND Enterprise.StatusType.Name = 'Unity-GB';
        INSERT INTO Enterprise.StatusTypeCategoryClassification
        (StatusTypeId,
         StatusTypeCategoryId,
         FromDate,
         ThruDate
        )
        VALUES
        (@StatusTypeId, -- StatusTypeId - int
         @statustypecategoryid, -- StatusTypeCategoryId - int
         GETDATE(), -- FromDate - datetime
         NULL  -- ThruDate - datetime
        );
END;



IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.StatusType
    WHERE name = 'Unity-ExcelImport'
)
    BEGIN
        INSERT INTO Enterprise.StatusType(name)
    VALUES('Unity-ExcelImport');
        SELECT @ident = @@IDENTITY;
        INSERT INTO Enterprise.StatusTypeCategoryClassification
        (StatusTypeId,
         StatusTypeCategoryId,
         FromDate,
         ThruDate
        )
        VALUES
        (@ident, -- StatusTypeId - int
         @statustypecategoryid, -- StatusTypeCategoryId - int
         GETDATE(), -- FromDate - datetime
         NULL  -- ThruDate - datetime
        );
END;
    ELSE
    BEGIN
        SELECT @statusTypeId = Enterprise.StatusType.StatusTypeId
        FROM Enterprise.StatusType
             INNER JOIN Enterprise.StatusTypeCategoryClassification ON Enterprise.StatusType.StatusTypeId = Enterprise.StatusTypeCategoryClassification.StatusTypeId
             INNER JOIN Enterprise.StatusTypeCategory ON Enterprise.StatusTypeCategoryClassification.StatusTypeCategoryId = Enterprise.StatusTypeCategory.StatusTypeCategoryId
             INNER JOIN Enterprise.StatusTypeCategoryType ON Enterprise.StatusTypeCategory.StatusTypeCategoryTypeId = Enterprise.StatusTypeCategoryType.StatusTypeCategoryTypeId
        WHERE Enterprise.StatusTypeCategoryType.name = 'Unified Platform'
              AND Enterprise.StatusType.Name = 'Unity-ExcelImport';
        INSERT INTO Enterprise.StatusTypeCategoryClassification
        (StatusTypeId,
         StatusTypeCategoryId,
         FromDate,
         ThruDate
        )
        VALUES
        (@StatusTypeId, -- StatusTypeId - int
         @statustypecategoryid, -- StatusTypeCategoryId - int
         GETDATE(), -- FromDate - datetime
         NULL  -- ThruDate - datetime
        );
END;




IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.StatusType
    WHERE name = 'Unity-MigrationTool'
)
    BEGIN
        INSERT INTO Enterprise.StatusType(name)
    VALUES('Unity-MigrationTool');
        SELECT @ident = @@IDENTITY;
        INSERT INTO Enterprise.StatusTypeCategoryClassification
        (StatusTypeId,
         StatusTypeCategoryId,
         FromDate,
         ThruDate
        )
        VALUES
        (@ident, -- StatusTypeId - int
         @statustypecategoryid, -- StatusTypeCategoryId - int
         GETDATE(), -- FromDate - datetime
         NULL  -- ThruDate - datetime
        );
END;
    ELSE
    BEGIN
        SELECT @statusTypeId = Enterprise.StatusType.StatusTypeId
        FROM Enterprise.StatusType
             INNER JOIN Enterprise.StatusTypeCategoryClassification ON Enterprise.StatusType.StatusTypeId = Enterprise.StatusTypeCategoryClassification.StatusTypeId
             INNER JOIN Enterprise.StatusTypeCategory ON Enterprise.StatusTypeCategoryClassification.StatusTypeCategoryId = Enterprise.StatusTypeCategory.StatusTypeCategoryId
             INNER JOIN Enterprise.StatusTypeCategoryType ON Enterprise.StatusTypeCategory.StatusTypeCategoryTypeId = Enterprise.StatusTypeCategoryType.StatusTypeCategoryTypeId
        WHERE Enterprise.StatusTypeCategoryType.name = 'Unified Platform'
              AND Enterprise.StatusType.Name = 'Unity-MigrationTool';
        INSERT INTO Enterprise.StatusTypeCategoryClassification
        (StatusTypeId,
         StatusTypeCategoryId,
         FromDate,
         ThruDate
        )
        VALUES
        (@StatusTypeId, -- StatusTypeId - int
         @statustypecategoryid, -- StatusTypeCategoryId - int
         GETDATE(), -- FromDate - datetime
         NULL  -- ThruDate - datetime
        );
END;

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.StatusType
    WHERE name = 'RPX'
)
    BEGIN
        INSERT INTO Enterprise.StatusType(name)
    VALUES('RPX');
        SELECT @ident = @@IDENTITY;
        INSERT INTO Enterprise.StatusTypeCategoryClassification
        (StatusTypeId,
         StatusTypeCategoryId,
         FromDate,
         ThruDate
        )
        VALUES
        (@ident, -- StatusTypeId - int
         @statustypecategoryid, -- StatusTypeCategoryId - int
         GETDATE(), -- FromDate - datetime
         NULL  -- ThruDate - datetime
        );
END;
    ELSE
    BEGIN
        SELECT @statusTypeId = Enterprise.StatusType.StatusTypeId
        FROM Enterprise.StatusType
             INNER JOIN Enterprise.StatusTypeCategoryClassification ON Enterprise.StatusType.StatusTypeId = Enterprise.StatusTypeCategoryClassification.StatusTypeId
             INNER JOIN Enterprise.StatusTypeCategory ON Enterprise.StatusTypeCategoryClassification.StatusTypeCategoryId = Enterprise.StatusTypeCategory.StatusTypeCategoryId
             INNER JOIN Enterprise.StatusTypeCategoryType ON Enterprise.StatusTypeCategory.StatusTypeCategoryTypeId = Enterprise.StatusTypeCategoryType.StatusTypeCategoryTypeId
        WHERE Enterprise.StatusTypeCategoryType.name = 'Unified Platform'
              AND Enterprise.StatusType.Name = 'RPX';
        INSERT INTO Enterprise.StatusTypeCategoryClassification
        (StatusTypeId,
         StatusTypeCategoryId,
         FromDate,
         ThruDate
        )
        VALUES
        (@StatusTypeId, -- StatusTypeId - int
         @statustypecategoryid, -- StatusTypeCategoryId - int
         GETDATE(), -- FromDate - datetime
         NULL  -- ThruDate - datetime
        );
END;
GO
GO
DECLARE @ProductName nvarchar(100) = 'Intelligent Lead Management',
	@ProductId int,
	@FromDate datetime,
	@ProductSettingId int,
	@ProductSettingTypeId int,
	@ConfigurationId int,
	@ClientName NVARCHAR(500),
    @ClientCode NVARCHAR(500),
    @TitleUniqueId UNIQUEIDENTIFIER,
    @ParentProductTypeId INT,
    @SamlProductSettingId INT,
    @ClientId INT;

SET @ProductId = 40
SET @FromDate = GETUTCDATE()
SET @ClientName = 'Intelligent Lead Management';
SET @ClientCode = 'IntelligentLeadManagement';
SET @ClientId = NULL;

DECLARE @AuthClients TABLE
( 
	ClientId int
);



IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductTYpe WHERE Name = 'Intelligent Lead Management')
BEGIN

INSERT INTO [Enterprise].[ProductType]
           ([ProductTypeId]
           ,[ParentProductTypeId]
           ,[Name]
           ,[Description]
           ,[ProductTypeGuid])
     VALUES
           (308
           ,300
           ,'Intelligent Lead Management'
           ,'Intelligent Lead Management'
           ,'B7E1D8CA-ADB4-4DB6-8804-2A5CD9323FA4')
END


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
					N'29D12BD2-DBDA-41C0-8A60-2364B7FDF46E', --ProductGUID
					@ProductName, --Name
					@ProductName, --Description
					308 AS ProductTypeID,
					'ILMLM' -- find out from Business for book code
	
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
		@Value = N'42C5F72C-BAD0-4075-9D17-6AE3AFA50C91',                               -- nvarchar(1000)
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
		@Value = N'/product/intelligentleadmanagement', 
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
	
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime
	------------------------------------------------------------------------------------------
	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ShowInUserDetails ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ShowInUserDetails',                                         -- varchar(50)
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
	
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime
	------------------------------------------------------------------------------------------
	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ShowInUserDetails ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'RequiresUnifiedLoginRight',                                         -- varchar(50)
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
	
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime
------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ShowInUserDetails ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ShowInRolesAndRights',                                         -- varchar(50)
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
	
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ShowInAppSwitcher ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ShowInAppSwitcher',                                         -- varchar(50)
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
	
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime
------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ShowInUserListFilter ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ShowInUserListFilter',                                         -- varchar(50)
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
	
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime
------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductAPIRequiresUser ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ProductAPIRequiresUser',                                         -- varchar(50)
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
	
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
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
		@Value = N'intelligentleadmanagement',                               -- nvarchar(1000)
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
		@Value = N'intelligentleadmanagement',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime
------------------------------------------------------------------------------------
	EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId
END

IF NOT EXISTS
(
	SELECT TOP 1 1
	FROM Auth.Scopes
	WHERE Name = @ClientCode
)
BEGIN
	INSERT INTO Auth.Scopes( Name, DisplayName, Description, ClaimsRule, Enabled, Required, Emphasize, Type, IncludeAllClaimsForUser, ShowInDiscoveryDocument, AllowUnrestrictedIntrospection )
	VALUES( @ClientCode, --Name,
	@ClientName, --DisplayName,
	NULL, --Description,
	NULL, --ClaimsRule,
	1, --Enabled,
	0, --Required,
	0, --Emphasize,
	1, --Type,
	1, --IncludeAllClaimsForUser,
	1, --ShowInDiscoveryDocument,
	0 --AllowUnrestrictedIntrospection
	);
END;

--ClientCredentials Flow

IF NOT EXISTS
(
	SELECT TOP 1 1
	FROM Auth.Clients
	WHERE ClientCode = @ClientCode AND 
		  Flow = 1
)
BEGIN
	INSERT INTO Auth.Clients( ClientCode, ClientName, ClientUri, LogoUri, Flow, LogoutUri, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes )
	OUTPUT INSERTED.ClientId
		   INTO @AuthClients(ClientId)
	VALUES( @ClientCode, --ClientCode
	@ClientName, --ClientName
	NULL, --ClientUri
	NULL, --LogoUri
	1, --ClientCredentials
	NULL, --LogoutUri
	36000, --IdentityTokenLifetime
	36000, --AccessTokenLifetime
	36000, --AuthorizationCodeLifetime
	86400, --AbsoluteRefreshTokenLifetime
	36000, --SlidingRefreshTokenLifetime
	0, --RefreshTokenUsage
	0, --RefreshTokenExpiration
	0, --AccessTokenType
	1, --UpdateAccessTokenOnRefresh
	1, --Enabled
	0, --LogoutSessionRequired
	0, --RequireSignOutPrompt
	1, --AllowAccessToAllScopes
	0, --AllowClientCredentialsOnly
	0, --RequireConsent
	1, --AllowRememberConsent
	1, --EnableLocalLogin
	1, --IncludeJwtId
	1, --AlwaysSendClientClaims
	1, --PrefixClientClaims
	1 --AllowAccessToAllGrantTypes
	);
	SELECT @ClientId = ClientId
	FROM @AuthClients;
	INSERT INTO Auth.ClientScopes( ClientId, Scope )
		   SELECT @ClientId, --ClientId
		   ScopeId --Scope
		   FROM Auth.Scopes
		   WHERE Name = @ClientCode;
	INSERT INTO Auth.ClientSecrets( ClientId, Value, Type, Description, Expiration )
	VALUES( @ClientId, 'RqRuejakSbnQH1D4qMvkF6fYONxuwfWm1t6GWLq/XNg=', NULL, @ClientName, '2099-12-31 00:00:00.0000000 -06:00' );

	Insert into Auth.ClientRedirectUris(ClientRedirectUriId,Uri)
	Values(@ClientId,N'http://54.189.37.40/sjilm')

END;
--[Guid("CA6641DC-977C-42D3-A101-5E29931CD8A1")]


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
		N'http://54.189.37.40/sjilm', --LoginUri
		N'NA', --SigningCertificateThumbprint
		N'productUsername' --SubjectIdSamlAttribute
	)
END

  --insert into [IdentityDevelopment].[Enterprise].[OrganizationProduct] (PartyId,ConfigurationId,ProductId,FromDate,ThruDate)
  --Select 3,1065,39,getdate(),null



  GO
  GO
GO
DECLARE @ProductName nvarchar(100) = 'Intelligent Lead Management-Leasing Analytics',
	@ProductId int,
	@FromDate datetime,
	@ProductSettingId int,
	@ProductSettingTypeId int,
	@ConfigurationId int,
	@ClientName NVARCHAR(500),
    @ClientCode NVARCHAR(500),
    @TitleUniqueId UNIQUEIDENTIFIER,
    @ParentProductTypeId INT,
    @SamlProductSettingId INT,
    @ClientId INT;

SET @ProductId = 41
SET @FromDate = GETUTCDATE()
SET @ClientName = 'Intelligent Lead Management-Leasing Analytics';
SET @ClientCode = 'intelligentleadmanagementleasinganalytics';
SET @ClientId = NULL;

DECLARE @AuthClients TABLE
( 
	ClientId int
);

IF NOT EXISTS (SELECT 1 FROM Enterprise.ProductTYpe WHERE Name = 'Intelligent Lead Management-Leasing Analytics')
BEGIN

INSERT INTO [Enterprise].[ProductType]
           ([ProductTypeId]
           ,[ParentProductTypeId]
           ,[Name]
           ,[Description]
           ,[ProductTypeGuid])
     VALUES
           (309
           ,300
           ,'Intelligent Lead Management-Leasing Analytics'
           ,'Intelligent Lead Management-Leasing Analytics'
           ,'96455F0B-B9C5-49ED-ABBF-BFE4D7B7D7F4')
END


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
	SELECT	41, --ProductId
					N'9427E995-5712-4978-AB2E-99AD547B6DBA', --ProductGUID
					@ProductName, --Name
					@ProductName, --Description
					309 AS ProductTypeID,
					'ILMLA' -- find out from Business for book code
	
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
		@Value = N'4FD51401-4F7A-4BA2-88AA-15B6706BFCEF',                               -- nvarchar(1000)
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
		@Value = N'/product/intelligentleadmanagementleasinganalytics', 
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
	
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime
	------------------------------------------------------------------------------------------
	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ShowInUserDetails ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ShowInUserDetails',                                         -- varchar(50)
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
	
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime
	------------------------------------------------------------------------------------------
	------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ShowInUserDetails ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'RequiresUnifiedLoginRight',                                         -- varchar(50)
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
	
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime
------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ShowInUserDetails ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ShowInRolesAndRights',                                         -- varchar(50)
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
	
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime

------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ShowInAppSwitcher ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ShowInAppSwitcher',                                         -- varchar(50)
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
	
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime
------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ShowInUserListFilter ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ShowInUserListFilter',                                         -- varchar(50)
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
	
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime
------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get the ProductAPIRequiresUser ProductSettingTypeId for the attribute you need to select, the API should have all these cached though.
	SET @ProductSettingId = NULL
	SET @ProductSettingTypeId = NULL

	EXEC Enterprise.GetProductSettingType
		@Name = 'ProductAPIRequiresUser',                                         -- varchar(50)
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
	
	EXEC Enterprise.LinkGlobalConfigurationToProduct
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductId = @ProductId,                    -- int
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
		@Value = N'intelligentleadmanagement',                               -- nvarchar(1000)
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
		@Value = N'intelligentleadmanagement',                               -- nvarchar(1000)
		@FromDate = @FromDate,          -- datetime
		@ThruDate = NULL,          -- datetime
		@ProductSettingId = @ProductSettingId OUTPUT -- int

	-- Link the Product Setting to an actual configuration
	EXEC Enterprise.LinkProductSettingToConfiguration
		@ConfigurationId = @ConfigurationId,              -- int
		@ProductSettingId = @ProductSettingId,             -- int
		@FromDate = @FromDate, -- datetime
		@ThruDate = NULL   -- datetime
------------------------------------------------------------------------------------
	EXEC Enterprise.ListGlobalSettingsForProduct @productid = @ProductId
END

IF NOT EXISTS
(
	SELECT TOP 1 1
	FROM Auth.Scopes
	WHERE Name = @ClientCode
)
BEGIN
	INSERT INTO Auth.Scopes( Name, DisplayName, Description, ClaimsRule, Enabled, Required, Emphasize, Type, IncludeAllClaimsForUser, ShowInDiscoveryDocument, AllowUnrestrictedIntrospection )
	VALUES( @ClientCode, --Name,
	@ClientName, --DisplayName,
	NULL, --Description,
	NULL, --ClaimsRule,
	1, --Enabled,
	0, --Required,
	0, --Emphasize,
	1, --Type,
	1, --IncludeAllClaimsForUser,
	1, --ShowInDiscoveryDocument,
	0 --AllowUnrestrictedIntrospection
	);
END;

--ClientCredentials Flow

IF NOT EXISTS
(
	SELECT TOP 1 1
	FROM Auth.Clients
	WHERE ClientCode = @ClientCode AND 
		  Flow = 1
)
BEGIN
	INSERT INTO Auth.Clients( ClientCode, ClientName, ClientUri, LogoUri, Flow, LogoutUri, IdentityTokenLifetime, AccessTokenLifetime, AuthorizationCodeLifetime, AbsoluteRefreshTokenLifetime, SlidingRefreshTokenLifetime, RefreshTokenUsage, RefreshTokenExpiration, AccessTokenType, UpdateAccessTokenOnRefresh, Enabled, LogoutSessionRequired, RequireSignOutPrompt, AllowAccessToAllScopes, AllowClientCredentialsOnly, RequireConsent, AllowRememberConsent, EnableLocalLogin, IncludeJwtId, AlwaysSendClientClaims, PrefixClientClaims, AllowAccessToAllGrantTypes )
	OUTPUT INSERTED.ClientId
		   INTO @AuthClients(ClientId)
	VALUES( @ClientCode, --ClientCode
	@ClientName, --ClientName
	NULL, --ClientUri
	NULL, --LogoUri
	1, --ClientCredentials
	NULL, --LogoutUri
	36000, --IdentityTokenLifetime
	36000, --AccessTokenLifetime
	36000, --AuthorizationCodeLifetime
	86400, --AbsoluteRefreshTokenLifetime
	36000, --SlidingRefreshTokenLifetime
	0, --RefreshTokenUsage
	0, --RefreshTokenExpiration
	0, --AccessTokenType
	1, --UpdateAccessTokenOnRefresh
	1, --Enabled
	0, --LogoutSessionRequired
	0, --RequireSignOutPrompt
	1, --AllowAccessToAllScopes
	0, --AllowClientCredentialsOnly
	0, --RequireConsent
	1, --AllowRememberConsent
	1, --EnableLocalLogin
	1, --IncludeJwtId
	1, --AlwaysSendClientClaims
	1, --PrefixClientClaims
	1 --AllowAccessToAllGrantTypes
	);
	SELECT @ClientId = ClientId
	FROM @AuthClients;
	INSERT INTO Auth.ClientScopes( ClientId, Scope )
		   SELECT @ClientId, --ClientId
		   ScopeId --Scope
		   FROM Auth.Scopes
		   WHERE Name = @ClientCode;
	INSERT INTO Auth.ClientSecrets( ClientId, Value, Type, Description, Expiration )
	VALUES( @ClientId, 'B9tS7xrNPRIwk6AXBrzddE3zeCwUwo87wLgaS7HKfKc=', NULL, @ClientName, '2099-12-31 00:00:00.0000000 -06:00' );

	Insert into Auth.ClientRedirectUris(ClientRedirectUriId,Uri)
	Values(@ClientId,N'http://54.189.37.40/sjilm')

END;
--[Guid("07BE535C-43FD-4B77-8A5C-8C92DEEE748D")]


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
		N'http://54.189.37.40/sjilm', --LoginUri
		N'NA', --SigningCertificateThumbprint
		N'productUsername' --SubjectIdSamlAttribute
	)
END

  --insert into [IdentityDevelopment].[Enterprise].[OrganizationProduct] (PartyId,ConfigurationId,ProductId,FromDate,ThruDate)
  --Select 3,1065,39,getdate(),null
  GO