--User/RolesRights Declaration Block
SELECT @ServerName = @@ServerName;
SET @DBName = 'Identity';
IF((@ServerName = 'RCDUSODBSQL001'
    OR @ServerName = 'RCTUSODBSQL001')
   AND @DBName = 'Identity')
    BEGIN
        
SET @ContactMechanismUsageId = NULL;
SET @PreferredContactMenoth = NULL;
SET @RealpageID = NULL;
SET @Email = NULL;
SET @now = NULL;
SET @orgID = NULL;
SET @orgrpid = NULL;
SET @pwdhash = NULL;
SET @pwdsalt = NULL;
SET @userStatus = NULL;
SET @roleid = NULL;
SET @contactmechanism = NULL;
SET @PartyCMId = NULL;
SET @Personaid = NULL;
SET @userId = NULL;
SET @UserRPId = NULL;
SET @OrgRowNum = NULL;
SET @PerRowNum = NULL;
SET @PerPriv = NULL;
SET @RoleName = NULL;
SET @RightID = NULL;
SET @ActionID = NULL;
SET @Status = NULL;
SET @UserActionID = NULL;
SET @PersonRoleID = NULL;
SET @Status_Role = NULL;
SET @Status_Right = NULL;
SET @ContactMechanismId = NULL;
--Create Org/Product Declaration Block

SET @OrganizationName = 'RealPage Employee'; --Put PMC Name Here
SET @BlueBookId = '-1'; --Put BlueBookId Here

DELETE FROM @ProductList;

--INSERT INTO @ProductList ( ProductID ) VALUES ( 1 )  -- OneSite
--INSERT INTO @ProductList ( ProductID ) VALUES ( 6 )  -- Lead2Lease
--INSERT INTO @ProductList ( ProductID ) VALUES ( 8 )  -- Accounting
--INSERT INTO @ProductList ( ProductID ) VALUES ( 9 )  -- Websites & Syncication (MarketingCenter)
--INSERT INTO @ProductList ( ProductID ) VALUES ( 10 ) -- Prospect Contact Center
--INSERT INTO @ProductList ( ProductID ) VALUES ( 13 ) -- Spend Management (Ops)
--INSERT INTO @ProductList ( ProductID ) VALUES ( 14 ) -- ClientPortal
--INSERT INTO @ProductList ( ProductID ) VALUES ( 15 ) -- Renters Insurance
--INSERT INTO @ProductList ( ProductID ) VALUES ( 16 ) -- Vendor Services (Compliance Depot)
--INSERT INTO @ProductList ( ProductID ) VALUES ( 17 ) -- Resident Portal (ActiveBuilding)
--INSERT INTO @ProductList ( ProductID ) VALUES ( 18 ) -- Utility Management (RUM)
--INSERT INTO @ProductList ( ProductID ) VALUES ( 19 ) -- Product Learning Portal
--INSERT INTO @ProductList ( ProductID ) VALUES ( 22 ) -- OmniChannel
--INSERT INTO @ProductList ( ProductID ) VALUES ( 23 ) -- On-Site
INSERT INTO @ProductList(ProductID)
VALUES(24); -- BlackBook (Internal only!)

SET @OrganizationId = NULL;
--DECLARE @ConfigurationId INT= NULL;
-- DECLARE @ProductId INT= NULL;
/*ASSIGNMENT BLOCK*/

SET @fname = 'RealPageAdmin';
SET @MName = '';
SET @Lname = '';
SET @Title = '';
SET @Suffix = '';
SET @Email = 'RealPageAd@test.com';
SET @now = GETUTCDATE();


/* CREATE ORGANIZATION */

SELECT @OrganizationId = PartyID
FROM Enterprise.Organization
WHERE [Name] = @OrganizationName;
IF @OrganizationId IS NULL
    BEGIN
        EXEC Enterprise.CreateOrganization
             NULL,
             @OrganizationName;
    END;
SELECT @OrganizationId = PartyID
FROM Enterprise.Organization
WHERE [Name] = @OrganizationName;
SELECT @OrganizationName,
       @OrganizationID;
IF
(
    SELECT 1
    FROM Enterprise.DataImportMapping
    WHERE PartyId = @OrganizationId
) IS NULL
    BEGIN
        EXEC Enterprise.MapBlueBookIdtoPartyId
             @BlueBookId,
             @OrganizationId;
    END;

