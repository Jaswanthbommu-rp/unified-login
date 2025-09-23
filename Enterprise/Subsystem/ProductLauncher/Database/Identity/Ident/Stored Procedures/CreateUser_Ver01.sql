CREATE PROCEDURE [Ident].[CreateUser_Ver01] (  
 @OrganizationId bigint,  
 @FirstName nvarchar(200),  
 @MiddleName nvarchar(100),  
 @LastName nvarchar(200),  
 @UserTypeId int,  
 @ThirdPartyIDP bit = 0,  
 @LoginName varchar(255),  
 @NotificationEmail varchar(255),  
 @UserEffectiveDate datetime,  
 @UserExpirationDate datetime,  
 @Phone nvarchar(20),  
 @PhoneType nvarchar(100),  
 @PreferredContactMethod nvarchar(200),  
 @Title nvarchar(100),  
 @Pwdhash nvarchar(510) = null,  
 @PwdSalt nvarchar(510) = null,  
 @CompanyJobTitleId int = NULL,  
 @Suffix nvarchar(20) = NULL,  
 @CreateUserSourceType nvarchar(50) = 'UnifiedPlatform',-- default - unifiedplatform  
 @EmployeeId nvarchar(max) = NULL  
)  
AS  
BEGIN  
 DECLARE @UserRealPageId uniqueidentifier,  
  @OrganizationRealPageId uniqueidentifier,  
  @OrganizationIDPCMId int,  
  @UserStatus int,  
  @UserId bigint,  
  @PersonaId bigint,  
  @RoleId int,  
  @ContactMechanismId bigint,  
  @PartyContactMechanismId bigint,  
  @ContactMechanismUsageId bigint,  
  @PersonaTypeId int,  
  @PerPriv int,  
  @RoleName varchar(200),  
  @Now datetime = GETUTCDATE(),  
  @UserType nvarchar(50),  
  @UserLoginPersonaId bigint,  
  @RoleTypeIdTo int,  
  @ContactMechanismUsageTypeId int  

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
  
 SELECT @UserType = Name  
 FROM  Enterprise.RoleType  
 WHERE PartyRoleTypeId = @UserTypeId;  
  
 SELECT @OrganizationRealPageId = P.RealPageId  
 FROM  Enterprise.Organization O  
     INNER JOIN Enterprise.Party P ON P.PartyId = O.PartyId  
 WHERE P.PartyId = @OrganizationId;  
  
 IF NOT EXISTS  
 (  
  SELECT 1  
  FROM Ident.UserLogin  
  WHERE LoginName = @LoginName  
 )  
 BEGIN  
  IF(@UserType = 'RealPage System Administrator') -- TODO:should be SuperUser  
  BEGIN  
   SELECT @PersonaTypeId = PersonaTypeId  
   FROM  Person.PersonaType  
   WHERE Name = 'System Administrator';  
  END;  
  ELSE  
  BEGIN  
   SELECT @PersonaTypeId = PersonaTypeId  
   FROM  Person.PersonaType  
   WHERE Name = 'Primary';  
  END;  
  
  SELECT @UserStatus = StatusTypeId  
  FROM  Enterprise.StatusType  
  WHERE Name = 'Pending';  
  
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
   @LoginName = @LoginName,  
   @CreateUserSourceType = @CreateUserSourceType  
  
  SELECT @UserId = UserId  
  FROM  Ident.UserLogin  
  WHERE LoginName = @LoginName;  
  
  EXEC Ident.CreateUserLoginPersona  
   @UserLoginId = @UserId,  
   @StatusTypeId = @UserStatus,  
   @OrganizationPartyId = @OrganizationId,  
   @Primaryorganization  = 1,  
   @Fromdate = @UserEffectiveDate,  
   @ThruDate = @UserExpirationDate,  
   @StatusThruDate = @Now  
  
  SELECT @UserLoginPersonaId = UserLoginPersonaId  
  FROM  Ident.UserLoginPersona  
  WHERE UserLoginId = @UserId  
  AND   OrganizationPartyId = @OrganizationId  
  AND   PrimaryOrganization = 1  
  
  SET @PersonaId = NULL;  
  
  EXEC Person.CreatePersona  
   @PersonRealPageId = @UserRealPageId,  
   @UserLoginPersonaId = @UserLoginPersonaId,  
   @OrganizationRealPageId = @OrganizationRealPageId,  
   @PersonaTypeId = @PersonaTypeId,  
   @PersonaEnvironmentTypeId = 1,  
   @UserId = @UserId,  
   @FromDate = @Now,  
   @ThruDate = NULL,  
   @PersonaId = @PersonaId OUTPUT;  
  
  IF @ThirdPartyIDP = '1'  
  BEGIN  
   SELECT @OrganizationIDPCMId = i.ContactMechanismId  
   FROM  Enterprise.Organization AS O  
       INNER JOIN Ident.IdentityProviderType AS i ON i.IdentityProviderTypeId = O.IdentityProviderTypeId  
   WHERE O.PartyId = @OrganizationId;  
  
   EXEC Ident.LinkIdentityProviderToUserLogin  
    @UserId = @UserId,  
    @ContactMechanismID = @OrganizationIDPCMId;  
  END;  
  ELSE  
  BEGIN  
   SELECT @OrganizationIDPCMId = ContactMechanismId  
   FROM  Ident.IdentityProviderType  
   WHERE Name = 'ID3'  
  
   EXEC Ident.LinkIdentityProviderToUserLogin  
    @UserId = @UserId,  
    @ContactMechanismID = @OrganizationIDPCMId;  
  END;  
  
  IF(@UserType IN('User (No Email)', 'User') AND @ThirdPartyIDP = '0')  
  BEGIN  
   IF(@Pwdhash <> '')  
   BEGIN  
    UPDATE Ident.UserLogin  
    SET   PasswordHash = @Pwdhash,  
        PasswordSalt = @PwdSalt,  
        PasswordModifiedDate = null  
    WHERE UserId = @UserId;  
   END  
  
   UPDATE Ident.UserLoginPersona  
   SET   StatusThruDate = DATEADD(day, 3, FromDate)  
   WHERE UserLoginId = @UserId  
   AND   OrganizationPartyId = @OrganizationId  
  END;  
  ELSE  
  BEGIN  
   UPDATE Ident.UserLogin  
   SET   PasswordModifiedDate = null  
   WHERE UserId = @UserId;  
  
   UPDATE Ident.UserLoginPersona  
   SET StatusTypeId = @UserStatus,  
       StatusThruDate = DATEADD(day, 3, FromDate),
	   UserDeactivationDate = CASE WHEN @UserStatus = 24 THEN GETUTCDATE() ELSE NULL END
   WHERE UserLoginId = @UserId  
   AND   OrganizationPartyId = @OrganizationId  
  END;  
  
  EXEC Enterprise.CreatePartyRoleByRealPageId  
   @RealPageId = @UserRealPageId,  
   @RoleTypeID = @UserTypeId;  
  
  IF @CompanyJobTitleId IS NULL OR @CompanyJobTitleId = ''  
  BEGIN  
   SELECT @CompanyJobTitleId = PartyRoleTypeId  
   FROM  Enterprise.RoleType  
   WHERE Name = 'Leasing Agent'  
  END;  
  
  SELECT @RoleTypeIdTo = PartyRoleTypeId  
  FROM  Enterprise.RoleType  
  WHERE Name = 'Employer'  
  
  EXEC Person.LinkPersonToOrganization  
   @PersonRealPageId = @UserRealPageId,  
   @OrganizationRealPageId = @OrganizationRealPageId,  
   @RoleTypeIdFrom = @CompanyJobTitleId,  
   @RoleTypeIdTo = @RoleTypeIdTo;  
  
  SELECT @RoleTypeIdTo = PartyRoleTypeId  
  FROM  Enterprise.RoleType  
  WHERE Name = 'User Type'  
  
  EXEC Person.LinkPersonToOrganization  
   @PersonRealPageId = @UserRealPageId,  
   @OrganizationRealPageId = @OrganizationRealPageId,  
   @RoleTypeIdFrom = @UserTypeId,  
   @RoleTypeIdTo = @RoleTypeIdTo;  
  
  EXEC Person.CreateContactMechanism  
   @ContactMechanismId = @ContactMechanismId OUTPUT;  
   
  EXEC Person.LinkContactMechanismToParty  
   @RealPageId = @UserRealPageId,  
   @PartyContactMechanismId = 0,  
   @ContactMechanismId = @ContactMechanismId,  
   @FromDate = @Now,  
   @ThruDate = '9999-12-31 23:59:59.997';  
  
  SELECT @PartyContactMechanismId = a.PartyContactMechanismId  
  FROM  Enterprise.PartyContactMechanism AS a  
      INNER JOIN Enterprise.Party AS p ON a.PartyId = p.PartyId  
  WHERE p.RealPageId = @UserRealPageId;  
  
  SELECT @ContactMechanismUsageTypeId = ContactMechanismUsageTypeId  
  FROM  Enterprise.ContactMechanismUsageType  
  WHERE Name = 'Email'  
  
  EXEC Person.LinkUsageTypeToPartyContactMechanism  
   @PartyContactMechanismId = @PartyContactMechanismId,  
   @ContactMechanismUsageTypeId = @ContactMechanismUsageTypeId;  
  
  SELECT @ContactMechanismUsageId = ContactMechanismUsageID  
  FROM  Enterprise.ContactMechanismUsage  
  WHERE PartyContactMechanismID = @PartyContactMechanismId;  
  
  EXEC Person.CreateElectronicAddress  
   @ContactMechanismId = @ContactMechanismId,  
   @ElectronicAddressString = @LoginName,  
   @ElectronicAddressType = N'Email';  
  
  IF(@LoginName <> @NotificationEmail)  
  BEGIN  
   EXEC Person.CreateElectronicAddress  
    @ContactMechanismId = @ContactMechanismId,  
    @ElectronicAddressString = @NotificationEmail,  
    @ElectronicAddressType = N'Email';  
  END;  
  
  IF (@EmployeeId IS NOT NULL)  
  BEGIN  
   EXEC Enterprise.CreateUserEmployeeId   
    @UserLoginPersonaId = @UserLoginPersonaId,  
    @EmployeeId = @EmployeeId  
  END  
  
  IF(@UserTypeId = 402)  
  BEGIN  
   SELECT TOP (1) @RoleId = R.RoleId  
    FROM  Security.Role AS R  
    WHERE R.RoleName = @PlatformAdminRoleValue AND R.OrgPartyID IS NULL AND R.ProductId = 3  
    ORDER BY R.RoleId  
  END;  
  ELSE  
  BEGIN  
   IF EXISTS (SELECT TOP (1) R.RoleId FROM Security.Role R INNER JOIN Security.OrganizationDefaultRole ODR ON ODR.RoleId = R.RoleId WHERE odr.OrgPartyId = @OrganizationId AND R.ProductId = 3)  
   BEGIN  
    SELECT TOP (1) @RoleId = R.RoleId  
     FROM Security.Role R  
     INNER JOIN Security.OrganizationDefaultRole ODR ON ODR.RoleId = R.RoleId  
     WHERE odr.OrgPartyId = @OrganizationId AND R.ProductId = 3  
     ORDER BY r.RoleId  
   END  
   ELSE  
   BEGIN  
    -- IF NOTHING SET AS DEFAULT, DEFAULT TO BASIC END USER  
    SELECT TOP (1) @RoleId = R.RoleId  
     FROM  Security.Role AS R  
     WHERE R.RoleName = 'Basic End User' AND R.OrgPartyID IS NULL AND R.ProductId = 3  
     ORDER BY R.RoleId  
   END  
  END;  
    
  DECLARE @CreatedUserId INT  
  SELECT TOP (1) @CreatedUserId = ulp.UserLoginId  
  FROM Enterprise.OrganizationAdminUser OAU  
   INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = OAU.UserLoginPersonaId  
  WHERE OAU.OrganizationPartyId = @OrganizationId  
  ORDER BY ulp.UserLoginId  
  
  IF NOT EXISTS  
  (  
   SELECT 1  
   FROM Security.PersonaRole  
   WHERE PersonaId = @PersonaId  
   AND  RoleID = @RoleId  
  )  
  BEGIN  
   EXEC Security.LinkPersonaToRole  
   @PersonaID = @PersonaId,  
   @RoleID = @RoleId,  
   @CreatedBy = @CreatedUserId,  
   @PersonaPrivilgeID = @PerPriv OUTPUT;  
  END  
 END;  
END;