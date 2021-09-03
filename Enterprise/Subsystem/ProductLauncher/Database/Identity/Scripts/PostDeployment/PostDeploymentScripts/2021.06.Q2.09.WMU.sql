GO
Declare @ControlId INT 
SELECT @ControlId = ControlId FROM UserManagement.Control WHERE UIId='On-SiteProductAccessAssignnewpropertiesautomaticallyPropertiesSwitchUIId' AND DisplayName = 'Assign new properties automatically'
IF (@ControlId IS NOT NULL)
BEGIN
UPDATE UserManagement.Control SET DisplayName = 'Assign current and new properties automatically' Where ControlId = @ControlId
END
 
GO

--  ADMIN ROLE FIXES
DECLARE @ProductConfiguration INT = 0, @ROLEID VARCHAR(10), @ProductId INT = 39, @ProductSettingId INT, @RoleName nVARCHAR(255) = 'Integration Manager'

SELECT @ProductConfiguration = ConfigurationId FROM enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId AND ThruDate IS NULL
IF (@ProductConfiguration <> 0 )
BEGIN
	IF NOT EXISTS (SELECT TOP 1 (1) FROM enterprise.GlobalProductConfiguration gpc 
			INNER JOIN enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			INNER JOIN enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId 
			INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId 
		WHERE gpc.ProductId = @ProductId
	AND gpc.ThruDate IS NULL AND pc.ThruDate IS NULL
	AND pst.Name = 'SuperUserRoleId' )
	BEGIN
		SELECT TOP 1 @ROLEID = RoleId FROM security.Role WHERE ProductId = @ProductId AND OrgPartyID IS NULL AND RoleName = @RoleName
		IF @ROLEID <> 0
		BEGIN
			IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' )
			BEGIN
				INSERT INTO Enterprise.ProductSetting  ( ProductId, ProductSettingTypeId, Value, FromDate, ThruDate )
				SELECT @ProductId, ProductSettingTypeId, @ROLEID, GETUTCDATE(), NULL
					FROM Enterprise.ProductSettingType WHERE NAME = 'SuperUserRoleId'
			END
			SELECT @ProductSettingId = productsettingid FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' 
			IF NOT EXISTS (SELECT TOP 1 (1) FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @ProductConfiguration AND @ProductSettingId = ProductSettingId AND ThruDate IS NULL)
			BEGIN
				INSERT INTO enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				SELECT @ProductConfiguration, @ProductSettingId, GETUTCDATE(), NULL
			END	
		END
	END
END

GO

DECLARE @ProductConfiguration INT = 0, @ROLEID VARCHAR(10), @ProductId INT = 57, @ProductSettingId INT, @RoleName nVARCHAR(255) = 'Portfolio Manager'

SELECT @ProductConfiguration = ConfigurationId FROM enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId AND ThruDate IS NULL
IF (@ProductConfiguration <> 0 )
BEGIN
	IF NOT EXISTS (SELECT TOP 1 (1) FROM enterprise.GlobalProductConfiguration gpc 
			INNER JOIN enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			INNER JOIN enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId 
			INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId 
		WHERE gpc.ProductId = @ProductId
	AND gpc.ThruDate IS NULL AND pc.ThruDate IS NULL
	AND pst.Name = 'SuperUserRoleId' )
	BEGIN
		SELECT TOP 1 @ROLEID = RoleId FROM security.Role WHERE ProductId = @ProductId AND OrgPartyID IS NULL AND RoleName = @RoleName
		IF @ROLEID <> 0
		BEGIN
			IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' )
			BEGIN
				INSERT INTO Enterprise.ProductSetting  ( ProductId, ProductSettingTypeId, Value, FromDate, ThruDate )
				SELECT @ProductId, ProductSettingTypeId, @ROLEID, GETUTCDATE(), NULL
					FROM Enterprise.ProductSettingType WHERE NAME = 'SuperUserRoleId'
			END
			SELECT @ProductSettingId = productsettingid FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' 
			IF NOT EXISTS (SELECT TOP 1 (1) FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @ProductConfiguration AND @ProductSettingId = ProductSettingId AND ThruDate IS NULL)
			BEGIN
				INSERT INTO enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				SELECT @ProductConfiguration, @ProductSettingId, GETUTCDATE(), NULL
			END	
		END
	END
