/*
=================================================================
Complete Settings Rights Management Script
=================================================================
This script performs three operations in sequence:
1. Creates all security rights
2. Maps rights to User Administrator role  
3. Maps rights to appropriate navigation menus

Optimized to use a data-driven approach to reduce code redundancy.
=================================================================
*/

-- Declare all variables once
DECLARE @UserAdminRoleId INT
DECLARE @CreateDate DATETIME
DECLARE @CreatedBy BIGINT
DECLARE @RightId INT
DECLARE @NavigationMenuId INT
DECLARE @NavigationPageId NVARCHAR(50)
DECLARE @RightName NVARCHAR(255)
DECLARE @Description NVARCHAR(1000)
DECLARE @Value NVARCHAR(255)
DECLARE @Level NVARCHAR(20)
DECLARE @SettingType NVARCHAR(50)
DECLARE @TargetProductId INT

-- Initialize common variables
SELECT @CreateDate = GETUTCDATE()
SELECT @CreatedBy = UserId FROM Ident.UserLogin WHERE LoginName LIKE 'realpagead@%'
SELECT @UserAdminRoleId = RoleId FROM Security.Role WHERE RoleName = 'User Administrator' AND ShortName = 'SuperUser' AND ProductId = 3

-- Create temporary table to store rights configuration
CREATE TABLE #RightsConfig (
    [Level] NVARCHAR(20),
    SettingType NVARCHAR(50),
    TargetProductId INT,
    NavigationPageId NVARCHAR(50)
)

-- Insert rights configuration data
INSERT INTO #RightsConfig ([Level], SettingType, TargetProductId, NavigationPageId) VALUES
-- Company Level Rights
('Company', 'DocumentManagement', 56, 'manage-settings'),
('Company', 'LeasingandRents', 56, 'manage-settings'),
('Company', 'Platform', 56, 'manage-settings'),
('Company', 'Screening', 56, 'manage-settings'),

-- Property Level Rights  
('Property', 'DocumentManagement', 56, 'manage-settings'),
('Property', 'Insurance', 56, 'manage-settings'),
('Property', 'LeasingandRents', 56, 'manage-settings'),
('Property', 'LeasingPortals', 56, 'manage-settings'),
('Property', 'Military', 56, 'manage-settings'),
('Property', 'Payments', 56, 'manage-settings'),
('Property', 'ResidentPortalsLOFT', 56, 'manage-settings'),
('Property', 'Screening', 56, 'manage-settings'),
('Property', 'Senior', 56, 'manage-settings'),
('Property', 'Student', 56, 'manage-settings'),
('Property', 'Utilities', 56, 'manage-settings'),

-- Template Level Rights
('Template', 'DocumentManagement', 56, 'manage-templates'),
('Template', 'Insurance', 56, 'manage-templates'),
('Template', 'LeasingandRents', 56, 'manage-templates'),
('Template', 'LeasingPortals', 56, 'manage-templates'),
('Template', 'Military', 56, 'manage-templates'),
('Template', 'Payments', 56, 'manage-templates'),
('Template', 'ResidentPortalsLOFT', 56, 'manage-templates'),
('Template', 'Screening', 56, 'manage-templates'),
('Template', 'Senior', 56, 'manage-templates'),
('Template', 'Student', 56, 'manage-templates'),
('Template', 'Utilities', 56, 'manage-templates')

-- ================================================================
-- STEP 1: CREATE ALL SECURITY RIGHTS
-- ================================================================
PRINT 'Creating security rights...'

DECLARE rights_cursor CURSOR FOR
SELECT Level, SettingType, TargetProductId FROM #RightsConfig

