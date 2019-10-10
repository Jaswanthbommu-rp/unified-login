GO
-- http://jira.realpage.com/browse/GB-1938
IF NOT EXISTS( SELECT TOP 1 1 from enterprise.ProductType where ProductTypeId = 110 )
BEGIN
	INSERT INTO Enterprise.ProductType ( ProductTypeId, ParentProductTypeId, Name, Description, ProductTypeGuid ) 
		VALUES
		( 110, 100, 'Document Management', 'Document Management', '8E52709F-A636-408E-8F77-CF4FC8F91B18' )
END
ELSE
BEGIN
	UPDATE Enterprise.ProductType set [Name] = 'Document Management', Description = 'Document Management', ParentProductTypeId = 100 WHERE ProductTypeId = 110
END

UPDATE Enterprise.Product SET name = 'Document Management', ProductTypeId = 110, BooksProductCode = 'DOC', description = 'A robust enterprise-level, content management system that gives Companies the ability to securely store, organize, search and publish all types of documents.  It provides a disaster recovery solution and reduces expenses such as supplies, storage and time spent filing and searching for documents.' where ProductId = 20
GO

UPDATE Enterprise.ProductSetting set value = 'Document Management' WHERE ProductId = 20 AND ProductSettingTypeId = 5
GO

UPDATE Enterprise.ProductSetting set value= '1' where productid = 20 and ProductSettingTypeId = (SELECT productsettingtypeid FROM Enterprise.ProductSettingType where name = 'ShowInUserListFilter' ) and value != '1'
GO
UPDATE Enterprise.ProductSetting set value= '1' where productid = 20 and ProductSettingTypeId = (SELECT productsettingtypeid FROM Enterprise.ProductSettingType where name = 'ProductAPIRequiresUser' ) and value != '1'
GO

DECLARE @ProdConfig AS ProductConfigurationType;

DECLARE @ProductID INT;
DECLARE @LoginURI NVARCHAR(100);
DECLARE @SigningCertificateThumbprint NVARCHAR(50);

INSERT INTO @ProdConfig (SettingName, SettingDescription, SettingValue)
	SELECT 'Subsolution', 'Enterprise Solution', 'Enterprise Solution'

INSERT INTO @ProdConfig (SettingName, SettingDescription, SettingValue)
	SELECT 'ApiUserName', 'The user to call the api with', 'c3NvQGtiZWU='

INSERT INTO @ProdConfig (SettingName, SettingDescription, SettingValue)
	SELECT 'ApiPassword', 'The password to call the api with', 'MUFxcXFxcXE='

	INSERT INTO @ProdConfig (SettingName, SettingDescription, SettingValue)
	SELECT 'ApiEndPoint', 'The url for the api', 'https://testing-rpdm.realpage.com'

SET @ProductID = 20
SET @LoginURI = 'https://testing-rpdm.realpage.com/sso/login'
SET @SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6'

EXEC Enterprise.ProductConfigurationSetup 
       @ProductId,
       @LoginURI,
       @SigningCertificateThumbprint,
       @ProdConfig

GO

-- http://jira.realpage.com/browse/GB-2331

UPDATE UL
	SET UL.PasswordHash = NULL,
	UL.PasswordSalt = NULL
FROM Enterprise.MasterCOnfigurationType MCT
     INNER JOIN Enterprise.MasterSettingTYpe MST ON MST.MasterConfigurationTypeId = MCT.MasterCOnfigurationTypeId
     INNER JOIN ENterprise.MasterSetting MS ON MS.MasterSettingTypeId = MST.MasterSettingTYpeId
     INNER JOIN Enterprise.Party P ON P.RealPageId = MS.Value
     INNER JOIN Person.Persona PE ON PE.PersonPartyId = P.PartyId
	 INNER JOIN Ident.UserLogin UL ON UL.UserId = PE.UserId
WHERE mct.Name = 'Organization'
      AND mst.Name IN('RealPageEmployeeAccessID')
	AND PE.PersonaId <> 33

GO

IF EXISTS
(
    SELECT 1
    FROM Enterprise.StatusType
    WHERE Name = 'Unity-GB'
)
    BEGIN
        UPDATE Enterprise.StatusType
          SET
              Name = 'UnifiedPlatform'
        WHERE Name = 'Unity-GB';
    END;
