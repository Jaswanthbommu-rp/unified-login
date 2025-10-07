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
 @AssignedProductId [enterprise].PRODUCTIDTYPE READONLY
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
        AND OrgPartyID IS NULL
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
		SET @RoleName = @PlatformAdminRoleValue;
        SELECT @RoleId = RoleID
        FROM Security.Role 
        WHERE RoleName = @RoleName
        AND OrgPartyID IS NULL

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
        --------------------------------
		Declare @SaveEnterpriseRoleData Varchar(10)
		SELECT	@SaveEnterpriseRoleData = ps.Value				
			FROM	Enterprise.GlobalProductConfiguration gpc
					JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
					JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId
					JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
			WHERE  gpc.ProductId = 3
			AND (gpc.ThruDate IS NULL)
			AND ( pc.ThruDate IS NULL)
			AND ( ps.ThruDate IS NULL)
			And PST.Name = 'SaveRoleDataInEnterprise'
		IF (@SaveEnterpriseRoleData = '1')
		Begin
			Exec [Security].[SetupSuperUserEnterpriseRoles] @OrganizationId, @PersonaId
		End
		----------------------------------
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

         IF NOT EXISTS ( SELECT TOP (1) 1 FROM Enterprise.PropertyInstanceMapping 
                          WHERE PersonaId  = @PersonaId AND ProductId = 3 AND PropertyInstanceId = -1  AND ThruDate IS NULL
         )
         BEGIN
	      INSERT INTO Enterprise.PropertyInstanceMapping
	      (
	           PersonaId
	          ,PropertyInstanceId
	          ,ProductId
	          ,FromDate
	          ,ThruDate
              ,Active
	      )
	      VALUES
	         (@PersonaId,    
	          -1,           
	          3,            
	          @NOW,
	          NULL ,        
             1             
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

        --Insert External UserRelationship type for userloginpersonaid if not exists
		IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ExternalUserRelationship WHERE UserLoginPersonaId = @UserLoginPersonaId)
         BEGIN
			INSERT INTO Enterprise.ExternalUserRelationship(UserLoginPersonaId,ThirdPartyRelationshipId,CompanyName,ThirdPartyCompanyPartyId)
			VALUES(@UserLoginPersonaId, 8, NULL, NULL);
		END
        --Insert External UserRelationship type for userloginpersonaid if not exists
    END;
