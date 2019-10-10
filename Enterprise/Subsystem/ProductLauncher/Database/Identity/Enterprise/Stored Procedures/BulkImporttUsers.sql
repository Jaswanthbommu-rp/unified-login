CREATE PROCEDURE [Enterprise].[BulkImporttUsers]
(
    @UserDetails USERDETAILS READONLY,
    @OrganizationName NVARCHAR(150)
)
AS
BEGIN
--    DECLARE @FirstName NVARCHAR(200);
--    DECLARE @MiddleName NVARCHAR(100);
--    DECLARE @LastName NVARCHAR(200);
--    DECLARE @UserType NVARCHAR(200);
--    DECLARE @ThirdPartyIDP NVARCHAR(10);
--    DECLARE @LoginName NVARCHAR(200);
--    DECLARE @NotificationEmail NVARCHAR(200);
--    DECLARE @TemporaryPassword NVARCHAR(MAX);
--    DECLARE @UserEffectiveDate DATETIME;
--    DECLARE @UserExprirationDate DATETIME;
--    DECLARE @Phone NVARCHAR(20);
--    DECLARE @PhoneType NVARCHAR(100);
--    DECLARE @PreferredContactMethod NVARCHAR(200);
--    DECLARE @Title NVARCHAR(200);
--    DECLARE @OrganizationId INT;
--    DECLARE @CompanyJobTitle NVARCHAR(200);
--    DECLARE @CompanyJobTitleId INT;
--    DECLARE @Pwdhash NVARCHAR(510) = NULL;
--    DECLARE @PwdSalt NVARCHAR(510) = NULL;
--    DECLARE @Id INT;
--    DECLARE @UserRealPageId UNIQUEIDENTIFIER;
--    DECLARE @OrganizationRealPageId UNIQUEIDENTIFIER;
--    DECLARE @OrganizationIDPCMId INT;
--    DECLARE @UserStatus INT;
--    DECLARE @UserId BIGINT;
--    DECLARE @PersonaId BIGINT;
--    DECLARE @RoleId INT;
--    DECLARE @ContactMechanismId BIGINT;
--    DECLARE @PartyContactMechanismId BIGINT;
--    DECLARE @ContactMechanismUsageId BIGINT;
--    DECLARE @PartyRoleTypeId INT;
--    DECLARE @PersonaTypeId INT;
--    DECLARE @PerPriv INT;
--    DECLARE @RoleName VARCHAR(200);
--	DECLARE @StatusThruDate DATETIME;
--	DECLARE @CreateUserSourceType NVARCHAR(50) = 'ExternalImport'

--    DECLARE @Now DATETIME = GETUTCDATE();

--	SET @StatusThruDate = @Now + 8
--    IF OBJECT_ID('tempdb..#HoldUserList') IS NULL
--    BEGIN
--        CREATE TABLE #HoldUserList
--        (
--            Id INT IDENTITY(1, 1),
--            FirstName NVARCHAR(200),
--            MiddleName NVARCHAR(100),
--            LastName NVARCHAR(200),
--            GBUserType NVARCHAR(200),
--            ThirdPartyIDP NVARCHAR(10),
--            LoginName NVARCHAR(200),
--            NotificationEmail NVARCHAR(200),
--            TemporaryPassword NVARCHAR(MAX),
--            UserEffectiveDate DATETIME,
--            UserExprirationDate DATETIME,
--            Phone NVARCHAR(20),
--            PhoneType NVARCHAR(100),
--            PreferredContactMethod NVARCHAR(200),
--            Title NVARCHAR(200),
--            CompanyJobTitle NVARCHAR(200),
--            PStatus BIT
--                DEFAULT 0
--        );
--    END;
--    INSERT INTO #HoldUserList
--    (
--        FirstName,
--        MiddleName,
--        LastName,
--        GBUserType,
--        ThirdPartyIDP,
--        LoginName,
--        NotificationEmail,
--        TemporaryPassword,
--        UserEffectiveDate,
--        UserExprirationDate,
--        Phone,
--        PhoneType,
--        PreferredContactMethod,
--        Title,
--        CompanyJobTitle
--    )
--    SELECT FirstName,
--           MiddleName,
--           LastName,
--           GBUserType,
--           ThirdPartyIDP,
--           LoginName,
--           NotificationEmail,
--           TemporaryPassword,
--           UserEffectiveDate,
--           UserExprirationDate,
--           Phone,
--           PhoneType,
--           PreferredContactMethod,
--           Title,
--           CompanyJobTitle
--    FROM @UserDetails;

