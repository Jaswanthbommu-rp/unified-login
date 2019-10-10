--User/RolesRights Declaration Block
DECLARE @Fname NVARCHAR(50);
DECLARE @MName NVARCHAR(50);
DECLARE @Lname NVARCHAR(50);
DECLARE @Title NVARCHAR(50);
DECLARE @Suffix NVARCHAR(50);
--DECLARE @ContactMechanismId INT;
DECLARE @ContactMechanismUsageId INT;
DECLARE @PreferredContactMenoth INT;
--DECLARE @RealpageID UNIQUEIDENTIFIER;
DECLARE @Email NVARCHAR(50);
--DECLARE @now DATETIME;
--DECLARE @orgID INT;
DECLARE @orgrpid UNIQUEIDENTIFIER;
DECLARE @pwdhash NVARCHAR(255);
DECLARE @pwdsalt NVARCHAR(255);
DECLARE @userStatus INT;
--DECLARE @roleid NVARCHAR(100);
--DECLARE @contactmechanism BIGINT;
DECLARE @PartyCMId BIGINT;
DECLARE @Personaid BIGINT;
DECLARE @userId BIGINT;
DECLARE @UserRPId UNIQUEIDENTIFIER;
DECLARE @OrgRowNum INT;
DECLARE @PerRowNum INT;
DECLARE @PerPriv INT;
DECLARE @RoleName VARCHAR(200);
--DECLARE @RightID INT;
--DECLARE @ActionID INT;
--DECLARE @Status INT;
DECLARE @UserActionID INT;
DECLARE @PersonRoleID INT;
DECLARE @Status_Role INT;
DECLARE @Status_Right INT;



--Create Org/Product Declaration Block

        DECLARE @OrganizationName VARCHAR(50)= 'Alliance'; --Put PMC Name Here
        DECLARE @BlueBookId VARCHAR(50)= '3945'; --Put BlueBookId Here
        DECLARE @ProductName1 VARCHAR(50)= 'On-Site'; --First Product to be Configured
        DECLARE @Description1 VARCHAR(100)= 'On-Site'; --If Product does not exist, use this as the description
        DECLARE @ProductName2 VARCHAR(50)= 'OneSite'; --Subsequent product to be Configured
        DECLARE @ProductName3 VARCHAR(50)= 'Resident Portal'; --Subsequent product to be Configured
        DECLARE @ProductName4 VARCHAR(50)= 'Websites & Syndication'; --Subsequent product to be Configured
        DECLARE @ProductName5 VARCHAR(50)= 'Lead2Lease'; --Subsequent product to be Configured



        DECLARE @OrganizationId INT= NULL;
        --DECLARE @ConfigurationId INT= NULL;
       -- DECLARE @ProductId INT= NULL;

SELECT @ServerName = @@ServerName;
SET @DBName = 'Identity';
IF((@ServerName = 'RCDUSODBSQL001'
    OR @ServerName = 'RCTUSODBSQL001')
   AND @DBName = 'Identity')
    BEGIN
        

    /*ASSIGNMENT BLOCK*/


        SET @fname = 'AllianceSU01';
        SET @MName = '';
        SET @Lname = '';
        SET @Title = '';
        SET @Suffix = '';
        SET @Email = 'AllianceSU01@test.com';
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

--SETUP Password Policy

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
        IF