IF EXISTS
(
    SELECT 1
    FROM Enterprise.StatusType
    WHERE Name = 'Unity-ExcelImport'
)
    BEGIN
        UPDATE Enterprise.StatusType
          SET
              Name = 'ExternalImport'
        WHERE Name = 'Unity-ExcelImport';
    END;
IF EXISTS
(
    SELECT 1
    FROM Enterprise.StatusType
    WHERE Name = 'Unity-MigrationTool'
)
    BEGIN
        UPDATE Enterprise.StatusType
          SET
              Name = 'MigrationTool'
        WHERE Name = 'Unity-MigrationTool';
    END;
	GO
	--
-- New RP System ROle

DECLARE @OrganizationId INT;
DECLARE @RoleId INT;
DECLARE @OrgRowNum INT;
DECLARE @PerRowNum INT;
DECLARE @PerPriv INT;
DECLARE @RoleName VARCHAR(200);
DECLARE @RightID INT;
DECLARE @ActionID INT;
DECLARE @Status INT;
DECLARE @UserActionID INT;
DECLARE @PersonRoleID INT;
DECLARE @Status_Role INT;
DECLARE @Status_Right INT;
DECLARE @HoldUserId INT;
IF OBJECT_ID('tempdb..#HoldOrgs') IS NULL
    BEGIN
        CREATE TABLE #HoldOrgs
(RowNumber           INT IDENTITY(1, 1),
 OrganizationPartyID INT,
 PStatus             BIT DEFAULT 0
);
    END;
BEGIN
    SELECT @Status_Right = ST.StatusTypeId
    FROM Enterprise.StatusTypeCategoryType AS STCT
         JOIN Enterprise.StatusTypeCategory AS STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
         JOIN Enterprise.StatusTypeCategoryClassification AS STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
         JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = STCC.StatusTypeId
    WHERE STC.Name = 'Right Type'
          AND ST.Name = 'System';
    SELECT @Status_Role = ST.StatusTypeId
    FROM Enterprise.StatusTypeCategoryType AS STCT
         JOIN Enterprise.StatusTypeCategory AS STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
         JOIN Enterprise.StatusTypeCategoryClassification AS STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
         JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = STCC.StatusTypeId
    WHERE STC.Name = 'Role Type'
          AND ST.Name = 'System';
    INSERT INTO #HoldOrgs(OrganizationPartyID)
           SELECT DISTINCT
                  OrganizationPartyID
           FROM Person.Persona AS P
                INNER JOIN Enterprise.Organization AS O ON P.OrganizationPartyId = O.PartyId;
    WHILE EXISTS
	(
		SELECT 1
		FROM #HoldOrgs
		WHERE PStatus = 0
	)
        BEGIN
            SELECT TOP 1 @OrganizationId = OrganizationPartyID,
                         @OrgRowNum = RowNumber
            FROM #HoldOrgs
            WHERE PStatus = 0;
            IF NOT EXISTS
			(
				SELECT 1
				FROM Enterprise.Role AS R
					 INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
				WHERE value = 'Read only for Unified Platform'
					  AND PartyID = @OrganizationId
			)
                BEGIN
                    EXEC Enterprise.CreateRole
                         @RoleName = N'Read only for Unified Platform',
                         @ShortName = 'ROForUnifiedPlatform',
                         @Description = N'Read only for Unified Platform',
                         @RoleTypeID = 402,
                         @RoleCategoryId = @Status_Role,
                         @PartyID = @OrganizationId,
                         @RoleID = @RoleID OUTPUT;
                    SET @RoleName = 'Read only for Unified Platform';
                    
					SELECT @RoleID = RoleId
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = @RoleName
                          AND PartyId = @OrganizationId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to view users',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    

                    
					EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to edit my own profile',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to view roles and rights',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
					

                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Access to Product Learning Portal',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Ability to view audit trail on user data',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Default_SideMenu_Users',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    EXECUTE Enterprise.CreateRight
                            @RoleId = @RoleId,
                            @PartyId = @OrganizationId,
                            @ProductId = 3,
                            @RightName = 'Default_Dashboard_Users',
                            @RightCategoryId = @Status_Right,
                            @RightID = @RightID OUTPUT,
                            @Description = '';
                    SELECT @RightId;
                    
					SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'SideMenu'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User'
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Default_SideMenu_Users'
                          AND RoleId = @RoleID;
                    EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

                    SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'Dashboard'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Default_dashboard_Users'
                          AND RoleId = @RoleID;
                    EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;
						 
					SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'EditUser'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'Edit User Route'
						  AND ParentActionId IS NULL
 					SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to edit my own profile'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

					SELECT @ActionID = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'UsersList'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to view users'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

					SELECT @ActionId = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'EditUser'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'Edit User Route';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to view audit trail on user data'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

					SELECT @ActionId = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'RolesAndRights'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Ability to view roles and rights'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;
						
					SELECT @ActionId = ActionID
                    FROM Enterprise.ACTION
                    WHERE ObjectValue = 'Dashboard'
                          AND ObjectType = 'ROUTE'
                          AND Description = 'User';
                    SELECT @RoleID = RoleID
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE value = 'Read only for Unified Platform'
                          AND partyid = @OrganizationId;
                    SELECT @RightID = RightId
                    FROM Enterprise.[Right] AS R
                         INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                    WHERE Value = 'Access to Product Learning Portal'
                          AND RoleId = @RoleID;
					EXEC Enterprise.LinkActionToRights
                         @ActionID = @ActionID,
                         @RightId = @RightId,
                         @StatusId = @Status_Right,
                         @UserActionId = @UserActionId OUTPUT;

                END;
            UPDATE #HoldOrgs
              SET
                  PStatus = 1
            WHERE RowNumber = @OrgRowNum;
        END;