/* END CREATE ORGANIZATION */

--SETUP Password Policy using RealPage company as template

IF NOT EXISTS
(
    SELECT 1
    FROM Ident.PasswordPolicy
    WHERE PartyId = @OrganizationId
)
    BEGIN
        INSERT INTO [Ident].[PasswordPolicy]
([PartyId],
 [MinimumLength],
 [MaximumLength],
 [MinimumLowercase],
 [MinimumUppercase],
 [MinimumNumeric],
 [MinimumSpecialCharacter],
 [AllowUsersToChangeOwnPassword],
 [EnablePasswordExpiration],
 [PasswordExpirationPeriodInDays],
 [PreventPasswordReuse],
 [NumberOfPasswordsToRemember],
 [UserId]
)
               SELECT @OrganizationId,
                      [MinimumLength],
                      [MaximumLength],
                      [MinimumLowercase],
                      [MinimumUppercase],
                      [MinimumNumeric],
                      [MinimumSpecialCharacter],
                      [AllowUsersToChangeOwnPassword],
                      [EnablePasswordExpiration],
                      [PasswordExpirationPeriodInDays],
                      [PreventPasswordReuse],
                      [NumberOfPasswordsToRemember],
                      3
               FROM [Ident].[PasswordPolicy]
               WHERE PartyId = 3;
    END;

/* LINK ORGANIZATION TO PRODUCT */

BEGIN
    INSERT INTO Enterprise.OrganizationProduct
(PartyId,
 ConfigurationId,
 ProductId,
 FromDate
)
           SELECT @OrganizationId,
                  GPC.ConfigurationId,
                  GPC.ProductId,
                  @Now
           FROM Enterprise.GlobalProductConfiguration GPC
                INNER JOIN @ProductList PL ON GPC.ProductId = PL.ProductID
                                              AND GPC.ThruDate IS NULL
                LEFT OUTER JOIN Enterprise.OrganizationProduct OP ON OP.PartyId = @OrganizationId
                                                                     AND OP.ProductId = GPC.ProductId
           WHERE GPC.ThruDate IS NULL
                 AND (OP.ConfigurationId IS NULL
                      OR (OP.ConfigurationId IS NOT NULL
                          AND OP.ThruDate IS NOT NULL));
            