(
    SELECT 1
    FROM Enterprise.Product
    WHERE [Name] = @ProductName1
) IS NULL
AND @ProductName1 = 'On-Site' -- This code is not extensible to other clients
            BEGIN
                --DECLARE @ProductSettingId INT;
                --DECLARE @ProductSettingTypeId INT;
                EXEC Enterprise.CreateProduct
                     NULL,
                     NULL,
                     @ProductName1,
                     @Description1,
                     NULL;
                SELECT @ProductID = ProductId
                FROM Enterprise.Product
                WHERE [Name] = @ProductName1;
                EXEC Enterprise.CreateProductConfiguration
                     @ConfigurationId OUTPUT;
                SELECT @ProductSettingTypeId = ProductSettingTypeId
                FROM Enterprise.ProductSettingType
                WHERE [Name] = 'ClientId';
                EXEC Enterprise.CreateProductSetting
                     @ProductId,
                     @ProductSettingTypeId,
                     '3431c19ab693ead1bfe2a138e2a220f3d96c6e24d3c4547236c9f7b52cb0d4e5',
                     @Now,
                     NULL,
                     @ProductSettingId OUTPUT;
                SELECT @ProductSettingTypeId = ProductSettingTypeId
                FROM Enterprise.ProductSettingType
                WHERE [Name] = 'ClassName';
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId,
                     @ProductSettingId,
                     @Now,
                     NULL;
                EXEC Enterprise.CreateProductSetting
                     @ProductId,
                     @ProductSettingTypeId,
                     'on-site',
                     @Now,
                     NULL,
                     @ProductSettingId OUTPUT;
                SELECT @ProductSettingTypeId = ProductSettingTypeId
                FROM Enterprise.ProductSettingType
                WHERE [Name] = 'ProductUrl';
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId,
                     @ProductSettingId,
                     @Now,
                     NULL;
                EXEC Enterprise.CreateProductSetting
                     @ProductId,
                     @ProductSettingTypeId,
                     '/product/onsite',
                     @Now,
                     NULL,
                     @ProductSettingId OUTPUT;
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId,
                     @ProductSettingId,
                     @Now,
                     NULL;
                SELECT @ProductSettingTypeId = ProductSettingTypeId
                FROM Enterprise.ProductSettingType
                WHERE [Name] = 'TitleId';
                EXEC Enterprise.CreateProductSetting
                     @ProductId,
                     @ProductSettingTypeId,
                     'On-Site',
                     @Now,
                     NULL,
                     @ProductSettingId OUTPUT;
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId,
                     @ProductSettingId,
                     @Now,
                     NULL;
                SELECT @ProductSettingTypeId = ProductSettingTypeId
                FROM Enterprise.ProductSettingType
                WHERE [Name] = 'TitleUniqueId';
                EXEC Enterprise.CreateProductSetting
                     @ProductId,
                     @ProductSettingTypeId,
                     '5E10D43B-AF05-48EF-B365-2084277C106E',
                     @Now,
                     NULL,
                     @ProductSettingId OUTPUT;
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId,
                     @ProductSettingId,
                     @Now,
                     NULL;
                SELECT @ProductSettingTypeId = ProductSettingTypeId
                FROM Enterprise.ProductSettingType
                WHERE [Name] = 'IsNewTab';
                EXEC Enterprise.CreateProductSetting
                     @ProductId,
                     @ProductSettingTypeId,
                     '1',
                     @Now,
                     NULL,
                     @ProductSettingId OUTPUT;
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId,
                     @ProductSettingId,
                     @Now,
                     NULL;
                SELECT @ProductSettingTypeId = ProductSettingTypeId
                FROM Enterprise.ProductSettingType
                WHERE [Name] = 'MetatagUniqueId';
                EXEC Enterprise.CreateProductSetting
                     @ProductId,
                     @ProductSettingTypeId,
                     'On-Site',
                     @Now,
                     NULL,
                     @ProductSettingId OUTPUT;
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId,
                     @ProductSettingId,
                     @Now,
                     NULL;
                SELECT @ProductSettingTypeId = ProductSettingTypeId
                FROM Enterprise.ProductSettingType
                WHERE [Name] = 'IsFavorite';
                EXEC Enterprise.CreateProductSetting
                     @ProductId,
                     @ProductSettingTypeId,
                     '1',
                     @Now,
                     NULL,
                     @ProductSettingId OUTPUT;
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId,
                     @ProductSettingId,
                     @Now,
                     NULL;
                SELECT @ProductSettingTypeId = ProductSettingTypeId
                FROM Enterprise.ProductSettingType
                WHERE [Name] = 'LearnMore';
                EXEC Enterprise.CreateProductSetting
                     @ProductId,
                     @ProductSettingTypeId,
                     'https://www.realpage.com/',
                     @Now,
                     NULL,
                     @ProductSettingId OUTPUT;
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId,
                     @ProductSettingId,
                     @Now,
                     NULL;
                SELECT @ProductSettingTypeId = ProductSettingTypeId
                FROM Enterprise.ProductSettingType
                WHERE [Name] = 'ApiEndPoint';
                EXEC Enterprise.CreateProductSetting
                     @ProductId,
                     @ProductSettingTypeId,
                     'https://staging9.on-site.com/api/greenbook',
                     @Now,
                     NULL,
                     @ProductSettingId OUTPUT;
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId,
                     @ProductSettingId,
                     @Now,
                     NULL;
                SELECT @ProductSettingTypeId = ProductSettingTypeId
                FROM Enterprise.ProductSettingType
                WHERE [Name] = 'ProductStatus';
                EXEC Enterprise.CreateProductSetting
                     @ProductId,
                     @ProductSettingTypeId,
                     '8',
                     @Now,
                     NULL,
                     @ProductSettingId OUTPUT;
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId,
                     @ProductSettingId,
                     @Now,
                     NULL;
                SELECT @ProductSettingTypeId = ProductSettingTypeId
                FROM Enterprise.ProductSettingType
                WHERE [Name] = 'ApiSecret';
                EXEC Enterprise.CreateProductSetting
                     @ProductId,
                     @ProductSettingTypeId,
                     'f3865f8b7c1a2177b0147f2ab1bb3ccfee25f716f883eb341d700986a61d4048',
                     @Now,
                     NULL,
                     @ProductSettingId OUTPUT;
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId,
                     @ProductSettingId,
                     @Now,
                     NULL;
                SELECT @ProductSettingTypeId = ProductSettingTypeId
                FROM Enterprise.ProductSettingType
                WHERE [Name] = 'TokenURL';
                EXEC Enterprise.CreateProductSetting
                     @ProductId,
                     @ProductSettingTypeId,
                     'https://staging9.on-site.com/oauth/token',
                     @Now,
                     NULL,
                     @ProductSettingId OUTPUT;
                EXEC Enterprise.LinkProductSettingToConfiguration
                     @ConfigurationId,
                     @ProductSettingId,
                     @Now,
                     NULL;
            END;