END

GO

DECLARE @ProductConfiguration INT = 0, @ROLEID VARCHAR(10), @ProductId INT = 58, @ProductSettingId INT, @RoleName nVARCHAR(255) = 'Portfolio Manager'

SELECT @ProductConfiguration = ConfigurationId FROM enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId AND ThruDate IS NULL
IF (@ProductConfiguration <> 0 )
BEGIN
	IF NOT EXISTS (SELECT TOP 1 (1) FROM enterprise.GlobalProductConfiguration gpc 
			INNER JOIN enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			INNER JOIN enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId 
			INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId 
		WHERE gpc.ProductId = @ProductId
	AND gpc.ThruDate IS NULL AND pc.ThruDate IS NULL
	AND pst.Name = 'SuperUserRoleId' )
	BEGIN
		SELECT TOP 1 @ROLEID = RoleId FROM security.Role WHERE ProductId = @ProductId AND OrgPartyID IS NULL AND RoleName = @RoleName
		IF @ROLEID <> 0
		BEGIN
			IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' )
			BEGIN
				INSERT INTO Enterprise.ProductSetting  ( ProductId, ProductSettingTypeId, Value, FromDate, ThruDate )
				SELECT @ProductId, ProductSettingTypeId, @ROLEID, GETUTCDATE(), NULL
					FROM Enterprise.ProductSettingType WHERE NAME = 'SuperUserRoleId'
			END
			SELECT @ProductSettingId = productsettingid FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' 
			IF NOT EXISTS (SELECT TOP 1 (1) FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @ProductConfiguration AND @ProductSettingId = ProductSettingId AND ThruDate IS NULL)
			BEGIN
				INSERT INTO enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				SELECT @ProductConfiguration, @ProductSettingId, GETUTCDATE(), NULL
			END	
		END
	END
END

GO

DECLARE @ProductConfiguration INT = 0, @ROLEID VARCHAR(10), @ProductId INT = 60, @ProductSettingId INT, @RoleName nVARCHAR(255) = 'Property Admin'

SELECT @ProductConfiguration = ConfigurationId FROM enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId AND ThruDate IS NULL
IF (@ProductConfiguration <> 0 )
BEGIN
	IF NOT EXISTS (SELECT TOP 1 (1) FROM enterprise.GlobalProductConfiguration gpc 
			INNER JOIN enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			INNER JOIN enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId 
			INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId 
		WHERE gpc.ProductId = @ProductId
	AND gpc.ThruDate IS NULL AND pc.ThruDate IS NULL
	AND pst.Name = 'SuperUserRoleId' )
	BEGIN
		SELECT TOP 1 @ROLEID = RoleId FROM security.Role WHERE ProductId = @ProductId AND OrgPartyID IS NULL AND RoleName = @RoleName
		IF @ROLEID <> 0
		BEGIN
			IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' )
			BEGIN
				INSERT INTO Enterprise.ProductSetting  ( ProductId, ProductSettingTypeId, Value, FromDate, ThruDate )
				SELECT @ProductId, ProductSettingTypeId, @ROLEID, GETUTCDATE(), NULL
					FROM Enterprise.ProductSettingType WHERE NAME = 'SuperUserRoleId'
			END
			SELECT @ProductSettingId = productsettingid FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' 
			IF NOT EXISTS (SELECT TOP 1 (1) FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @ProductConfiguration AND @ProductSettingId = ProductSettingId AND ThruDate IS NULL)
			BEGIN
				INSERT INTO enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				SELECT @ProductConfiguration, @ProductSettingId, GETUTCDATE(), NULL
			END	
		END
	END