--Enable User for Organization
/*CREATE PERSON*/

    IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM Ident.UserLogin
    WHERE LoginName = @Email
)
        BEGIN
            SELECT @OrgId = PartyId
            FROM Enterprise.Organization
            WHERE Name = @OrganizationName;
            SELECT @OrgRPid = realpageid
            FROM enterprise.party
            WHERE PartyId = @OrgId;
            EXEC Person.CreatePerson
                 @FirstName = @Fname,
                 @MiddleName = @MName,
                 @LastName = @Lname,
                 @Title = @Title,
                 @Suffix = @Suffix,
                 @PreferredContactMethodId = 0,
                 @RealPageId = @RealpageID OUTPUT;
            EXEC Ident.CreateUserLogin
                 @RealPageId = @RealpageID,
                 @LoginName = @Email,
                 @FromDate = @now,
                 @ThruDate = '9999-12-31 00:00:00';
            SELECT @pwdhash = Passwordhash,
                   @PwdSalt = PasswordSalt
            FROM ident.userlogin
            WHERE loginname = 'james@test.com';
            SELECT @userStatus = StatusTypeId
            FROM enterprise.statustype
            WHERE Enterprise.statustype.Name = 'Pending';
            SELECT @UserId = UserId
            FROM Ident.UserLogin
            WHERE LoginName = @Email;
            EXEC Person.CreatePersona
                 @personRealPageId = @RealpageID,
                 @organizationRealPageId = @orgrpid,
                 @personaTypeId = 1,
                 @personaEnvironmentTypeId = 1,
                 @UserId = @UserId,
                 @fromDate = @now,
                 @thruDate = NULL,
                 @personaId = NULL;
            SELECT @Personaid = p.PersonaId
            FROM Person.Persona p
                 INNER JOIN Enterprise.Party pp ON p.PersonPartyId = pp.PartyId
            WHERE pp.RealPageId = @RealpageID;
            EXEC Person.UpdatePersona
                 @personaId = @Personaid,
                 @personaTypeId = 3,
                 @personaEnvironmentTypeId = 1;
            EXEC Ident.LinkIdentityProviderToUserLogin
                 @PersonaId = @Personaid,
                 @UserId = @userId,
                 @ContactMechanismID = 46;
            UPDATE Ident.UserLogin
              SET
                  Passwordhash = @pwdhash,
                  PasswordSalt = @pwdsalt
            FROM ident.userlogin
            WHERE UserId = @UserId;
            EXEC Ident.UpdateUserStatus
                 @RealPageId = @RealpageID,
                 @StatusTypeId = @userStatus,
                 @FromDate = @now,
                 @ThruDate = '9999-12-31 00:00:00';
            SELECT @roleid = PartyRoleTypeId
            FROM enterprise.roletype
            WHERE name = 'SuperUser';
            EXEC Enterprise.CreatePartyRoleByRealPageId
                 @RealPageId = @RealpageID,
                 @roleTypeId = @roleid;
            EXEC Person.LinkPersonToOrganization
                 @personRealPageId = @RealpageID,
                 @organizationRealPageId = @orgrpid,
                 @roleTypeIdFrom = 328,
                 @roleTypeIdTo = 203;
            EXEC Person.LinkPersonToOrganization
                 @personRealPageId = @RealpageID,
                 @organizationRealPageId = @orgrpid,
                 @roleTypeIdFrom = @roleid,
                 @roleTypeIdTo = 205;
            EXEC Enterprise.ListContactMechanismUsageType
                 @ContactMechanismUsageTypeName = N'Email Notification';
            EXEC Person.CreateContactMechanism
                 @ContactMechanismId = @contactmechanism OUTPUT;
            EXEC Person.LinkContactMechanismToParty
                 @RealPageId = @RealpageID,
                 @PartyContactMechanismId = 0,
                 @ContactMechanismId = @contactmechanism,
                 @FromDate = @now,
                 @ThruDate = '9999-12-31 23:59:59.997';
            SELECT @PartyCMId = PartyContactMechanismId
            FROM Enterprise.PartyContactMechanism a
                 INNER JOIN enterprise.party p ON a.PartyId = p.PartyId
            WHERE RealPageId = @RealPageId;
            EXEC Person.LinkUsageTypeToPartyContactMechanism
                 @PartyContactMechanismId = @PartyCMId,
                 @ContactMechanismUsageTypeId = 301;
            SELECT @ContactMechanismUsageId = ContactMechanismUsageID
            FROM Enterprise.ContactMechanismUsage
            WHERE PartyContactMechanismID = @PartyCMId;
            EXEC Person.CreateElectronicAddress
                 @ContactMechanismId = @contactmechanism,
                 @ElectronicAddressString = @Email,
                 @ElectronicAddressType = N'Email';
            SELECT @ContactMechanismId = ContactMechanismId
            FROM Ident.IdentityProviderType I
                 INNER JOIN Ident.UserLogin U ON U.IdentityProviderTypeId = I.IdentityProviderTypeId
            WHERE U.UserId = @UserId;
            EXEC Ident.LinkIdentityProviderToUserLogin
                 @PersonaId = @PersonaId,
                 @UserId = @UserId,
                 @ContactMechanismID = 46;

/*Create USER ROLE for all the existing organizations*/

/*Create Basic End User for all the existing organizations*/

           DELETE FROM #HoldOrgs
		 DELETE FROM #HoldPersona;
            SELECT @Status_Right = ST.StatusTypeId
            FROM Enterprise.StatusTypeCategoryType STCT
                 JOIN Enterprise.StatusTypeCategory STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
                 JOIN Enterprise.StatusTypeCategoryClassification STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
                 JOIN Enterprise.StatusType ST ON ST.StatusTypeId = STCC.StatusTypeId
            WHERE STC.Name = 'Right Type'
                  AND ST.Name = 'Default';
            SELECT @Status_Role = ST.StatusTypeId
            FROM Enterprise.StatusTypeCategoryType STCT
                 JOIN Enterprise.StatusTypeCategory STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
                 JOIN Enterprise.StatusTypeCategoryClassification STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
                 JOIN Enterprise.StatusType ST ON ST.StatusTypeId = STCC.StatusTypeId
            WHERE STC.Name = 'Role Type'
                  AND ST.Name = 'Default';

            INSERT INTO #HoldOrgs(OrganizationPartyID)
            VALUES(@OrgId);