/* END CREATE PRODUCT */

        SELECT @COnfigurationId = MAX(ConfigurationId)
        FROM Enterprise.Configuration;

/* LINK ORGANIZATION TO PRODUCT */

        BEGIN
            SELECT @ProductId = ProductId
            FROM Enterprise.Product
            WHERE [Name] = @ProductName1;

	/* If there is a proc to do this, I can't find it */

            INSERT INTO Enterprise.OrganizationProduct
(PartyId,
 ConfigurationId,
 ProductId,
 FromDate
)
                   SELECT TOP 1 @OrganizationId,
                                ConfigurationId,
                                ProductId,
                                @Now
                   FROM Enterprise.GlobalProductConfiguration
                   WHERE ThruDate IS NULL
                         AND ProductId = @ProductId;
            SELECT @ProductId = ProductId
            FROM Enterprise.Product
            WHERE [Name] = @ProductName2;
            INSERT INTO Enterprise.OrganizationProduct
(PartyId,
 ConfigurationId,
 ProductId,
 FromDate
)
                   SELECT TOP 1 @OrganizationId,
                                ConfigurationId,
                                ProductId,
                                @Now
                   FROM Enterprise.GlobalProductConfiguration
                   WHERE ThruDate IS NULL
                         AND ProductId = @ProductId;
            SELECT @ProductId = ProductId
            FROM Enterprise.Product
            WHERE [Name] = @ProductName3;
            INSERT INTO Enterprise.OrganizationProduct
(PartyId,
 ConfigurationId,
 ProductId,
 FromDate
)
                   SELECT TOP 1 @OrganizationId,
                                ConfigurationId,
                                ProductId,
                                @Now
                   FROM Enterprise.GlobalProductConfiguration
                   WHERE ThruDate IS NULL
                         AND ProductId = @ProductId;
            SELECT @ProductId = ProductId
            FROM Enterprise.Product
            WHERE [Name] = @ProductName4;
            INSERT INTO Enterprise.OrganizationProduct
(PartyId,
 ConfigurationId,
 ProductId,
 FromDate
)
                   SELECT TOP 1 @OrganizationId,
                                ConfigurationId,
                                ProductId,
                                @Now
                   FROM Enterprise.GlobalProductConfiguration
                   WHERE ThruDate IS NULL
                         AND ProductId = @ProductId;
            SELECT @ProductId = ProductId
            FROM Enterprise.Product
            WHERE [Name] = @ProductName5;
            INSERT INTO Enterprise.OrganizationProduct
(PartyId,
 ConfigurationId,
 ProductId,
 FromDate
)
                   SELECT TOP 1 @OrganizationId,
                                ConfigurationId,
                                ProductId,
                                @Now
                   FROM Enterprise.GlobalProductConfiguration
                   WHERE ThruDate IS NULL
                         AND ProductId = @ProductId;
        END;

--Enable User for Organization
/*CREATE PERSON*/

        SELECT @OrgId = PartyId
        FROM Enterprise.Organization
        WHERE Name = 'Alliance';
        SELECT @OrgRPid = realpageid
        FROM enterprise.party
        WHERE PartyId = @orgID;
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
DELETE FROM #HoldPersona;
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

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='36'