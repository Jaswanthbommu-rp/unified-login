Create PROCEDURE [Security].[SetupSuperUser]
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
		SET @RoleName = 'Basic End User';
        SELECT @RoleId = RoleID
        FROM Security.Role 
        WHERE RoleName = @RoleName
		--Set default role
		IF NOT EXISTS  (Select 1 From Security.OrganizationDefaultRole Where OrgPartyId = @OrganizationId)
		BEGIN
			INSERT INTO Security.OrganizationDefaultRole(OrgPartyId,RoleId,CreatedBy,CreatedDate)
			SELECT @OrganizationId,@RoleId,@UserId,GETDATE()
		END
		ELSE
		BEGIN
			UPDATE Security.OrganizationDefaultRole SET RoleId = @RoleId
			WHERE OrgPartyId = @OrganizationId
		END

		--Link role to persona
		SET @RoleName = 'User Administrator';
        SELECT @RoleId = RoleID
        FROM Security.Role 
        WHERE RoleName = @RoleName
		IF NOT EXISTS  (Select 1 From Security.PersonaRole Where PersonaId = @PersonaId)
		BEGIN
			INSERT INTO Security.PersonaRole(PersonaId,RoleId,CreatedBy,CreatedDate)
			SELECT @PersonaId,@RoleId,@UserId,GETDATE()
		END
		ELSE
		BEGIN
			UPDATE Security.PersonaRole SET RoleId = @RoleId
			WHERE PersonaId = @PersonaId
		END
		----------------------------------
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
            WHERE OrganizationPartyId = @OrganizationId
        )
            BEGIN
                DECLARE @MasterCOnfigurationId INT;
                DECLARE @AttributeId INT;
                DECLARE @RealPageId UNIQUEIDENTIFIER;
                DECLARE @LoginName NVARCHAR(50);
                DECLARE @OrgId INT;
                DECLARE @mastersettingtypeid INT;
                DECLARE @MasterSettingId INT;
                SELECT @mastersettingtypeid = MasterSettingTypeId
                FROM Enterprise.MasterSettingType
                WHERE Name = 'RealPageEmployeeAccessID';
                SELECT @UserId = UL.UserId, 
                       @RealPageId = P.RealPageId, 
                       @LoginName = UL.LoginName, 
                       @OrgId = ULP.OrganizationPartyId, 
                       @MasterCOnfigurationId = MC.MasterConfigurationId
                FROM Enterprise.Party AS P
                     INNER JOIN Ident.UserLogin AS UL ON UL.PersonPartyId = P.PartyId
                     INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
                     INNER JOIN Person.Persona AS PP ON ULP.UserLoginPersonaId = PP.UserLoginPersonaId
                     INNER JOIN Enterprise.MasterConfiguration AS MC ON MC.AttributeId = ULP.OrganizationPartyId
                WHERE UL.LoginName IN(@Email)
                     AND MC.MasterConfigurationTypeId = 2;
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM Enterprise.MasterSetting
                    WHERE Value = CONVERT(VARCHAR(36), @RealPageId)
                )
                    BEGIN
                        INSERT INTO Enterprise.MasterSetting
                        (MasterSettingTypeId, 
                         Value, 
                         FromDate
                        )
                        VALUES
                        (@mastersettingtypeid, 
                         @RealPageId, 
                         @NOW
                        );
                        SELECT @MasterSettingId = SCOPE_IDENTITY();
                END;
                    ELSE
                    BEGIN
                        SELECT @MasterSettingId = MasterSettingId
                        FROM Enterprise.MasterSetting
                        WHERE Value = CONVERT(VARCHAR(36), @RealPageId);
                END;
                IF NOT EXISTS
                (
                    SELECT 1
                    FROM Enterprise.MasterConfigurationSetting
                    WHERE MasterConfigurationId = @MasterCOnfigurationId
                          AND MasterSettingId = @MasterSettingId
                )
                    BEGIN
                        INSERT INTO Enterprise.MasterConfigurationSetting
                        (MasterConfigurationId, 
                         MasterSettingId
                        )
                        VALUES
                        (@MasterCOnfigurationId, 
                         @MasterSettingId
                        );
                END;
        END;
                        -- ADD USER TO SUPPORT TOOL LIST IF NO OTHER USERS EXIST FOR THE COMPANY
    END;
