-- ================================================================
-- UNIFIED LOGIN - RIGHTS MANAGEMENT SCRIPT
-- ================================================================
-- Purpose: Create and map rights for SDE Extracts access
-- Date: September 25, 2025
-- ================================================================

-- Declare all variables once
DECLARE @UserAdminRoleId INT
DECLARE @CreateDate DATETIME
DECLARE @CreatedBy BIGINT
DECLARE @RightId INT
DECLARE @NavigationMenuIdReports INT
DECLARE @NavigationMenuIdActivityLog INT
DECLARE @NavigationMenuIdReportsParent INT
DECLARE @NavigationPageId NVARCHAR(50)
DECLARE @RightName NVARCHAR(255)
DECLARE @Description NVARCHAR(1000)
DECLARE @Value NVARCHAR(255)
DECLARE @TargetProductId INT
DECLARE @RouteId INT

-- Initialize common variables
SELECT @CreateDate = GETUTCDATE()

-- Get CreatedBy user ID (handle case where user might not exist)
SELECT TOP (1) @CreatedBy = UserId 
FROM Ident.UserLogin 
WHERE LoginName LIKE 'realpagead@%' ORDER BY LoginName

IF @CreatedBy IS NULL
BEGIN
    PRINT 'Warning: realpagead user not found, using system user ID 0'
    SET @CreatedBy = 0
END

-- Get Platform Administrator Role ID
SELECT @UserAdminRoleId = RoleId 
FROM Security.Role 
WHERE ShortName = 'SuperUser' 
  AND ProductId = 3

IF @UserAdminRoleId IS NULL
BEGIN
    PRINT 'ERROR: SuperUser role (ProductId=3) not found! Script cannot continue.'
    RETURN
END

-- Initialize right-specific variables
SET @RightName = 'AccesstoSDEExtractsonly'
SET @Value = 'Access to SDE Extracts only'
SET @Description = 'Allows user access to Reports > Manage Reports / Activity Log'
SET @TargetProductId = 67  -- Assuming same as ProductId
SET @NavigationPageId = 'Reports'  -- You may need to adjust this based on your navigation structure

PRINT 'Starting rights management script...'
PRINT 'Using CreatedBy: ' + CAST(@CreatedBy AS NVARCHAR(10))
PRINT 'Using UserAdminRoleId: ' + CAST(@UserAdminRoleId AS NVARCHAR(10))

-- ================================================================
-- STEP 1: CREATE THE RIGHT
-- ================================================================
PRINT 'Creating right: ' + @RightName

-- Create the right if it doesn't exist
IF NOT EXISTS (SELECT 1 FROM [Security].[Right] WHERE RightName = @RightName AND Value = @Value)
BEGIN
    INSERT INTO [Security].[Right] (
        RightName, 
        Description, 
        Value, 
        StatusTypeId, 
        VisibilityStatusId, 
        ProductId, 
        TargetProductId, 
        CreatedBy, 
        CreatedDate, 
        PersistRight, 
        IsExcludeRightFromImpersonation
    )
    VALUES (
        @RightName, 
        @Description, 
        @Value, 
        13,              -- Active status
        9,               -- Visible status
        3,               -- Product ID
        @TargetProductId, 
        @CreatedBy, 
        @CreateDate, 
        0,               -- Not persistent
        0                -- Not excluded from impersonation
    )
    
    PRINT 'Successfully created right: ' + @RightName
END
ELSE
BEGIN
    PRINT 'Right already exists: ' + @RightName
END

-- ================================================================
-- STEP 2: MAP RIGHT TO PLATFORM ADMINISTRATOR ROLE
-- ================================================================
PRINT 'Mapping right to Platform Administrator role...'

-- Get the RightId for the newly created or existing right
SELECT @RightId = RightID 
FROM [Security].[Right] 
WHERE RightName = @RightName AND Value = @Value

IF @RightId IS NOT NULL AND @UserAdminRoleId IS NOT NULL
BEGIN
    -- Map right to role if mapping doesn't exist
    IF NOT EXISTS(SELECT 1 FROM [Security].[RoleRight] WHERE RoleId = @UserAdminRoleId AND RightID = @RightId)
    BEGIN
        INSERT INTO [Security].[RoleRight](RoleId, RightId, CreatedBy, CreatedDate)
        VALUES(@UserAdminRoleId, @RightId, @CreatedBy, @CreateDate)
        PRINT 'Successfully mapped right to Platform Administrator role: ' + @RightName
    END
    ELSE
    BEGIN
        PRINT 'Role mapping already exists: ' + @RightName
    END