END;

GO
--GB-833

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
VALUES
( 1, 'Ability to access / view Settings Management Console', 'This Right should be used by the Unified Login team to display a Settings Management Resource Tile for RealPage Employees in the RealPage Company, and allow them to view Settings & Questionnaire Content Details.', 'AccessSettingMGMTConsole' )
, ( 2, 'Ability to manage Settings in Settings Management Console', 'This Right will be used by Unified Settings to control which RealPage Employee can edit Settings content.', 'ManageSettingMGMTConsole' )
, ( 3, 'Ability to manage Implementaion Questionnaires in Settings Management Console', 'This Right will be used by Unified Settings to control who can edit Questioinnaire content.', 'ManageQuestionnairesMGMTConsole' )

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
	  ST.Name = 'System';

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
	  ST.Name = 'System';

SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
     JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
     JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
     JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'Internal Only'
      AND StatusTypeCategoryType.Name = 'Security';



IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Access Settings in MGMTConsole' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Access Settings in MGMTConsole', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = 'Ability to access / view Settings Management Console', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'SupportTool' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Access Settings in MGMTConsole' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Access Settings in MGMTConsole', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

-->
IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Manage Settings in MGMTConsole' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Manage Settings in MGMTConsole', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = 'Ability to manage Settings in Settings Management Console  ', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'SupportTool' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Manage Settings in MGMTConsole' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Manage Settings in MGMTConsole', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;
-->
IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Implement Questionaires in MGMTConsole' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Implement Questionaires in MGMTConsole', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = 'Ability to manage Implementaion Questionnaires in Settings Management Console', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'SupportTool' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Implement Questionaires in MGMTConsole' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Implement Questionaires in MGMTConsole', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;


SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
WHERE O.Name = 'RealPage Employee'

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
	
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_AccessSettingsinMGMTConsole', @ShortName = 'AccessSettingMGMTConsole', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
	SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access Settings in MGMTConsole' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
		

	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ManageSettingsinMGMTConsole', @ShortName = 'ManageSettingMGMTConsole', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
	SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage Settings in MGMTConsole' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
			
	EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ManageQuestionnairesMGMTConsole', @ShortName = 'ManageQuestionnairesMGMTConsole', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '', @RightId = @RightId OUTPUT;
    SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Implement Questionaires in MGMTConsole' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	

	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #RightsUnifiedSettings;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @RightId = @RightId OUTPUT;
		
		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	

	SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Ability to access / view Settings Management Console'
		AND R.PartyId = @PartyId

		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Access Settings in MGMTConsole' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	

	SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Ability to manage Settings in Settings Management Console'
		AND R.PartyId = @PartyId

		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage Settings in MGMTConsole' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	


	SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Ability to manage Implementaion Questionnaires in Settings Management Console'
		AND R.PartyId = @PartyId

		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Implement Questionaires in MGMTConsole' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;
	
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

