Go
-- Adding Role Type saml for Admin Portal Product
IF NOT EXISTS(SELECT TOP 1 1 FROM Ident.SamlAttribute where [Name] = 'RoleType')
BEGIN
	INSERT INTO Ident.SamlAttribute([Name], SamlAttributeTypeId, DisplayName)
	VALUES('RoleType',1,'Role Type')
END
Go

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

Select @Right1 = RightId from Security.[Right] where value = 'Manage company-level settings';
Select @Right2 = RightId from Security.[Right] where value = 'View all company-level settings';
Select @Right3 = RightId from Security.[Right] where value = 'Manage property-level settings';
Select @Right4 = RightId from Security.[Right] where value = 'View all property-level settings';


If Not Exists (Select Top 1 1 from Security.[RightRoute] where RightId in (@Right1,@Right2,@Right3,@Right4) and RouteId = @SidemenuId)
Begin
     Insert into Security.[RightRoute] values (@Right1,@SidemenuId,@UserId,@Now),(@Right2,@SidemenuId,@UserId,@Now),(@Right3,@SidemenuId,@UserId,@Now),(@Right4,@SidemenuId,@UserId,@Now);
End
GO
