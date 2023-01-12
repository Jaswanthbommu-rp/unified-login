

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
    exec [Enterprise].[SetProductSetting] 0,@Productid,@ProductsettingTypeid,'InternalAdminaccessToUnifiedSettings,EmployeeAccesstoManageSettingsTemplates,AccessSettingsAdmin,EmployeeAccessUnifiedReportingAdminConsole'
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