DECLARE @Access INT
DECLARE @Manage INT
DECLARE @Questions INT

SELECT  @Access = RightValueTypeId
FROM Enterprise.RightValueType 
WHERE value = 'Default_AccessSettingsinMGMTConsole';

SELECT  @Manage = RightValueTypeId
FROM Enterprise.RightValueType 
WHERE value = 'Default_ManageSettingsinMGMTConsole';

SELECT  @Questions = RightValueTypeId
FROM Enterprise.RightValueType 
WHERE value = 'Default_ManageQuestionnairesMGMTConsole';


SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to access / view Settings Management Console');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @Access
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @Access );
END;

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to manage Settings in Settings Management Console');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @Manage
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @Manage );
END;

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Ability to manage Implementaion Questionnaires in Settings Management Console');

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @Questions
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @Questions );
END;

GO


DECLARE @VisibilityStatus INT  
DECLARE @RightTypeId INT         

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
	'Ability to access / view Settings Management Console'
	,'Ability to manage Settings in Settings Management Console'
	,'Ability to manage Implementaion Questionnaires in Settings Management Console'
	,'Default_AccessSettingsinMGMTConsole'
	,'Default_ManageSettingsinMGMTConsole'
	,'Default_ManageQuestionnairesMGMTConsole'
	)

SELECT @RightTypeId =  StatusType.StatusTypeID
         FROM Enterprise.StatusTypeCategoryType
              JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
              JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
              JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
         WHERE StatusType.name = 'System'
				AND Enterprise.StatusTypeCategory.Name = 'Role Type'
               AND StatusTypeCategoryType.Name = 'Security';
UPDATE Enterprise.RightValueType
	SET StatusTypeId = @RightTypeId 
WHERE StatusTypeId IS NULL
GO


--ILMDECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;
DECLARE @ProductID INT;
DECLARE @LoginURI NVARCHAR(100);
DECLARE @SigningCertificateThumbprint NVARCHAR(50);
DECLARE @ProductConfiguration ProductConfigurationType

SET @ProductId = 40
SET @LoginURI = 'http://ilmbeta.slopejet.com/sjilm';
SET @SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6';

IF NOT EXISTS( SELECT TOP 1 1 from enterprise.ProductType where ProductTypeId = 308 )
BEGIN
	INSERT INTO Enterprise.ProductType ( ProductTypeId, ParentProductTypeId, Name, Description, ProductTypeGuid ) 
		VALUES
		( 308, 300, 'Intelligent Lead Management', 'Intelligent Lead Management', 'B7E1D8CA-ADB4-4DB6-8804-2A5CD9323FA4' )
END

IF NOT EXISTS( SELECT TOP 1 1 from enterprise.ProductType where ProductTypeId = 309 )
BEGIN
	INSERT INTO Enterprise.ProductType ( ProductTypeId, ParentProductTypeId, Name, Description, ProductTypeGuid ) 
		VALUES
		( 309, 300, 'Intelligent Lead Management-Leasing Analytics', 'Intelligent Lead Management-Leasing Analytics', '96455F0B-B9C5-49ED-ABBF-BFE4D7B7D7F4' )
END


IF NOT EXISTS(SELECT 1 FROM Enterprise.Product WHERE Name = 'Intelligent Lead Management')
BEGIN
	EXEC Enterprise.CreateProduct @ProductId = @ProductId, @ProductGUID = '29D12BD2-DBDA-41C0-8A60-2364B7FDF46E', @Name = 'Intelligent Lead Management', @Description = 'Intelligent Lead Management', @ProductTypeId = 308
	UPDATE Enterprise.Product Set BooksProductCode = 'ILMLM' WHERE ProductId = @ProductId
END

INSERT INTO @ProductConfiguration
VALUES( 'GetRoleEndpoint', 'Role End point for product API', '/roles' );

INSERT INTO @ProductConfiguration
VALUES( 'GetProfileEndpoint', 'GET Profile Endpoint for product API', '/users/profile?loginName={0}' );

INSERT INTO @ProductConfiguration
VALUES( 'GetUserEndpoint', 'GET User Endpoint for product API', '/users?loginName={0}' );

INSERT INTO @ProductConfiguration
VALUES( 'PostUserEndpoint', 'POST User Endpoint for product API', '/users' );