END

GO

DECLARE @ProductConfiguration INT = 0, @ROLEID VARCHAR(10), @ProductId INT = 63, @ProductSettingId INT, @RoleName nVARCHAR(255) = 'Creator'

SELECT @ProductConfiguration = ConfigurationId FROM enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId AND ThruDate IS NULL
IF (@ProductConfiguration <> 0 )
BEGIN
	IF NOT EXISTS (SELECT TOP 1 (1) FROM enterprise.GlobalProductConfiguration gpc 
			INNER JOIN enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			INNER JOIN enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId 
			INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId 
		WHERE gpc.ProductId = @ProductId
	AND gpc.ThruDate IS NULL AND pc.ThruDate IS NULL
	AND pst.Name = 'SuperUserRoleId' )
	BEGIN
		SELECT TOP 1 @ROLEID = RoleId FROM security.Role WHERE ProductId = @ProductId AND OrgPartyID IS NULL AND RoleName = @RoleName
		IF @ROLEID <> 0
		BEGIN
			IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' )
			BEGIN
				INSERT INTO Enterprise.ProductSetting  ( ProductId, ProductSettingTypeId, Value, FromDate, ThruDate )
				SELECT @ProductId, ProductSettingTypeId, @ROLEID, GETUTCDATE(), NULL
					FROM Enterprise.ProductSettingType WHERE NAME = 'SuperUserRoleId'
			END
			SELECT @ProductSettingId = productsettingid FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' 
			IF NOT EXISTS (SELECT TOP 1 (1) FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @ProductConfiguration AND @ProductSettingId = ProductSettingId AND ThruDate IS NULL)
			BEGIN
				INSERT INTO enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				SELECT @ProductConfiguration, @ProductSettingId, GETUTCDATE(), NULL
			END	
		END
	END
END

GO

DECLARE @ProductConfiguration INT = 0, @ROLEID VARCHAR(10), @ProductId INT = 65, @ProductSettingId INT, @RoleName nVARCHAR(255) = 'Implementations'

SELECT @ProductConfiguration = ConfigurationId FROM enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId AND ThruDate IS NULL
IF (@ProductConfiguration <> 0 )
BEGIN
	IF NOT EXISTS (SELECT TOP 1 (1) FROM enterprise.GlobalProductConfiguration gpc 
			INNER JOIN enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			INNER JOIN enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId 
			INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId 
		WHERE gpc.ProductId = @ProductId
	AND gpc.ThruDate IS NULL AND pc.ThruDate IS NULL
	AND pst.Name = 'SuperUserRoleId' )
	BEGIN
		SELECT TOP 1 @ROLEID = RoleId FROM security.Role WHERE ProductId = @ProductId AND OrgPartyID IS NULL AND RoleName = @RoleName
		IF @ROLEID <> 0
		BEGIN
			IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' )
			BEGIN
				INSERT INTO Enterprise.ProductSetting  ( ProductId, ProductSettingTypeId, Value, FromDate, ThruDate )
				SELECT @ProductId, ProductSettingTypeId, @ROLEID, GETUTCDATE(), NULL
					FROM Enterprise.ProductSettingType WHERE NAME = 'SuperUserRoleId'
			END
			SELECT @ProductSettingId = productsettingid FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' 
			IF NOT EXISTS (SELECT TOP 1 (1) FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @ProductConfiguration AND @ProductSettingId = ProductSettingId AND ThruDate IS NULL)
			BEGIN
				INSERT INTO enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				SELECT @ProductConfiguration, @ProductSettingId, GETUTCDATE(), NULL
			END	
		END
	END
END

GO

DECLARE @ProductConfiguration INT = 0, @ROLEID VARCHAR(10), @ProductId INT = 70, @ProductSettingId INT, @RoleName nVARCHAR(255) = 'Portfolio Manager'