--SELECT * FROM #HoldOrgs

            WHILE EXISTS
(
    SELECT 1
    FROM #HoldOrgs
    WHERE PStatus = 0
)
                BEGIN
                    SELECT TOP 1 @OrgID = OrganizationPartyID,
                                 @OrgRowNum = RowNumber
                    FROM #HoldOrgs
                    WHERE PStatus = 0;
                   
                    		 INSERT INTO #HoldPersona
            SELECT p.personaID,
                   pr.RoleTypeIdFrom PartyRoleID,
                   0 PStatus
            FROM person.persona p
                 INNER JOIN enterprise.partyrelationship pr ON pr.partyidfrom = p.personpartyid
            WHERE p.organizationPartyid = @OrgId
                  AND pr.RoleTypeIdFrom IN(400, 401, 402, 403, 404);
                    IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Role R
         INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
    WHERE value = 'Basic End User'
          AND PartyID = @OrgID
)
                        BEGIN
                            PRINT 'USER RIGHTS ===============';
                            EXEC [Enterprise].[CreateRole]
                                 @RoleName = N'Basic End User',
                                 @Description = N'',
                                 @RoleTypeID = 400,
                                 @PartyID = @OrgID,
                                 @RoleCategoryId = @Status_Role,
                                 @RoleID = @RoleID OUTPUT;
                            PRINT 'USER RIGHTS ===============';
                            SET @RoleName = 'Basic End User';
                            SELECT @RoleID = RoleId
                            FROM Enterprise.Role R
                                 INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                            WHERE value = @RoleName
                                  AND PartyId = @OrgID;
                            PRINT 'R1';
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Ability to edit my own profile',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            PRINT 'R2';
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Access to Client Portal',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            PRINT 'R3';
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Access to Product Learning Portal',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            PRINT 'R4';
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Default_SideMenu_Users',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            PRINT 'R5';
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
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
                                  AND Description = 'User';
                            SELECT @RoleID = RoleID
                            FROM Enterprise.Role R
                                 INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                            WHERE value = 'Basic End User'
                                  AND partyid = @OrgId;
                            SELECT @RightID = RightId
                            FROM Enterprise.[Right] R
                                 INNER JOIN Enterprise.RightValueType RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                            WHERE Value = 'Default_SideMenu_Users'
                                  AND RoleId = @RoleID;
                            EXEC [Enterprise].[LinkActionToRights]
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
                            FROM Enterprise.Role R
                                 INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                            WHERE value = 'Basic End User'
                                  AND partyid = @OrgId;
                            SELECT @RightID = RightId
                            FROM Enterprise.[Right] R
                                 INNER JOIN Enterprise.RightValueType RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                            WHERE Value = 'Default_Dashboard_Users'
                                  AND RoleId = @RoleID;
                            EXEC [Enterprise].[LinkActionToRights]
                                 @ActionID = @ActionID,
                                 @RightId = @RightId,
                                 @StatusId = @Status_Right,
                                 @UserActionId = @UserActionId OUTPUT;
                        END;
                    IF EXISTS