END
ELSE
BEGIN
    IF @RightId IS NULL 
        PRINT 'ERROR: Could not retrieve RightId for: ' + @RightName
    IF @UserAdminRoleId IS NULL 
        PRINT 'ERROR: Platform Administrator role ID is null'
END

-- ================================================================
-- STEP 3: MAP RIGHT TO NAVIGATION MENU (OPTIONAL)
-- ================================================================
PRINT 'Mapping right to navigation menu...'

-- Get the NavigationMenuId (you may need to adjust the PageId value)
SELECT @NavigationMenuIdReports = Id 
FROM Enterprise.NavigationMenu 
WHERE PageId = 'Manage Reports'

SELECT @NavigationMenuIdActivityLog = Id 
FROM Enterprise.NavigationMenu 
WHERE PageId = 'Report Activity Log'

SELECT @NavigationMenuIdReportsParent = Id 
FROM Enterprise.NavigationMenu 
WHERE PageId = 'reporting'

IF @RightId IS NOT NULL AND @NavigationMenuIdReports IS NOT NULL
BEGIN
    -- Map right to navigation menu if mapping doesn't exist
    IF NOT EXISTS(SELECT 1 FROM Enterprise.NavigationMenuRights WHERE RightId = @RightId AND NavigationMenuId = @NavigationMenuIdReports)
    BEGIN
        INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
        VALUES(@NavigationMenuIdReports, @RightId)
        PRINT 'Successfully mapped to navigation menu (Manage Reports): ' + @RightName
    END
    ELSE
    BEGIN
        PRINT 'Navigation mapping already exists: ' + @RightName
    END
END
ELSE
BEGIN
    IF @RightId IS NULL 
        PRINT 'Warning: Right not found for navigation mapping: ' + @RightName
    IF @NavigationMenuIdReports IS NULL 
        PRINT 'Warning: Navigation menu not found for PageId: ' + ISNULL(@NavigationPageId, 'NULL')
    PRINT 'Skipping navigation menu mapping...'
END

IF @RightId IS NOT NULL AND @NavigationMenuIdActivityLog IS NOT NULL
BEGIN
    -- Map right to navigation menu if mapping doesn't exist
    IF NOT EXISTS(SELECT 1 FROM Enterprise.NavigationMenuRights WHERE RightId = @RightId AND NavigationMenuId = @NavigationMenuIdActivityLog)
    BEGIN
        INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
        VALUES(@NavigationMenuIdActivityLog, @RightId)
        PRINT 'Successfully mapped to navigation menu (Report Activity Log): ' + @RightName
    END
    ELSE
    BEGIN
        PRINT 'Navigation mapping already exists: ' + @RightName
    END
END
ELSE
BEGIN
    IF @RightId IS NULL 
        PRINT 'Warning: Right not found for navigation mapping: ' + @RightName
    IF @NavigationMenuIdActivityLog IS NULL 
        PRINT 'Warning: Navigation menu not found for PageId: ' + ISNULL(@NavigationPageId, 'NULL')
    PRINT 'Skipping navigation menu mapping...'
END

IF @RightId IS NOT NULL AND @NavigationMenuIdReportsParent IS NOT NULL
BEGIN
    -- Map right to navigation menu if mapping doesn't exist
    IF NOT EXISTS(SELECT 1 FROM Enterprise.NavigationMenuRights WHERE RightId = @RightId AND NavigationMenuId = @NavigationMenuIdReportsParent)
    BEGIN
        INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
        VALUES(@NavigationMenuIdReportsParent, @RightId)
        PRINT 'Successfully mapped to navigation menu (Reports): ' + @RightName
    END
    ELSE
    BEGIN
        PRINT 'Navigation mapping already exists: ' + @RightName
    END
END
ELSE
BEGIN
    IF @RightId IS NULL 
        PRINT 'Warning: Right not found for navigation mapping: ' + @RightName
    IF @NavigationMenuIdReportsParent IS NULL 
        PRINT 'Warning: Navigation menu not found for PageId: ' + ISNULL(@NavigationPageId, 'NULL')
    PRINT 'Skipping navigation menu mapping...'
END

-- ================================================================
-- STEP 4: MAP RIGHT TO ROUTE (SideMenu)
-- ================================================================
PRINT 'Mapping right to SideMenu route...'