INSERT INTO @ProductConfiguration
VALUES( 'PutUserEndpoint', 'PUT User Endpoint for product API', '/users' );

INSERT INTO @ProductConfiguration
VALUES( 'PatchUserEndpoint', 'PATCH User Endpoint for product API', '/users?loginName={0}' );

INSERT INTO @ProductConfiguration
VALUES( 'PatchProfileEndpoint', 'PATCH Profile Endpoint for product API', '/users/profile' );

INSERT INTO @ProductConfiguration
VALUES( 'GetPropertyEndpoint', 'GET Property Endpoint for product API', '/properties?companyId={0}' );

INSERT INTO @ProductConfiguration
VALUES( 'ClientId', NULL, 'ilm' );

INSERT INTO @ProductConfiguration
VALUES( 'ClassName', NULL, 'ilm' );

INSERT INTO @ProductConfiguration
VALUES( 'ProductUrl', NULL, '/product/ilmleadmanagement' );

INSERT INTO @ProductConfiguration
VALUES( 'TitleId', NULL, 'Intelligent Lead Management' );

INSERT INTO @ProductConfiguration
VALUES( 'TitleUniqueId', NULL, '42C5F72C-BAD0-4075-9D17-6AE3AFA50C91' );

INSERT INTO @ProductConfiguration
VALUES( 'IsNewTab', NULL, '1' );

INSERT INTO @ProductConfiguration
VALUES( 'MetatagUniqueId', NULL, 'ILM-Lead Management' );

INSERT INTO @ProductConfiguration
VALUES( 'IsResource', NULL, '0' );

INSERT INTO @ProductConfiguration
VALUES( 'ApiEndPoint', NULL, 'http://ilmbeta.slopejet.com/ilmcustmgmt/ilm-lm' );

INSERT INTO @ProductConfiguration
VALUES( 'ProductStatus', NULL, '8' );

INSERT INTO @ProductConfiguration
VALUES( 'ShowInUserDetails', NULL, '1' );

INSERT INTO @ProductConfiguration
VALUES( 'RequiresUnifiedLoginRight', NULL, '0' );

INSERT INTO @ProductConfiguration
VALUES( 'ShowInRolesAndRights', NULL, '0' );

INSERT INTO @ProductConfiguration
VALUES( 'ShowInAppSwitcher', NULL, '1' );

INSERT INTO @ProductConfiguration
VALUES( 'ShowInUserListFilter', NULL, '1' );

INSERT INTO @ProductConfiguration
VALUES( 'ProductAPIRequiresUser', NULL, '0' );

EXEC Enterprise.ProductConfigurationSetup @ProductId, @LoginURI, @SigningCertificateThumbprint, @ProductConfiguration;


GO

DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;
DECLARE @ProductID INT;
DECLARE @LoginURI NVARCHAR(100);
DECLARE @SigningCertificateThumbprint NVARCHAR(50);

SET @ProductId = 41;
SET @LoginURI = 'http://ilmbeta.slopejet.com/sjila';
SET @SigningCertificateThumbprint = 'EF26FEC08C554976572E8A9767DDA437AC452CF6';


IF NOT EXISTS(SELECT 1 FROM Enterprise.Product WHERE Name = 'Intelligent Lead Management-Leasing Analytics ')
BEGIN
	EXEC Enterprise.CreateProduct @ProductId = @ProductId, @ProductGUID = '9427E995-5712-4978-AB2E-99AD547B6DBA', @Name = 'Intelligent Lead Management-Leasing Analytics ', @Description = 'Intelligent Lead Management-Leasing Analytics ', @ProductTypeId = 309
	UPDATE Enterprise.Product Set BooksProductCode = 'ILMLA' WHERE ProductId = @ProductId
END

INSERT INTO @ProductConfiguration
VALUES( 'GetRoleEndpoint', 'Role End point for product API', '/roles' );

INSERT INTO @ProductConfiguration
VALUES( 'GetPropertyGroupsEndpoint', 'GET Groups Endpoint for product API', '/propertyGroups?companyId={0}' );

INSERT INTO @ProductConfiguration
VALUES( 'GetProfileEndpoint', 'GET Profile Endpoint for product API', '/users/profile?loginName={0}' );