(
    SELECT 1
    FROM Enterprise.Role R
         INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
    WHERE value = 'Basic End User'
          AND PartyID = @OrgID
)
                        BEGIN
                            SELECT @RoleID = RoleID
                            FROM Enterprise.Role R
                                 INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                            WHERE value = 'Basic End User'
                                  AND PartyID = @OrgID;
                        END;
                    IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Role R
         INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
    WHERE value = 'User Administrator'
          AND PartyID = @OrgID
)
                        BEGIN
                            PRINT 'SUPERUSER RIGHTS ===============';
                            EXEC [Enterprise].[CreateRole]
                                 @RoleName = N'User Administrator',
                                 @Description = N'',
                                 @RoleTypeID = 402,
                                 @RoleCategoryId = @Status_Role,
                                 @PartyID = @OrgID,
                                 @RoleID = @RoleID OUTPUT;
                            PRINT 'SUPERUSER RIGHTS ===============';
                            SET @RoleName = 'User Administrator';
                            SELECT @RoleID = RoleId
                            FROM Enterprise.Role R
                                 INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                            WHERE value = @RoleName
                                  AND PartyId = @OrgID;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Create User',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Edit User',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Edit Profile of Others',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Lock/Unlock User',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'View users',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Edit Profile',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Edit Password',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Clone User',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Manage Roles & Rights',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'View Roles & Rights',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Access to Client Portal',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Access to Product Learning Portal',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Access to Leasing & Rents Conversion Tool',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Access to Identity Provider Configuration Page',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = 'Defaults to Internal Only until specified otherwise.';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Access to Green Book Migration Tool',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Access to Amenities Tool',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Access to Property Hierarchy Tool',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Access to Employee Management',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'View Audit Trail on User Data',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'View Audit Trail on Profile Data',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Impersonate a User',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'See All RealPage Products',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Default_SideMenu_Admin',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            EXECUTE Enterprise.CreateRight
                                    @RoleId = @RoleId,
                                    @PartyId = @orgId,
                                    @ProductId = 3,
                                    @RightName = 'Default_Dashboard_Admin',
                                    @RightCategoryId = @Status_Right,
                                    @RightID = @RightID OUTPUT,
                                    @Description = '';
                            SELECT @RightId;
                            SELECT @ActionID = ActionID
                            FROM Enterprise.ACTION
                            WHERE ObjectValue = 'Userslist'
                                  AND ObjectType = 'ROUTE'
                                  AND Description = 'SuperUser';
                            SELECT @RoleID = RoleID
                            FROM Enterprise.Role R
                                 INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                            WHERE RVT.value = 'User Administrator'
                                  AND PartyId = @OrgId;
                            SELECT @RightID = RightId
                            FROM Enterprise.[Right] R
                                 INNER JOIN Enterprise.RightValueType RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                            WHERE Value = 'Create User'
                                  AND RoleId = @RoleID;
                            EXEC [Enterprise].[LinkActionToRights]
                                 @ActionID = @ActionID,
                                 @RightId = @RightId,
                                 @StatusId = @Status_Right,
                                 @UserActionId = @UserActionId OUTPUT;
                            SELECT @ActionID = ActionID
                            FROM Enterprise.ACTION
                            WHERE ObjectValue = 'EditUser'
                                  AND ObjectType = 'ROUTE'
                                  AND Description = 'SuperUser';
                            SELECT @RoleID = RoleID
                            FROM Enterprise.Role R
                                 INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                            WHERE RVT.value = 'User Administrator'
                                  AND PartyId = @OrgId;
                            SELECT @RightID = RightId
                            FROM Enterprise.[Right] R
                                 INNER JOIN Enterprise.RightValueType RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                            WHERE Value = 'Edit User'
                                  AND RoleId = @RoleID;
                            EXEC [Enterprise].[LinkActionToRights]
                                 @ActionID = @ActionID,
                                 @RightId = @RightId,
                                 @StatusId = @Status_Right,
                                 @UserActionId = @UserActionId OUTPUT;
                            SELECT @ActionID = ActionID
                            FROM Enterprise.ACTION
                            WHERE ObjectValue = 'SideMenu'
                                  AND ObjectType = 'ROUTE'
                                  AND Description = 'SuperUser';
                            SELECT @RoleID = RoleID
                            FROM Enterprise.Role R
                                 INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                            WHERE RVT.value = 'User Administrator'
                                  AND PartyId = @OrgId;
                            SELECT @RightID = RightId
                            FROM Enterprise.[Right] R
                                 INNER JOIN Enterprise.RightValueType RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                            WHERE Value = 'Default_SideMenu_Admin'
                                  AND RoleId = @RoleID;
                            EXEC [Enterprise].[LinkActionToRights]
                                 @ActionID = @ActionID,
                                 @RightId = @RightId,
                                 @StatusId = @Status_Right,
                                 @UserActionId = @UserActionId OUTPUT;
                            SELECT @ActionID = ActionID
                            FROM Enterprise.ACTION
                            WHERE ObjectValue = 'Dashboard'
                                  AND ObjectType = 'ROUTE'
                                  AND Description = 'SuperUser';
                            SELECT @RoleID = RoleID
                            FROM Enterprise.Role R
                                 INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                            WHERE RVT.value = 'User Administrator'
                                  AND PartyId = @OrgId;
                            SELECT @RightID = RightId
                            FROM Enterprise.[Right] R
                                 INNER JOIN Enterprise.RightValueType RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                            WHERE Value = 'Default_Dashboard_Admin'
                                  AND RoleId = @RoleID;
                            EXEC [Enterprise].[LinkActionToRights]
                                 @ActionID = @ActionID,
                                 @RightId = @RightId,
                                 @StatusId = @Status_Right,
                                 @UserActionId = @UserActionId OUTPUT;
                            SELECT @ActionID = ActionID
                            FROM Enterprise.ACTION
                            WHERE ObjectValue = 'RolesAndRights'
                                  AND ObjectType = 'ROUTE'
                                  AND Description = 'SuperUser';
                            SELECT @RoleID = RoleID
                            FROM Enterprise.Role R
                                 INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                            WHERE RVT.value = 'User Administrator'
                                  AND PartyId = @OrgId;
                            SELECT @RightID = RightId
                            FROM Enterprise.[Right] R
                                 INNER JOIN Enterprise.RightValueType RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                            WHERE Value = 'Manage Roles & Rights'
                                  AND RoleId = @RoleID;
                            EXEC [Enterprise].[LinkActionToRights]
                                 @ActionID = @ActionID,
                                 @RightId = @RightId,
                                 @StatusId = @Status_Right,
                                 @UserActionId = @UserActionId OUTPUT;
                            SELECT @ActionID = ActionID
                            FROM Enterprise.ACTION
                            WHERE ObjectValue = 'AddUser'
                                  AND ObjectType = 'ROUTE'
                                  AND Description = 'SuperUser';
                            SELECT @RoleID = RoleID
                            FROM Enterprise.Role R
                                 INNER JOIN Enterprise.RoleValueType RVT ON RVT.RoleValueTypeId = R.RoleValueTypeId
                            WHERE RVT.value = 'User Administrator'
                                  AND PartyId = @OrgId;
                            SELECT @RightID = RightId
                            FROM Enterprise.[Right] R
                                 INNER JOIN Enterprise.RightValueType RVT ON RVT.RightValueTypeId = R.RightValueTypeId
                            WHERE Value = 'Manage Roles & Rights'
                                  AND RoleId = @RoleID;
                            EXEC [Enterprise].[LinkActionToRights]
                                 @ActionID = @ActionID,
                                 @RightId = @RightId,
                                 @StatusId = @Status_Right,
                                 @UserActionId = @UserActionId OUTPUT;
                        END;
                    WHILE EXISTS