SELECT @ProductConfiguration = ConfigurationId FROM enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId AND ThruDate IS NULL
IF (@ProductConfiguration <> 0 )
BEGIN
	IF NOT EXISTS (SELECT TOP 1 (1) FROM enterprise.GlobalProductConfiguration gpc 
			INNER JOIN enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			INNER JOIN enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId 
			INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId 
		WHERE gpc.ProductId = @ProductId
	AND gpc.ThruDate IS NULL AND pc.ThruDate IS NULL
	AND pst.Name = 'SuperUserRoleId' )
	BEGIN
		SELECT TOP 1 @ROLEID = RoleId FROM security.Role WHERE ProductId = @ProductId AND OrgPartyID IS NULL AND RoleName = @RoleName
		IF @ROLEID <> 0
		BEGIN
			IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' )
			BEGIN
				INSERT INTO Enterprise.ProductSetting  ( ProductId, ProductSettingTypeId, Value, FromDate, ThruDate )
				SELECT @ProductId, ProductSettingTypeId, @ROLEID, GETUTCDATE(), NULL
					FROM Enterprise.ProductSettingType WHERE NAME = 'SuperUserRoleId'
			END
			SELECT @ProductSettingId = productsettingid FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' 
			IF NOT EXISTS (SELECT TOP 1 (1) FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @ProductConfiguration AND @ProductSettingId = ProductSettingId AND ThruDate IS NULL)
			BEGIN
				INSERT INTO enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				SELECT @ProductConfiguration, @ProductSettingId, GETUTCDATE(), NULL
			END	
		END
	END
END
GO

DECLARE @ProductConfiguration INT = 0, @ROLEID VARCHAR(10), @ProductId INT = 69, @ProductSettingId INT, @RoleName nVARCHAR(255) = 'Admin'

SELECT @ProductConfiguration = ConfigurationId FROM enterprise.GlobalProductConfiguration WHERE ProductId = @ProductId AND ThruDate IS NULL
IF (@ProductConfiguration <> 0 )
BEGIN
	IF NOT EXISTS (SELECT TOP 1 (1) FROM enterprise.GlobalProductConfiguration gpc 
			INNER JOIN enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			INNER JOIN enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId 
			INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId 
		WHERE gpc.ProductId = @ProductId
	AND gpc.ThruDate IS NULL AND pc.ThruDate IS NULL
	AND pst.Name = 'SuperUserRoleId' )
	BEGIN
		SELECT TOP 1 @ROLEID = r.RoleId FROM security.Role r INNER JOIN enterprise.Organization o ON o.PartyId = r.OrgPartyID INNER JOIN enterprise.Party p ON p.PartyId = o.PartyId WHERE r.RoleName = @RoleName AND ProductId = 69 AND p.RealPageId = '0D018E46-C20E-477D-ADED-4E5A35FB8F99'
		IF @ROLEID <> 0
		BEGIN
			IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' )
			BEGIN
				INSERT INTO Enterprise.ProductSetting  ( ProductId, ProductSettingTypeId, Value, FromDate, ThruDate )
				SELECT @ProductId, ProductSettingTypeId, @ROLEID, GETUTCDATE(), NULL
					FROM Enterprise.ProductSettingType WHERE NAME = 'SuperUserRoleId'
			END
			SELECT @ProductSettingId = productsettingid FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
				WHERE value = @roleId AND productid = @ProductId AND pst.Name = 'SuperUserRoleId' 
			IF NOT EXISTS (SELECT TOP 1 (1) FROM Enterprise.ProductConfiguration WHERE ConfigurationId = @ProductConfiguration AND @ProductSettingId = ProductSettingId AND ThruDate IS NULL)
			BEGIN
				INSERT INTO enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				SELECT @ProductConfiguration, @ProductSettingId, GETUTCDATE(), NULL
			END	
		END
	END
END
-- ADMIN ROLE FIXES
GO
