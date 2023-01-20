

DECLARE @ProductId INT = 3, @Now DATETIME = GETUTCDATE(), @ProductsettingTypeid int;
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'ImpersonationRightsToBeExcluded')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('ImpersonationRightsToBeExcluded', 'Rights to be ignored when impersonating a user', 0);
END
-- Enabling 
IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
    WHERE productid = @ProductId AND pst.Name = 'ImpersonationRightsToBeExcluded' )
BEGIN
    SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'ImpersonationRightsToBeExcluded'
    exec [Enterprise].[SetProductSetting] 0,@Productid,@ProductsettingTypeid,'InternalAdminaccessToUnifiedSettings,EmployeeAccesstoManageSettingsTemplates,AccessSettingsAdmin,EmployeeAccessUnifiedReportingAdminConsole,ManageNotifications,EmployeeAccessToInternalRolesAndRightsSetup,EmployeeAccessToCompanySetup,AbilityToAddProducts,EmployeeAccessToInternalClientSettings,EmployeeAccessToInternalRolesAndRightsSetup,EmployeeAccessToLoginPageSetup,CreatePlatformAlerts,ApprovePlatformAlerts'
END
GO

Declare @Right1 BigInt,@Right2 BigInt,@Right3 BigInt, @Right4 BigInt, @SidemenuId BigInt, @Now DATETIME = GETUTCDATE();
Declare @UserId bigint;
SELECT @UserId = UserId
FROM   Ident.UserLogin
WHERE  LoginName LIKE 'realpagead@%';

Select @SidemenuId = RouteId from [Security].[Route] where RouteValue = 'SideMenu';

Select @Right1 = RightId from Security.[Right] where RightName = 'Managecompanylevelsettings'
Select @Right2 = RightId from Security.[Right] where RightName = 'Viewallcompanylevelsettings';
Select @Right3 = RightId from Security.[Right] where RightName = 'Managepropertylevelsettings';
Select @Right4 = RightId from Security.[Right] where RightName = 'Viewallpropertylevelsettings';


IF NOT EXISTS (Select TOP 1 1 from Security.[RightRoute] where RightId = @Right1 and RouteId =@SidemenuId)
BEGIN
      Insert into Security.[RightRoute] values (@Right1,@SidemenuId,@UserId,@Now)
END
IF NOT EXISTS (Select TOP 1 1 from Security.[RightRoute] where RightId = @Right2 and RouteId =@SidemenuId)
BEGIN
      Insert into Security.[RightRoute] values (@Right2,@SidemenuId,@UserId,@Now)
END
IF NOT EXISTS (Select TOP 1 1 from Security.[RightRoute] where RightId = @Right3 and RouteId =@SidemenuId)
BEGIN
      Insert into Security.[RightRoute] values (@Right3,@SidemenuId,@UserId,@Now)
END
IF NOT EXISTS (Select TOP 1 1 from Security.[RightRoute] where RightId = @Right4 and RouteId =@SidemenuId)
BEGIN
      Insert into Security.[RightRoute] values (@Right4,@SidemenuId,@UserId,@Now)
END
GO

Declare @navigationMenuID varchar(50), @rightId BIGINt;
Select @rightId = RightId from Security.[Right] where RightName= 'ViewUnifiedSettings'
Select @navigationMenuID = Id from Enterprise.NavigationMenu where Title = 'Settings Activity Log' and Origin = 'unified-settings';

IF NOT EXISTS (Select Top 1 1 from Enterprise.NavigationMenuRights where  NavigationMenuId = @navigationMenuID and RightId = @rightId)
Begin
    Insert into Enterprise.NavigationMenuRights values (@navigationMenuID,@rightId);
end

GO


Declare @rightID BIGINT, @routeID BIGINT, @userId Bigint;
Declare @rig1 BIGINT;
Declare @navimenuID BIGINT, @navbar1 BIGINT;

SELECT @UserId = UserId
FROM   Ident.UserLogin
WHERE  LoginName LIKE 'realpagead@%';

select @rightID = RightId from Security.[Right] where RightName = 'InternalAdminaccessToUnifiedSettings';
Select @rig1 = RightId from Security.[Right] where RightName = 'ViewSettingsTemplates';

Select @navbar1 = Id from Enterprise.NavigationMenu where Title = 'Manage Templates' and Origin = 'unified-settings';
Select @routeID  = RouteId from security.route where RouteValue = 'SideMenu';

IF NOT EXISTS (Select TOP 1 1 from security.RightRoute where RightId = @rightID)
BEGIN
  INSERT INTO Security.RightRoute values (@rightID,@routeID,@userID,GETUTCDATE())
END
IF NOT EXISTS (Select TOP 1 1 from security.RightRoute where RightId = @rig1)
BEGIN
  INSERT INTO Security.RightRoute values (@rig1,@routeID,@userID,GETUTCDATE())
END

IF NOT EXISTS (Select TOP 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @navbar1 and RightId = @rig1)
BEGIN
  INSERT INTO  Enterprise.NavigationMenuRights values (@navbar1,@rig1)
END
GO

Declare @UserId bigint;
SELECT @UserId = UserId
FROM   Ident.UserLogin
WHERE  LoginName LIKE 'realpagead@%';
IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right] WHERE RightName ='ManageAdminSupportPortalProductAccess' AND ProductId = 89)
BEGIN
        INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight)
        VALUES('ManageAdminSupportPortalProductAccess','For Admin & Support Portal, this right unlocks the ability to edit the Product assignment and Product Access Details for a user, assuming that the user can access the page because of Ability to view users.  Al','Manage AdminSupportPortal Product Access',13,9,3 ,89,@UserId,getUTCDate(),0);
END

Declare @Right1 bigint;

Select @Right1 = RightId from Security.[Right] where RightName = 'ManageAdminSupportPortalProductAccess'
IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[RoleRight] WHERE RightId = @Right1 AND RoleId = 1 )
BEGIN
    Insert into Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate) Values(1,@Right1,@UserId,getUTCDate());
END
GO


IF EXISTS (Select Top 1 1 from Security.[Right] where RightName = 'ViewSettingsTemplates' and PersistRight =0)
BEGIN
   UPDATE  Security.[Right] Set PersistRight = 1  where RightName = 'ViewSettingsTemplates';
END
IF EXISTS (Select Top 1 1 from Security.[Right] where RightName = 'Managecompanylevelsettings' and PersistRight =0)
BEGIN
   UPDATE  Security.[Right] Set PersistRight = 1  where RightName = 'Managecompanylevelsettings';
END
IF EXISTS (Select Top 1 1 from Security.[Right] where RightName = 'Managepropertylevelsettings' and PersistRight =0)
BEGIN
   UPDATE  Security.[Right] Set PersistRight = 1  where RightName = 'Managepropertylevelsettings';
END
IF EXISTS (Select Top 1 1 from Security.[Right] where RightName = 'Viewallpropertylevelsettings' and PersistRight =0)
BEGIN
   UPDATE  Security.[Right] Set PersistRight = 1  where RightName = 'Viewallpropertylevelsettings';
END
IF EXISTS (Select Top 1 1 from Security.[Right] where RightName = 'Viewallcompanylevelsettings' and PersistRight =0)
BEGIN
   UPDATE  Security.[Right] Set PersistRight = 1  where RightName = 'Viewallcompanylevelsettings';
END