--    SELECT @OrganizationId = O.PartyId,
--           @OrganizationRealPageId = P.RealPageId
--    FROM Enterprise.Organization O
--        INNER JOIN Enterprise.Party P
--            ON P.PartyId = O.PartyId
--    WHERE O.Name = @OrganizationName;


--    WHILE EXISTS (SELECT 1 FROM #HoldUserList WHERE PStatus = 0)
--    BEGIN
--        SELECT TOP 1
--               @Id = Id,
--               @FirstName = FirstName,
--               @MiddleName = MiddleName,
--               @LastName = LastName,
--               @UserType = GBUserType,
--               @ThirdPartyIDP = ThirdPartyIDP,
--               @LoginName = LoginName,
--               @NotificationEmail = NotificationEmail,
--               @TemporaryPassword = TemporaryPassword,
--               @UserEffectiveDate = ISNULL(UserEffectiveDate, GETUTCDATE()),
--               @UserExprirationDate = UserExprirationDate,
--               @Phone = Phone,
--               @PhoneType = PhoneType,
--               @PreferredContactMethod = PreferredContactMethod,
--               @Title = Title,
--               @CompanyJobTitle = CompanyJobTitle
--        FROM #HoldUserList
--        WHERE PStatus = 0
--        ORDER BY Id;
--        SELECT @PartyRoleTypeId = PartyRoleTypeId
--        FROM Enterprise.RoleType
--        WHERE Name = CASE
--                         WHEN @UserType = 'RealPage System Administrator' THEN
--                             'SuperUser'
--                         WHEN @UserType = 'Regular User (No Email)' THEN
--                             'User (No Email)'
--                         WHEN @UserType = 'Regular User' THEN
--                             'User'
--                         ELSE
--                             @UserType
--                     END;
--        IF NOT EXISTS (SELECT 1 FROM Ident.UserLogin WHERE LoginName = @LoginName)
--        BEGIN
--            IF (@UserType = 'RealPage System Administrator')
--            BEGIN
--                SELECT @PersonaTypeId = PersonaTypeId
--                FROM Person.PersonaType
--                WHERE Name = 'System Administrator';
--            END;
--            ELSE
--            BEGIN
--                SELECT @PersonaTypeId = PersonaTypeId
--                FROM Person.PersonaType
--                WHERE Name = 'Primary';
--            END;

--            SELECT @UserStatus = StatusTypeId
--            FROM Enterprise.StatusType
--            WHERE Name = 'Pending';

--            EXEC Person.CreatePerson @FirstName = @FirstName,
--                                     @MiddleName = @MiddleName,
--                                     @LastName = @LastName,
--                                     @Title = @Title,
--                                     @Suffix = '',
--                                     @PreferredContactMethodId = 0,
--                                     @RealPageId = @UserRealPageId OUTPUT;

--            EXEC Ident.CreateUserLogin @RealPageId = @UserRealPageId,
--                                       @LoginName = @LoginName,
--                                       @FromDate = @UserEffectiveDate,
--                                       @ThruDate = NULL,
--									   @CreateUserSourceType = @CreateUserSourceType;

--            SELECT @UserId = UserId
--            FROM Ident.UserLogin
--            WHERE LoginName = @LoginName;

--            SET @PersonaId = NULL;

----8/19/219 Moonte jennings
--DECLARE @UserLoginPersonaId bigint
--EXEC Ident.CreateUserLoginPersona @UserId,@UserStatus,'True', @Now,NULL
--SELECT  @UserLoginPersonaId =  UserLoginPersonaId 
--FROM  ident.UserLoginPersona 
--WHERE UserLoginId = @UserId AND StatusTypeId = @UserStatus AND FromDate = @Now
----end edit

--            EXEC Person.CreatePersona @PersonRealPageId = @UserRealPageId,
--									  @UserLoginPersonaId = @UserLoginPersonaId,
--                                      @OrganizationRealPageId = @OrganizationRealPageId,
--                                      @PersonaTypeId = @PersonaTypeId,
--                                      @PersonaEnvironmentTypeId = 1,
--                                      @UserId = @UserId,
--                                      @FromDate = @Now,
--                                      @ThruDate = NULL,
--                                      @PersonaId = @PersonaId OUTPUT;


--            IF @ThirdPartyIDP = 'Y'
--            BEGIN

--                SELECT @OrganizationIDPCMId = i.ContactMechanismId
--                FROM Enterprise.Organization AS O
--                    INNER JOIN Ident.IdentityProviderType AS i
--                        ON i.IdentityProviderTypeId = O.IdentityProviderTypeId
--                WHERE O.PartyId = @OrganizationId;

--                EXEC Ident.LinkIdentityProviderToUserLogin @PersonaId = @PersonaId,
--                                                           @UserId = @UserId,
--                                                           @ContactMechanismID = @OrganizationIDPCMId;
--            END;
--            ELSE
--            BEGIN
--                SELECT @OrganizationIDPCMId = i.ContactMechanismId
--                FROM Enterprise.Organization AS O
--                    INNER JOIN Ident.IdentityProviderType AS i
--                        ON i.IdentityProviderTypeId = O.IdentityProviderTypeId
--                WHERE i.Name = 'ID3'
--                      AND O.PartyId = @OrganizationId;

--                EXEC Ident.LinkIdentityProviderToUserLogin @PersonaId = @PersonaId,
--                                                           @UserId = @UserId,
--                                                           @ContactMechanismID = @OrganizationIDPCMId;
--            END;

--            IF (
--                   @UserType IN ( 'Regular User (No Email)', 'Regular User' )
--                   AND @ThirdPartyIDP = 'N'
--               )
--            BEGIN
--                SET @Pwdhash
--                    = N'lDgqPfSFN5tcdffGrVzejzpF/BjE62Hy+HW5b12ozo4zMCbAvxLcrQGvgzrH7i2UTPmTD60RdSBYW4KsaPXZTF1/CCJpZ+dH/MoRRkSIykB0KzM7RcRsmQmX8wLau27+i6uhg1JDV+AorW6fP1t6O1k3lW+OoUSHU6nQDIz4tBg=';
--                SET @PwdSalt
--                    = N'ydyQsxBSMGnRF5YRCieibEzkTFVIfLezxeseMjrrIrsVUOV6o+gjIGpiOLht0KCREjeGEP9LKu4Jb0ge0Ecl6fZXB2D/M434T80E9SjN8bbPsvPHFxNTRCe2m/RF6+zvV4gfsztSAhN+8Uhzh3JUwH/bjsys7B82E/MciUNBf/c=';
--                UPDATE Ident.UserLogin
--                SET PasswordHash = @Pwdhash,
--                    PasswordSalt = @PwdSalt,
--                    PasswordModifiedDate = DATEADD(YEAR, 50, GETDATE())
--                FROM Ident.UserLogin
--                WHERE UserId = @UserId;


--				EXEC Ident.UpdateUserStatus
--				 @RealPageId = @UserRealPageId,
--				 @StatusTypeId = @userStatus,
--				 @FromDate = @now,
--				 @ThruDate = @StatusThrudate;

--            END;
--            ELSE
--            BEGIN
--                UPDATE Ident.UserLogin
--                SET PasswordModifiedDate = DATEADD(YEAR, 50, GETDATE())
--                FROM Ident.UserLogin
--                WHERE UserId = @UserId;

--				EXEC Ident.UpdateUserStatus
--				 @RealPageId = @UserRealPageId,
--				 @StatusTypeId = @userStatus,
--				 @FromDate = @now,
--				 @ThruDate = @StatusThrudate;
--            END;

--            EXEC Enterprise.CreatePartyRoleByRealPageId @RealPageId = @UserRealPageId,
--                                                        @RoleTypeID = @PartyRoleTypeId;

--            SELECT @CompanyJobTitleId = Name
--            FROM Enterprise.RoleType
--            WHERE Name = @CompanyJobTitle;

--            IF @CompanyJobTitleId IS NULL
--               OR @CompanyJobTitleId = ''
--                SET @CompanyJobTitleId = 328;

--            EXEC Person.LinkPersonToOrganization @PersonRealPageId = @UserRealPageId,
--                                                 @OrganizationRealPageId = @OrganizationRealPageId,
--                                                 @RoleTypeIdFrom = @CompanyJobTitleId,
--                                                 @RoleTypeIdTo = 203;

--            EXEC Person.LinkPersonToOrganization @PersonRealPageId = @UserRealPageId,
--                                                 @OrganizationRealPageId = @OrganizationRealPageId,
--                                                 @RoleTypeIdFrom = @PartyRoleTypeId,
--                                                 @RoleTypeIdTo = 205;

--            EXEC Person.CreateContactMechanism @ContactMechanismId = @ContactMechanismId OUTPUT;

--            EXEC Person.LinkContactMechanismToParty @RealPageId = @UserRealPageId,
--                                                    @PartyContactMechanismId = 0,
--                                                    @ContactMechanismId = @ContactMechanismId,
--                                                    @FromDate = @Now,
--                                                    @ThruDate = '9999-12-31 23:59:59.997';

--            SELECT @PartyContactMechanismId = a.PartyContactMechanismId
--            FROM Enterprise.PartyContactMechanism AS a
--                INNER JOIN Enterprise.Party AS p
--                    ON a.PartyId = p.PartyId
--            WHERE p.RealPageId = @UserRealPageId;

--            EXEC Person.LinkUsageTypeToPartyContactMechanism @PartyContactMechanismId = @PartyContactMechanismId,
--                                                             @ContactMechanismUsageTypeId = 301;

--            SELECT @ContactMechanismUsageId = ContactMechanismUsageID
--            FROM Enterprise.ContactMechanismUsage
--            WHERE PartyContactMechanismID = @PartyContactMechanismId;

--            EXEC Person.CreateElectronicAddress @ContactMechanismId = @ContactMechanismId,
--                                                @ElectronicAddressString = @LoginName,
--                                                @ElectronicAddressType = N'Email';
--            IF (@LoginName <> @NotificationEmail)
--            BEGIN
--                EXEC Person.CreateElectronicAddress @ContactMechanismId = @ContactMechanismId,
--                                                    @ElectronicAddressString = @NotificationEmail,
--                                                    @ElectronicAddressType = N'Email';
--            END;
--            IF (@PartyRoleTypeId = 402)
--            BEGIN
--                SELECT @RoleId = R.RoleID
--                FROM Enterprise.Role R
--                    INNER JOIN Enterprise.RoleValueType RVT
--                        ON RVT.RoleValueTypeId = R.RoleValueTypeId
--                WHERE RVT.Value = 'User Administrator'
--                      AND R.PartyID = @OrganizationId;
--            END;
--            ELSE
--            BEGIN
--                SELECT @RoleId = R.RoleID
--                FROM Enterprise.Role R
--                    INNER JOIN Enterprise.RoleValueType RVT
--                        ON RVT.RoleValueTypeId = R.RoleValueTypeId
--                WHERE R.DefaultRole = 1
--                      AND R.PartyID = @OrganizationId;
--            END;

--            IF NOT EXISTS
--            (
--                SELECT 1
--                FROM Enterprise.PersonaPrivilege
--                WHERE PersonaId = @PersonaId
--                      AND RoleID = @RoleId
--            )
--            BEGIN
--                EXEC Enterprise.LinkPersonaToRole @PersonaID = @PersonaId,
--                                                  @RoleID = @RoleId,
--                                                  @PersonaPrivilgeID = @PerPriv OUTPUT;
--                --EXEC [Enterprise].[LinkPersonaToRole] @PersonaID = '+CONVERT(VARCHAR, @PersonaId)+', @RoleID = '+CONVERT(VARCHAR, @RoleId)+', @PersonaPrivilgeID = @PerPriv OUTPUT;


--            END;
--        END;
--		UPDATE #HoldUserList
--		SET PStatus = 1
--		WHERE Id = @Id;
--    END;
select 1
END;