(
    SELECT 1
    FROM #HoldPersona
    WHERE PStatus = 0
)
                        BEGIN
                            SELECT TOP 1 @PersonaID = PersonaID,
                                         @PersonRoleID = PartyRoleID
                            FROM #HoldPersona
                            WHERE PStatus = 0;
                            SELECT @RoleName = CASE
                                                   WHEN @PersonRoleID IN(402)
                                                   THEN 'User Administrator'
                                                   ELSE 'Basic End User'
                                               END;
                            SELECT @RoleID = R.RoleID
                            FROM Enterprise.Role R
                                 INNER JOIN Enterprise.RoleType RT ON R.RoleTypeId = RT.PartyRoleTypeId
                            WHERE RT.name = @RoleName
                                  AND R.PartyID = @OrgID;
                            IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.PersonaPrivilege
    WHERE PersonaID = @PersonaID
          AND RoleID = @RoleID
)
                                BEGIN
                                    EXEC [Enterprise].[LinkPersonaToRole]
                                         @PersonaID = @PersonaId,
                                         @RoleID = @RoleId,
                                         @PersonaPrivilgeID = @PerPriv OUTPUT;
                        --EXEC [Enterprise].[LinkPersonaToRole] @PersonaID = '+CONVERT(VARCHAR, @PersonaId)+', @RoleID = '+CONVERT(VARCHAR, @RoleId)+', @PersonaPrivilgeID = @PerPriv OUTPUT;
                                END;
                            UPDATE #HoldPersona
                              SET
                                  PStatus = 1
                            WHERE PersonaID = @PersonaID;
                        END;
                    UPDATE #HoldOrgs
                      SET
                          PStatus = 1
                    WHERE RowNumber = @OrgRowNum;
                END;
        END;
END;
END
EXEC sys.sp_updateextendedproperty
     @name = N'Build',
     @value = '40';