INSERT INTO @ProductConfiguration
VALUES( 'GetUserEndpoint', 'GET User Endpoint for product API', '/users?loginName={0}' );

INSERT INTO @ProductConfiguration
VALUES( 'PostUserEndpoint', 'POST User Endpoint for product API', '/users' );

INSERT INTO @ProductConfiguration
VALUES( 'PutUserEndpoint', 'PUT User Endpoint for product API', '/users' );

INSERT INTO @ProductConfiguration
VALUES( 'PatchUserEndpoint', 'PATCH User Endpoint for product API', '/users?loginName={0}' );

INSERT INTO @ProductConfiguration
VALUES( 'PatchProfileEndpoint', 'PATCH Profile Endpoint for product API', '/users/profile' );

INSERT INTO @ProductConfiguration
VALUES( 'GetPropertyEndpoint', 'GET Property Endpoint for product API', '/properties?companyId={0}' );

INSERT INTO @ProductConfiguration
VALUES( 'ClientId', NULL, 'ilmla' );

INSERT INTO @ProductConfiguration
VALUES( 'ClassName', NULL, 'ilmla' );

INSERT INTO @ProductConfiguration
VALUES( 'ProductUrl', NULL, '/product/ilmleadanalytics' );

INSERT INTO @ProductConfiguration
VALUES( 'TitleId', NULL, 'Intelligent Lead Management-Leasing Analytics' );

INSERT INTO @ProductConfiguration
VALUES( 'TitleUniqueId', NULL, '4FD51401-4F7A-4BA2-88AA-15B6706BFCEF' );

INSERT INTO @ProductConfiguration
VALUES( 'IsNewTab', NULL, '1' );

INSERT INTO @ProductConfiguration
VALUES( 'MetatagUniqueId', NULL, 'ILM-Leasing Analytics' );

INSERT INTO @ProductConfiguration
VALUES( 'IsResource', NULL, '0' );

INSERT INTO @ProductConfiguration
VALUES( 'ApiEndPoint', NULL, 'http://ilmbeta.slopejet.com/ilmcustmgmt/ilm-la' );

INSERT INTO @ProductConfiguration
VALUES( 'ProductStatus', NULL, '8' );

INSERT INTO @ProductConfiguration
VALUES( 'ShowInUserDetails', NULL, '1' );

INSERT INTO @ProductConfiguration
VALUES( 'RequiresUnifiedLoginRight', NULL, '0' );

INSERT INTO @ProductConfiguration
VALUES( 'ShowInRolesAndRights', NULL, '0' );

INSERT INTO @ProductConfiguration
VALUES( 'ShowInAppSwitcher', NULL, '1' );

INSERT INTO @ProductConfiguration
VALUES( 'ShowInUserListFilter', NULL, '1' );

INSERT INTO @ProductConfiguration
VALUES( 'ProductAPIRequiresUser', NULL, '0' );         


--SELECT * FROM Enterprise.ProductSettingType
--WHERE ProductSettingTypeId BETWEEN 1010 AND 1015

EXEC Enterprise.ProductConfigurationSetup @ProductId, @LoginURI, @SigningCertificateThumbprint, @ProductConfiguration;
             
GO

--Managemnt Settinsg


DECLARE @ProdConfig AS PRODUCTCONFIGURATIONTYPE;
DECLARE @ServerName NVARCHAR(100) = @@SERVERNAME
DECLARE @ProductID INT= 43;
DECLARE @LoginURI NVARCHAR(100)= CASE 
	WHEN @ServerName = 'RCDUSODBSQL001' THEN 'https://settings-management-dev.corp.realpage.com/signin-oidc' --DEV
	WHEN @ServerName = 'RCTUSODBSQL001' THEN 'https://settings-management-qa.realpage.com/signin-oidc' --QA
	WHEN @ServerName = 'RCQUSODBSQL001' THEN 'https://settings-management-sat.realpage.com/signin-oidc' -- SAT
	WHEN @ServerName IN ('RCPGBKDBSQL005B', 'RCPGBKDBSQL005A') THEN 'https://settings-management.realpage.com/signin-oidc' -- Prod
	WHEN @ServerName = 'RCVEUSODBSQL001' THEN 'https://settings-management-demo.realpage.com/signin-oidc' --DEMO
	--Update in future once we have all the environments.
	--WHEN @ServerName = '' THEN 'https://settings-management-int.realpage.com/signin-oidc' --INT
 	--WHEN @ServerName = '' THEN 'https://settings-management-uat.realpage.com/signin-oidc' --UAT
	--WHEN @ServerName = '' THEN 'https://settings-management-training.realpage.com/signin-oidc' --Training
	END
	


