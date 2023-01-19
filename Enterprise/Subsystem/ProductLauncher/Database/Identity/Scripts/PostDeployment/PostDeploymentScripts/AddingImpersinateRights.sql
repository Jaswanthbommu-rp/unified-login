

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
go

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