OPEN rights_cursor
FETCH NEXT FROM rights_cursor INTO @Level, @SettingType, @TargetProductId

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Build right name and description dynamically
    SET @RightName = 'Manage' + @Level + 'Level' + @SettingType + 'SettingsOnly'
    SET @Value = 'Manage ' + @Level + ' Level ' + 
                 CASE 
                     WHEN @SettingType = 'LeasingandRents' THEN 'Leasing and Rents'
                     WHEN @SettingType = 'DocumentManagement' THEN 'Document Management'
                     WHEN @SettingType = 'LeasingPortals' THEN 'Leasing Portals'
                     WHEN @SettingType = 'ResidentPortalsLOFT' THEN 'Resident Portals/LOFT'
                     WHEN @SettingType = 'Platform' THEN 'Platform'
                     ELSE @SettingType
                 END + ' Settings Only'
    
    SET @Description = 'This user will only have access to manage ' + 
                      CASE 
                          WHEN @SettingType = 'LeasingandRents' THEN 'Leasing and Rents'
                          WHEN @SettingType = 'DocumentManagement' THEN 'Document Management'
                          WHEN @SettingType = 'LeasingPortals' THEN 'Leasing Portals'
                          WHEN @SettingType = 'ResidentPortalsLOFT' THEN 'Resident Portals/LOFT'
                          WHEN @SettingType = 'Platform' THEN 'Platform Services'
                          ELSE @SettingType
                      END + ' settings at ' + LOWER(@Level) + ' level. Must have Access to Unified Settings right.'

    -- Create the right if it doesn't exist
    IF NOT EXISTS (SELECT 1 FROM [security].[right] WHERE RightName = @RightName AND Value = @Value)
    BEGIN
        INSERT INTO [security].[right] (
            RightName, Description, Value, StatusTypeId, VisibilityStatusId, 
            ProductId, TargetProductId, CreatedBy, CreatedDate, PersistRight, IsExcludeRightFromImpersonation
        )
        VALUES (
            @RightName, @Description, @Value, 13, 9, 3, @TargetProductId, 
            0, @CreateDate, 0, 0
        )
        PRINT 'Created right: ' + @RightName
    END
    ELSE
    BEGIN
        PRINT 'Right already exists: ' + @RightName
    END

    FETCH NEXT FROM rights_cursor INTO @Level, @SettingType, @TargetProductId
END

CLOSE rights_cursor
DEALLOCATE rights_cursor

-- ================================================================
-- STEP 2: MAP RIGHTS TO USER ADMINISTRATOR ROLE
-- ================================================================
PRINT 'Mapping rights to User Administrator role...'

DECLARE role_cursor CURSOR FOR
SELECT Level, SettingType FROM #RightsConfig

OPEN role_cursor
FETCH NEXT FROM role_cursor INTO @Level, @SettingType

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @RightName = 'Manage' + @Level + 'Level' + @SettingType + 'SettingsOnly'
    
    -- Get the RightId
    SELECT @RightId = RightID FROM [security].[Right] WHERE RightName = @RightName
    
    IF @RightId IS NOT NULL AND @UserAdminRoleId IS NOT NULL
    BEGIN
        -- Map right to role if mapping doesn't exist
        IF NOT EXISTS(SELECT 1 FROM [security].[RoleRight] WHERE RoleId = @UserAdminRoleId AND RightID = @RightId)
        BEGIN
            INSERT INTO [security].RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
            VALUES(@UserAdminRoleId, @RightId, @CreatedBy, @CreateDate)
            PRINT 'Mapped to role: ' + @RightName
        END
        ELSE
        BEGIN
            PRINT 'Role mapping already exists: ' + @RightName
        END
    END

    FETCH NEXT FROM role_cursor INTO @Level, @SettingType
END

CLOSE role_cursor
DEALLOCATE role_cursor

-- ================================================================
-- STEP 3: MAP RIGHTS TO NAVIGATION MENUS
-- ================================================================
PRINT 'Mapping rights to navigation menus...'

DECLARE nav_cursor CURSOR FOR
SELECT Level, SettingType, NavigationPageId FROM #RightsConfig

OPEN nav_cursor
FETCH NEXT FROM nav_cursor INTO @Level, @SettingType, @NavigationPageId

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @RightName = 'Manage' + @Level + 'Level' + @SettingType + 'SettingsOnly'
    
    -- Get the RightId and NavigationMenuId
    SELECT @RightId = RightID FROM [security].[Right] WHERE RightName = @RightName
    SELECT @NavigationMenuId = Id FROM Enterprise.NavigationMenu WHERE PageId = @NavigationPageId
    
    IF @RightId IS NOT NULL AND @NavigationMenuId IS NOT NULL
    BEGIN
        -- Map right to navigation menu if mapping doesn't exist
        IF NOT EXISTS(SELECT 1 FROM Enterprise.NavigationMenuRights WHERE RightId = @RightId)
        BEGIN
            INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
            VALUES(@NavigationMenuId, @RightId)
            PRINT 'Mapped to navigation (' + @NavigationPageId + '): ' + @RightName
        END
        ELSE
        BEGIN
            PRINT 'Navigation mapping already exists: ' + @RightName
        END
    END
    ELSE
    BEGIN
        IF @RightId IS NULL PRINT 'Warning: Right not found: ' + @RightName
        IF @NavigationMenuId IS NULL PRINT 'Warning: Navigation menu not found for PageId: ' + @NavigationPageId
    END

    FETCH NEXT FROM nav_cursor INTO @Level, @SettingType, @NavigationPageId
END

CLOSE nav_cursor
DEALLOCATE nav_cursor

-- Clean up
DROP TABLE #RightsConfig

PRINT 'Script completed successfully!'
