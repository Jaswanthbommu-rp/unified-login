CREATE PROCEDURE [Enterprise].[SetupSuperUser]
(@OrganizationId    INT, --PartyId
 @FirstName         NVARCHAR(200), 
 @MiddleName        NVARCHAR(50), 
 @LastName          NVARCHAR(200), 
 @Title             NVARCHAR(50), 
 @Suffix            NVARCHAR(50), 
 @Email             NVARCHAR(200), 
 @DefaultIDP        BIT           = 1, 
 @IDPTypeId         INT           = NULL, 
 @AssignedProductId PRODUCTIDTYPE READONLY
)
AS
    BEGIN
/*
	 Initial Stored procedure to setup:
	  1. Super User for the newly created or existing organization
	  2. Setup intitial roles required for the organization
	  3. Assgin Super User privileges to the user
	 Bug 125452: 02/07/2018
	  Role assignment was updating or adding roles for all the super users
	  in the organization. Filtered it by just the one persona which is
	  actually required.
	 IDP Enhancements: 02/08/2018
	  ThirdPartyIDP related functionality is added and tested.
	 Added support to Edit own profile: 02/27/2018
	*/

        DECLARE @NOW DATETIME= GETUTCDATE(), @UserRealPageId UNIQUEIDENTIFIER, @OrganizationRealPageId UNIQUEIDENTIFIER, @OrganizationIDPCMId INT, @UserStatus INT, @UserId BIGINT, @PersonaId BIGINT, @RoleId INT, @ContactMechanismId BIGINT, @PartyContactMechanismId BIGINT, @ContactMechanismUsageId BIGINT, @TargetProductId INT, @UserLoginPersonaId BIGINT, @RoleTypeIdTo INT, @RoleTypeIdFrom INT;
        DECLARE @PlatformAdminRoleValue NVARCHAR(200);
	    SELECT @PlatformAdminRoleValue = ps.Value
	    FROM Enterprise.GlobalProductConfiguration gpc
	    JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
	    JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
	    JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
	    WHERE gpc.ProductId = 3
	     AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))
	     AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))
	     AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
	     AND pst.Name = 'PlatformAdminRole';
        IF
        (
            SELECT COUNT(*)
            FROM @AssignedProductId
        ) = 0
            BEGIN
                SELECT 0 AS Id, 
                       'Target ProductId list is empty.';
                RETURN;
        END;
        --User/RolesRights Declaration Block
        DECLARE @OrgRowNum INT, @PerRowNum INT, @PerPriv INT, @RoleName VARCHAR(200), @RightID INT, @ActionID INT, @Status INT, @UserActionID INT, @PersonRoleID INT, @Status_Role INT, @Status_Right INT, @VisibilityStatusId INT;
        IF OBJECT_ID('tempdb..#HoldOrgs') IS NULL
            BEGIN
                CREATE TABLE #HoldOrgs
                (RowNumber           INT IDENTITY(1, 1), 
                 OrganizationPartyID INT, 
                 PStatus             BIT DEFAULT 0
                );
        END;
        DECLARE @Pwdhash NVARCHAR(510)= '';
        DECLARE @PwdSalt NVARCHAR(510)= '';
        IF NOT EXISTS
        (
            SELECT 1
            FROM Ident.UserLogin
            WHERE LoginName = @Email
        )
            BEGIN
                SELECT @OrganizationRealPageId = RealPageId
                FROM Enterprise.Party
                WHERE PartyId = @OrganizationId;
                EXEC Person.CreatePerson 
                     @FirstName = @FirstName, 
                     @MiddleName = @MiddleName, 
                     @LastName = @LastName, 
                     @Title = @Title, 
                     @Suffix = @Suffix, 
                     @PreferredContactMethodId = 0, 
                     @RealPageId = @UserRealPageId OUTPUT;
                EXEC Ident.CreateUserLogin 
                     @RealPageId = @UserRealPageId, 
                     @LoginName = @Email, 
                     @CreateUserSourceType = 'UnifiedPlatform';
                SELECT @UserId = UserId
                FROM Ident.UserLogin
                WHERE LoginName = @Email;
                EXEC Ident.CreateUserLoginPersona 
                     @UserLoginId = @UserId, 
                     @StatusTypeId = 1, 
                     @OrganizationPartyId = @OrganizationId, 
                     @Primaryorganization = 1, 
                     @FromDate = @NOW, 
                     @ThruDate = '9999-12-31 00:00:00', 
                     @StatusThruDate = NULL;
                SELECT @UserLoginPersonaId = UserLoginPersonaId
                FROM Ident.UserLoginPersona
                WHERE UserLoginId = @UserId
                      AND OrganizationPartyId = @OrganizationId
                      AND PrimaryOrganization = 1;
                EXEC Person.CreatePersona 
                     @PersonRealPageId = @UserRealPageId, 
                     @UserLoginPersonaId = @UserLoginPersonaId, 
                     @OrganizationRealPageId = @OrganizationRealPageId, 
                     @PersonaTypeId = 1, 
                     @PersonaEnvironmentTypeId = 1, 
                     @UserId = @UserId, 
                     @FromDate = @NOW, 
                     @ThruDate = NULL, 
                     @PersonaId = @PersonaId OUTPUT;
                EXEC Person.UpdatePersona 
                     @PersonaId = @PersonaId, 
                     @PersonaTypeId = 3, 
                     @PersonaEnvironmentTypeId = 1;
                IF @DefaultIDP = 1
                   AND @IDPTypeId IS NULL
                    BEGIN
                        SELECT @OrganizationIDPCMId = i.ContactMechanismId
                        FROM Enterprise.Organization AS O
                             INNER JOIN Ident.IdentityProviderType AS i ON i.IdentityProviderTypeId = O.IdentityProviderTypeId
                        WHERE O.PartyId = @OrganizationId;
                        EXEC Ident.LinkIdentityProviderToUserLogin 
                             @UserId = @UserId, 
                             @ContactMechanismID = @OrganizationIDPCMId;
                END;
                    ELSE
                    BEGIN
                        SELECT @OrganizationIDPCMId = i.ContactMechanismId
                        FROM Enterprise.Organization AS O
                             INNER JOIN Ident.IdentityProviderType AS i ON i.IdentityProviderTypeId = O.IdentityProviderTypeId
                        WHERE i.IdentityProviderTypeId = @IDPTypeId;
                        EXEC Ident.LinkIdentityProviderToUserLogin 
                             @UserId = @UserId, 
                             @ContactMechanismID = @OrganizationIDPCMId;
                END;
                UPDATE Ident.UserLogin
                  SET 
                      PasswordHash = @Pwdhash, 
                      PasswordSalt = @PwdSalt, 
                      PasswordModifiedDate = DATEADD(YEAR, 50, GETDATE())
                FROM Ident.UserLogin
                WHERE UserId = @UserId;
                SELECT @RoleId = PartyRoleTypeId
                FROM Enterprise.RoleType
                WHERE Name = 'SuperUser';
                EXEC Enterprise.CreatePartyRoleByRealPageId 
                     @RealPageId = @UserRealPageId, 
                     @RoleTypeID = @RoleId;
                SELECT @RoleTypeIdFrom = PartyRoleTypeId
                FROM Enterprise.RoleType
                WHERE Name = 'Leasing Agent';
                SELECT @RoleTypeIdTo = PartyRoleTypeId
                FROM Enterprise.RoleType
                WHERE Name = 'Employer';
                EXEC Person.LinkPersonToOrganization 
                     @PersonRealPageId = @UserRealPageId, 
                     @OrganizationRealPageId = @OrganizationRealPageId, 
                     @RoleTypeIdFrom = @RoleTypeIdFrom, 
                     @RoleTypeIdTo = @RoleTypeIdTo;
                SELECT @RoleTypeIdTo = PartyRoleTypeId
                FROM Enterprise.RoleType
                WHERE Name = 'User Type';
                EXEC Person.LinkPersonToOrganization 
                     @PersonRealPageId = @UserRealPageId, 
                     @OrganizationRealPageId = @OrganizationRealPageId, 
                     @RoleTypeIdFrom = @RoleId, 
                     @RoleTypeIdTo = @RoleTypeIdTo;
                EXEC Person.CreateContactMechanism 
                     @ContactMechanismId = @ContactMechanismId OUTPUT;
                EXEC Person.LinkContactMechanismToParty 
                     @RealPageId = @UserRealPageId, 
                     @PartyContactMechanismId = 0, 
                     @ContactMechanismId = @ContactMechanismId, 
                     @FromDate = @NOW, 
                     @ThruDate = '9999-12-31 23:59:59.997';
                SELECT @PartyContactMechanismId = PartyContactMechanismId
                FROM Enterprise.PartyContactMechanism AS a
                     INNER JOIN Enterprise.Party AS p ON a.PartyId = p.PartyId
                WHERE RealPageId = @UserRealPageId;
                EXEC Person.LinkUsageTypeToPartyContactMechanism 
                     @PartyContactMechanismId = @PartyContactMechanismId, 
                     @ContactMechanismUsageTypeId = 301;
                SELECT @ContactMechanismUsageId = ContactMechanismUsageID
                FROM Enterprise.ContactMechanismUsage
                WHERE PartyContactMechanismID = @PartyContactMechanismId;
                EXEC Person.CreateElectronicAddress 
                     @ContactMechanismId = @ContactMechanismId, 
                     @ElectronicAddressString = @Email, 
                     @ElectronicAddressType = N'Email';
        END;
        ----------------------------
        /*Create USER ROLE for all the existing organizations*/
        /*Create Basic End User for all the existing organizations*/

        SELECT @Status_Right = ST.StatusTypeId
        FROM Enterprise.StatusTypeCategoryType AS STCT
             INNER JOIN Enterprise.StatusTypeCategory AS STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
             INNER JOIN Enterprise.StatusTypeCategoryClassification AS STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
             INNER JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = STCC.StatusTypeId
        WHERE STC.Name = 'Right Type'
              AND ST.Name = 'System';
        SELECT @Status_Role = ST.StatusTypeId
        FROM Enterprise.StatusTypeCategoryType AS STCT
             INNER JOIN Enterprise.StatusTypeCategory AS STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
             INNER JOIN Enterprise.StatusTypeCategoryClassification AS STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
             INNER JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = STCC.StatusTypeId
        WHERE STC.Name = 'Role Type'
              AND ST.Name = 'System';
        SELECT @VisibilityStatusId = StatusType.StatusTypeId
        FROM Enterprise.StatusTypeCategoryType
             INNER JOIN Enterprise.StatusTypeCategory ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
             INNER JOIN Enterprise.StatusTypeCategoryClassification ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
             INNER JOIN Enterprise.StatusType ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
        WHERE StatusType.Name = 'ALL'
              AND StatusTypeCategoryType.Name = 'Security';
        INSERT INTO #HoldOrgs(OrganizationPartyID)
        VALUES(@OrganizationId);
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
                    WHERE Value = 'Basic End User'
                          AND PartyID = @OrganizationId
                )
                    BEGIN
                        EXEC Enterprise.CreateRole 
                             @RoleName = N'Basic End User', 
                             @Description = N'', 
                             @RoleTypeID = 400, 
                             @PartyID = @OrganizationId, 
                             @RoleCategoryId = @Status_Role, 
							 @CreatedBy = @UserId,
                             @RoleID = @RoleId OUTPUT;
                        SET @RoleName = 'Basic End User';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE Value = @RoleName
                              AND PartyID = @OrganizationId;
                        UPDATE Enterprise.Role
                          SET 
                              DefaultRole = 1
                        WHERE RoleID = @RoleId;
                        SET @TargetProductId = 3;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to edit my own profile', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Access to Product Learning Portal', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        --EXECUTE Enterprise.CreateRight 
                        --        @RoleID = @RoleId, 
                        --        @PartyId = @OrganizationId, 
                        --        @ProductId = 3, 
                        --        @RightName = 'Access to Help Center', 
                        --        @RightCategoryId = @Status_Right, 
                        --        @RightID = @RightID OUTPUT, 
                        --        @Description = '', 
                        --        @TargetProductId = @TargetProductId, 
                        --        @VisibilityStatusId = @VisibilityStatusId;
                        --SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Default_SideMenu_Users', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '';
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Default_Dashboard_Users', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Vendor Marketplace';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Access to Vendor Marketplace', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'SideMenu'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'User';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE Value = 'Basic End User'
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Default_SideMenu_Users'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'Dashboard'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'User';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE Value = 'Basic End User'
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Default_Dashboard_Users'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'EditUser'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'User';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE Value = 'Basic End User'
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Ability to edit my own profile'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                END;
                SET @RoleId = NULL;

                IF NOT EXISTS
                (
                    SELECT 1
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE Value = 'Read only for Unified Platform'
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
							  @CreatedBy = @UserId,
                             @RoleID = @RoleId OUTPUT;
                        SET @RoleName = 'Read only for Unified Platform';
                        SET @TargetProductId = 3;
                        SELECT @RoleId = R.RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE Value = @RoleName
                              AND PartyID = @OrganizationId;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to view users', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to edit my own profile', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to view roles and rights', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Access to Product Learning Portal', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to view audit trail on user data', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Default_SideMenu_Users', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Default_Dashboard_Users', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Vendor Marketplace';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Access to Vendor Marketplace', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        --EXECUTE Enterprise.CreateRight 
                        --        @RoleID = @RoleId, 
                        --        @PartyId = @OrganizationId, 
                        --        @ProductId = 3, 
                        --        @RightName = 'Access to Help Center', 
                        --        @RightCategoryId = @Status_Right, 
                        --        @RightID = @RightID OUTPUT, 
                        --        @Description = '', 
                        --        @TargetProductId = @TargetProductId, 
                        --        @VisibilityStatusId = @VisibilityStatusId;
                        --SELECT @RightID;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'SideMenu'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'User';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE Value = 'Read only for Unified Platform'
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Default_SideMenu_Users'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'Dashboard'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'User';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE Value = 'Read only for Unified Platform'
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Default_dashboard_Users'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'EditUser'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'Edit User Route'
                              AND ParentActionId IS NULL;
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE Value = 'Read only for Unified Platform'
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Ability to edit my own profile'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'UsersList'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'User';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE Value = 'Read only for Unified Platform'
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Ability to view users'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'EditUser'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'Edit User Route';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE Value = 'Read only for Unified Platform'
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Ability to view audit trail on user data'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'RolesAndRights'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'User';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE Value = 'Read only for Unified Platform'
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Ability to view roles and rights'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'Dashboard'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'User';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE Value = 'Read only for Unified Platform'
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Access to Product Learning Portal'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'ActivityLog'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'User';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE Value = 'Read only for Unified Platform'
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Ability to view audit trail on user data'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                END;
                SET @RoleId = NULL;
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM Enterprise.Role AS R
                         INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                    WHERE Value = 'User Administrator'
                          AND PartyID = @OrganizationId
                )
                    BEGIN
                        EXEC Enterprise.CreateRole 
                             @RoleName = N'User Administrator', 
                             @Description = N'', 
                             @RoleTypeID = 402, 
                             @RoleCategoryId = @Status_Role, 
                             @PartyID = @OrganizationId, 
							 @CreatedBy = @UserId,
                             @RoleID = @RoleId OUTPUT;
                        SET @RoleName = @PlatformAdminRoleValue;
                        SET @TargetProductId = 3;
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE Value = @RoleName
                              AND PartyID = @OrganizationId;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to create User Details', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to edit User Details', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to edit profile of other users', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to lock/unlock users', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to view users', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        --EXECUTE Enterprise.CreateRight 
                        --        @RoleID = @RoleId, 
                        --        @PartyId = @OrganizationId, 
                        --        @ProductId = 3, 
                        --        @RightName = 'Access to Help Center', 
                        --        @RightCategoryId = @Status_Right, 
                        --        @RightID = @RightID OUTPUT, 
                        --        @Description = '', 
                        --        @TargetProductId = @TargetProductId, 
                        --        @VisibilityStatusId = @VisibilityStatusId;
                        --SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to edit my own profile', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to edit password', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to clone users (all products)', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to manage roles and rights', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to view roles and rights', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Access to Product Learning Portal', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '';
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to view audit trail on user data', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to Resend Invite', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to Activate/Deactivate User', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Default_SideMenu_Admin', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Default_Dashboard_Admin', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to Answer Questions for CIMPL', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Access to Submit questionnaires within CIMPL', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Personally Identifiable Information (PII) in CIMPL', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Sensitive Financial Data in CIMPL', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'View CIMPL Implementation Questions', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to answer company-level questionnaires in CIMPL', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage CIMPL Templates', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;

                        --EXECUTE Enterprise.CreateRight 
                        --        @RoleID = @RoleId, 
                        --        @PartyId = @OrganizationId, 
                        --        @ProductId = 3, 
                        --        @RightName = 'Ability to manage Platform Notifications', 
                        --        @RightCategoryId = @Status_Right, 
                        --        @RightID = @RightID OUTPUT, 
                        --        @Description = '', 
                        --        @TargetProductId = @TargetProductId, 
                        --        @VisibilityStatusId = @VisibilityStatusId;
                        --SELECT @RightID;

                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Migration Tool Application';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Ability to Migrate Users', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Resident Portals';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Resident Portals Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'OneSite';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage OneSite Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Financial Suite';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Financial Suite Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Spend Management';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Spend Management Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Vendor Credentialing';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Vendor Credentialing Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Unified Amenities';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Unified Amenities Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Renters Insurance';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Renters Insurance Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Lead2Lease';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Lead2Lease Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Marketing Center';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Marketing Center Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Prospect Contact Center';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Prospect Contact Center Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'On-Site';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage On-Site Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Asset Optimization';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Asset Optimization Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'ILM Lead Management';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage ILM Lead Management Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'ILM Leasing Analytics';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage ILM Leasing Analytics Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Document Director';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Document Director Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Utility Management';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Utility Management Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Client Portal';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Client Portal Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Property Photos';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Access to Property Photos (requires Marketing Center access)', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Vendor Marketplace';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Access to Vendor Marketplace', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Portfolio Management';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Portfolio Management Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Integration Marketplace';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Integration Marketplace Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Deposit Alternative';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage Deposit Alternative Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'Payments';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Manage ClickPay Product Access', 
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID;

                        -------
                        SELECT @TargetProductId = ProductId
                        FROM Enterprise.Product
                        WHERE Name = 'L&R Conversion Utility';
                        EXECUTE Enterprise.CreateRight 
                                @RoleID = @RoleId, 
                                @PartyId = @OrganizationId, 
                                @ProductId = 3, 
                                @RightName = 'Access to Leasing & Rents Conversion Utility for OneSite users',
                                @RightCategoryId = @Status_Right, 
                                @RightID = @RightID OUTPUT, 
                                @Description = '', 
                                @TargetProductId = @TargetProductId, 
                                @VisibilityStatusId = @VisibilityStatusId;
                        SELECT @RightID; 
                        ----------------

                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'Userslist'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'SuperUser';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE RVT.Value = @PlatformAdminRoleValue
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Ability to create User Details'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'EditUser'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'SuperUser';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE RVT.Value = @PlatformAdminRoleValue
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Ability to edit User Details'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'SideMenu'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'SuperUser';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE RVT.Value = @PlatformAdminRoleValue
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Default_SideMenu_Admin'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'Dashboard'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'SuperUser';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE RVT.Value = @PlatformAdminRoleValue
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Default_Dashboard_Admin'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'RolesAndRights'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'SuperUser';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE RVT.Value = @PlatformAdminRoleValue
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Ability to manage roles and rights'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'AddUser'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'SuperUser';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE RVT.Value = @PlatformAdminRoleValue
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Ability to manage roles and rights'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'CloneUser'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'SuperUser';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE RVT.Value = @PlatformAdminRoleValue
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Ability to clone users (all products)'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                        SELECT @ActionID = ActionID
                        FROM Enterprise.ACTION
                        WHERE ObjectValue = 'ActivityLog'
                              AND ObjectType = 'ROUTE'
                              AND Description = 'SuperUSer';
                        SELECT @RoleId = RoleID
                        FROM Enterprise.Role AS R
                             INNER JOIN Enterprise.RoleValueType AS RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                        WHERE Value = @PlatformAdminRoleValue
                              AND PartyID = @OrganizationId;
                        SELECT @RightID = RightID
                        FROM Enterprise.[Right] AS R
                             INNER JOIN Enterprise.RightValueType AS RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                        WHERE Value = 'Ability to view audit trail on user data'
                              AND RoleID = @RoleId;
                        EXEC Enterprise.LinkActionToRights 
                             @ActionID = @ActionID, 
                             @RightId = @RightID, 
                             @StatusId = @Status_Right, 
                             @UserActionId = @UserActionID OUTPUT;
                END;
                SET @RoleId = NULL;
                SET @PersonaId = NULL;
                SELECT @RoleId = R.RoleID
                FROM Enterprise.Role AS R
                     INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                WHERE RVT.Value = @PlatformAdminRoleValue
                      AND R.PartyID = @OrganizationId;
                SELECT @PersonaId = P.PersonaId
                FROM Ident.UserLogin UL
                     INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
                     INNER JOIN Person.Persona P ON P.UserLoginPersonaId = ULP.UserLoginPersonaId--P.UserId = UL.UserId
                WHERE UL.LoginName = @Email;
                SELECT @PersonaId, 
                       @RoleId;
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM Enterprise.PersonaPrivilege
                    WHERE PersonaId = @PersonaId
                          AND RoleID = @RoleId
                )
                    BEGIN
                        EXEC Enterprise.LinkPersonaToRole 
                             @PersonaID = @PersonaId, 
                             @RoleID = @RoleId, 
							  @CreatedBy = @UserId,
                             @PersonaPrivilgeID = @PerPriv OUTPUT;
                END;
				
								--INSERT all properties indicator for UPFM
				IF NOT EXISTS
				(
					SELECT 1
					FROM Enterprise.PropertyMapping
					WHERE PersonaId = @PersonaId
					AND ProductId = 3
					AND PropertyId = -1
					AND ThruDate IS NULL
				)
				BEGIN
					INSERT INTO Enterprise.PropertyMapping (
						PersonaId,
						PropertyId,
						ProductId,
						FromDate,
						ThruDate
					)
					VALUES (
						@PersonaId,
						-1,
						3,
						@NOW,
						NULL
					)
				END

                UPDATE #HoldOrgs
                  SET 
                      PStatus = 1
                WHERE RowNumber = @OrgRowNum;
            END;
        -- ADD USER TO PRODUCTS THAT DO NOT REQUIRE EXISTING PRODUCT USERS
        IF
        (
            SELECT ControlValue
            FROM Enterprise.GlobalControl
            WHERE ControlName = 'IsNewBatchService'
        ) = 0
            BEGIN
                INSERT INTO Enterprise.ProductBatch
                (PersonPartyId, 
                 CreateUserPersonaId, 
                 AssignUserPersonaId, 
                 ProductId, 
                 StatusTypeId, 
                 RetryCount, 
                 InputJson, 
                 CreatedDate, 
                 ModifiedDate
                )
                       SELECT DISTINCT 
                              UL.PersonPartyId, 
                              p.PersonaId, 
                              p.PersonaId, 
                              ap.ProductId, 
                              5, 
                              0, 
                              '{"PropertyList":[],"RoleList":[],"IsAssigned":true,"CompanyId":0}', 
                              @NOW, 
                              @NOW
                       FROM Person.Persona AS p
                            INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = P.UserLoginPersonaId
                            INNER JOIN Ident.UserLogin UL ON ULP.UserLoginId = UL.UserId
                            CROSS JOIN @AssignedProductId ap
                       WHERE ULP.UserLoginId = @UserId --p.UserId = @UserId
                             AND ULP.OrganizationPartyId = @OrganizationId
                             AND p.ThruDate IS NULL
                             AND ap.ProductId NOT IN(3, 4, 8, 14, 28, 36); --Unified Platform (3), Asset Optimization (4), RealPage Accounting (8), Client Portal (14), Product Updates (28), EasyLMS (36)
        END;
            ELSE
            BEGIN
                INSERT INTO [Batch].[BatchProcessor]
                ([CorrelationId], 
                 [EditorUserPartyId], 
                 [EditorUserPersonaId], 
                 [SubjectUserPersonaId], 
                 [BatchProcessTypeId], 
                 [ProductId], 
                 [StatusTypeId], 
                 [RetryCount], 
                 [InputJSON], 
                 [CreatedDateTime], 
                 [LastRunDateTime]
                )
                       SELECT DISTINCT 
                              NEWID(), 
                              UL.PersonPartyId, 
                              p.PersonaId, 
                              p.PersonaId, 
                              1, 
                              ap.ProductId, 
                              5, 
                              0, 
                              '{"PropertyList":[],"RoleList":[],"IsAssigned":true,"CompanyId":0}', 
                              @NOW, 
                              @NOW
                       FROM Person.Persona AS p
                            INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = P.UserLoginPersonaId
                            INNER JOIN Ident.UserLogin UL ON ULP.UserLoginId = UL.UserId
                            CROSS JOIN @AssignedProductId ap
                       WHERE ULP.UserLoginId = @UserId --@UserId
                             AND ULP.OrganizationPartyId = @OrganizationId
                             AND p.ThruDate IS NULL
                             AND ap.ProductId NOT IN(3, 4, 8, 14, 28, 36); --Unified Platform (3), Asset Optimization (4), RealPage Accounting (8), Client Portal (14), Product Updates (28), EasyLMS (36)
        END;
        -- ADD USER TO PRODUCTS THAT DO NOT REQUIRE EXISTING PRODUCT USERS
        -- ADD USER TO SUPPORT TOOL LIST IF NO OTHER USERS EXIST FOR THE COMPANY
        IF 1 =
        (
            SELECT COUNT(1)
            FROM Person.Persona PE
                 INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
            WHERE ULP.OrganizationPartyId = @OrganizationId
        )
        BEGIN
            IF NOT EXISTS( SELECT TOP(1) 1 FROM Enterprise.OrganizationAdminUser WHERE OrganizationPartyId = @OrganizationId )
			BEGIN
				INSERT INTO Enterprise.OrganizationAdminUser ( OrganizationPartyId, UserLoginPersonaId ) 
					SELECT @OrganizationId, ULP.UserLoginPersonaId 
						FROM Ident.UserLogin AS UL
						INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
					WHERE UL.LoginName IN(@Email)
			END
		END;
        -- ADD USER TO SUPPORT TOOL LIST IF NO OTHER USERS EXIST FOR THE COMPANY
    END;