-- Get the RouteID for 'SideMenu'
SELECT @RouteId = RouteID 
FROM Security.Route 
WHERE RouteValue = 'SideMenu'

IF @RouteId IS NOT NULL AND @RightId IS NOT NULL
BEGIN
    -- Map right to route if mapping doesn't exist
    IF NOT EXISTS(SELECT 1 FROM Security.RightRoute WHERE RightId = @RightId AND RouteId = @RouteId)
    BEGIN
        INSERT INTO Security.RightRoute(RightId, RouteId, CreatedBy, CreatedDate)
        VALUES(@RightId, @RouteId, @CreatedBy, @CreateDate)
        PRINT 'Successfully mapped to SideMenu route: ' + @RightName
    END
    ELSE
    BEGIN
        PRINT 'Route mapping already exists for SideMenu: ' + @RightName
    END
END
ELSE
BEGIN
    IF @RouteId IS NULL 
        PRINT 'Warning: SideMenu route not found!'
    IF @RightId IS NULL 
        PRINT 'Warning: Right not found for route mapping: ' + @RightName
    PRINT 'Skipping route mapping...'
END

-- ================================================================
-- STEP 5: VERIFICATION
-- ================================================================
PRINT ''
PRINT '================================================================'
PRINT 'VERIFICATION RESULTS'
PRINT '================================================================'

-- Verify right creation
IF EXISTS(SELECT 1 FROM [Security].[Right] WHERE RightName = @RightName AND Value = @Value)
    PRINT 'Right exists: ' + @RightName
ELSE
    PRINT 'Right NOT found: ' + @RightName

-- Verify role mapping
IF EXISTS(SELECT 1 FROM [Security].[RoleRight] rr 
          INNER JOIN [Security].[Right] r ON rr.RightID = r.RightID 
          WHERE rr.RoleId = @UserAdminRoleId AND r.RightName = @RightName)
    PRINT 'Role mapping exists for Platform Administrator'
ELSE
    PRINT 'Role mapping NOT found for Platform Administrator'

-- Verify navigation mapping for Manage Reports
IF @NavigationMenuIdReports IS NOT NULL
BEGIN
    IF EXISTS(SELECT 1 FROM Enterprise.NavigationMenuRights nmr 
              INNER JOIN [Security].[Right] r ON nmr.RightId = r.RightID 
              WHERE nmr.NavigationMenuId = @NavigationMenuIdReports AND r.RightName = @RightName)
        PRINT 'Navigation mapping exists for Manage Reports'
    ELSE
        PRINT 'Navigation mapping NOT found for Manage Reports'
END

-- Verify navigation mapping for Reports
IF @NavigationMenuIdReportsParent IS NOT NULL
BEGIN
    IF EXISTS(SELECT 1 FROM Enterprise.NavigationMenuRights nmr 
              INNER JOIN [Security].[Right] r ON nmr.RightId = r.RightID 
              WHERE nmr.NavigationMenuId = @NavigationMenuIdReportsParent AND r.RightName = @RightName)
        PRINT 'Navigation mapping exists for Reports'
    ELSE
        PRINT 'Navigation mapping NOT found for Reports'
END

-- Verify navigation mapping for Report Activity Log
IF @NavigationMenuIdActivityLog IS NOT NULL
BEGIN
    IF EXISTS(SELECT 1 FROM Enterprise.NavigationMenuRights nmr 
              INNER JOIN [Security].[Right] r ON nmr.RightId = r.RightID 
              WHERE nmr.NavigationMenuId = @NavigationMenuIdActivityLog AND r.RightName = @RightName)
        PRINT 'Navigation mapping exists for Report Activity Log'
    ELSE
        PRINT 'Navigation mapping NOT found for Report Activity Log'
END

-- Verify route mapping
IF @RouteId IS NOT NULL
BEGIN
    IF EXISTS(SELECT 1 FROM Security.RightRoute rr 
              INNER JOIN [Security].[Right] r ON rr.RightId = r.RightID 
              WHERE rr.RouteId = @RouteId AND r.RightName = @RightName)
        PRINT 'Route mapping exists for SideMenu'
    ELSE
        PRINT 'Route mapping NOT found for SideMenu'
END

PRINT ''
PRINT 'Script completed successfully!'
PRINT 'Date/Time: ' + CONVERT(NVARCHAR(50), @CreateDate, 120)