DECLARE @SigningCertificateThumbprint NVARCHAR(50)= 'EF26FEC08C554976572E8A9767DDA437AC452CF6';
DECLARE @ProductName NVARCHAR(100)= 'Settings Management';
DECLARE @ConfigurationId INT= 0;

IF NOT EXISTS
(
	SELECT TOP 1 1
	FROM Enterprise.Product
	WHERE ProductId = @ProductId AND 
		  Name = @ProductName
)
BEGIN
	INSERT INTO Enterprise.Product( ProductId, ProductGUID, Name, Description, ProductTypeId, BooksProductCode )
		   SELECT @ProductId, --ProductId
		   N'D730CE4A-5319-457B-AAE5-1B78A8E08BE1', @ProductName, --Name
		   @ProductName, --Description
		   NULL AS ProductTypeID, 'SM';

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'ClientId', NULL, '1';

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'ClassName', NULL, 'settingsmanagement';

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'TitleId', NULL, @ProductName;

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'TitleUniqueId', NULL, '2B287707-F3EE-45F8-AFA7-D252DCB9E3CA';

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'IsNewTab', NULL, '1';

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'MetatagUniqueId', NULL, 'SettingsManagement';

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'IsFavorite', NULL, '0';

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'IsResource', NULL, '1';

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'LearnMore', NULL, 'https://www.realpage.com/';

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'ProductStatus', 'Show if the external application was configured for the dashboard user.', '8';

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'ProductUrl', NULL, '/product/settingsmanagement';

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'ShowInUserDetails', 'Should the product show in the New/Edit user pages', '0';

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'ShowInRolesAndRights', 'Should the product show in the Role/Rights page', '0';

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'ShowInAppSwitcher', 'Should the product show in the application switcher', '0';

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'ShowInUserListFilter', 'Should the product show in the user list product pick list', '0';

	INSERT INTO @ProdConfig( SettingName, SettingDescription, SettingValue )
		   SELECT 'ProductAPIRequiresUser', 'Does the product require a user for api calls', '0';

	EXEC Enterprise.ProductConfigurationSetup @ProductId, @LoginURI, @SigningCertificateThumbprint, @ProdConfig;

	--Product Configuration
	SELECT @ConfigurationId = ConfigurationId
	FROM Enterprise.GlobalProductConfiguration
	WHERE thrudate IS NULL AND 
		  ProductID = @ProductId; --Settings Management

	IF(@ConfigurationId > 0)
	BEGIN
		INSERT INTO Enterprise.OrganizationProduct( PartyId, ConfigurationId, ProductId, FromDate )
			   SELECT eo.PartyId, @ConfigurationId, @ProductId, GETUTCDATE()
			   FROM Enterprise.Organization AS eo
					LEFT OUTER JOIN
					Enterprise.OrganizationProduct AS eop
					ON( eo.PartyId = eop.PartyId AND 
						eop.ProductId = @ProductId
					  )
			   WHERE Name IN( 'RealPage Employee' ) AND 
					 eop.OrganizationProductId IS NULL;
	END;
END;
GO




DECLARE @MasterConfigurationId INT;
DECLARE @MasterSettingTypeId INT;
DECLARE @MasterSettingId INT;

SELECT @MasterConfigurationId = MasterConfigurationId
FROM EnterPrise.MasterConfiguration
WHERE MasterConfigurationTypeId = 1;

SELECT @MasterSettingTypeId = [MasterSettingTypeId]
FROM [Enterprise].[MasterSettingType]
WHERE Name = N'IdentityServerCorsAllowedOrigins';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.MasterSetting
	WHERE MasterSettingTypeId = @MasterSettingTypeId AND 
		  Value = 'https://ilmbeta.slopejet.com'
)
BEGIN
	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
	VALUES( @MasterSettingTypeId, 'https://ilmbeta.slopejet.com', GETUTCDATE(), NULL );
	SELECT @MasterSettingId = SCOPE_IDENTITY();
	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
	VALUES( @MasterConfigurationId, @MasterSettingId );
END;