GO

DECLARE @UserId bigint

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS ( SELECT TOP(1) 1 FROM security.Role WHERE ProductId = 39 AND RoleName = 'Integration Viewer' AND ShortName = 'Role-IntVwr' )
BEGIN
	INSERT INTO security.Role ( RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
	VALUES ( N'Integration Viewer', 'Role-IntVwr', 'Integration Viewer', 3, NULL, 39, @UserId, GETUTCDATE() )
END
IF NOT EXISTS ( SELECT TOP(1) 1 FROM security.Role WHERE ProductId = 39 AND RoleName = 'Integration Manager' AND ShortName = 'Role-IntMgr' )
BEGIN
	INSERT INTO security.Role ( RoleName,ShortName,Description,RoleTypeID,OrgPartyID,ProductId,CreatedBy,CreatedDate)
		VALUES ( N'Integration Manager', 'Role-IntMgr', 'Integration Manager', 3, NULL, 39, @UserId, GETUTCDATE() )
END

IF NOT EXISTS (SELECT 1 FROM [Security].[Right] WHERE RightName = 'Role-IntVwr')
BEGIN
	INSERT INTO [Security].[Right](	RightName,Description, Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,	CreatedBy,CreatedDate)
    VALUES ('Role-IntVwr', 'Integration Viewer','Integration Viewer', 13,9, 39, 39, @UserId, GETUTCDATE())
END

IF NOT EXISTS (SELECT 1 FROM [Security].[Right] WHERE RightName = 'Role-IntMgr')
BEGIN
	INSERT INTO [Security].[Right](	RightName,Description, Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,	CreatedBy,CreatedDate)
    VALUES ('Role-IntMgr', 'Integration Manager','Integration Manager', 13,9, 39, 39, @UserId, GETUTCDATE())
END

IF NOT EXISTS ( SELECT 1 FROM [Security].RoleRight RR 
					INNER JOIN Security.Role R ON R.RoleId = RR.RoleId 
					INNER JOIN security.[Right] R2 ON R2.RightId = RR.RightId 
				WHERE R.ShortName = 'Role-IntVwr' AND R.ProductId = 39 AND R.OrgPartyID IS NULL AND r2.RightName = 'Role-IntVwr' AND R2.ProductId = 39 )
BEGIN
	INSERT INTO SECURITY.RoleRight ( RoleId, RightId, CreatedBy, CreatedDate )
	SELECT r.RoleId, R2.RightId, @UserId, GETUTCDATE() 
		FROM Security.Role R CROSS JOIN security.[Right] R2 
	WHERE R.ShortName = 'Role-IntVwr' AND R.ProductId = 39 AND R.OrgPartyID IS NULL AND r2.RightName = 'Role-IntVwr' AND R2.ProductId = 39
END

IF NOT EXISTS ( SELECT 1 FROM [Security].RoleRight RR 
					INNER JOIN Security.Role R ON R.RoleId = RR.RoleId 
					INNER JOIN security.[Right] R2 ON R2.RightId = RR.RightId 
				WHERE R.ShortName = 'Role-IntMgr' AND R.ProductId = 39 AND R.OrgPartyID IS NULL AND r2.RightName = 'Role-IntMgr' AND R2.ProductId = 39 )
BEGIN
	INSERT INTO SECURITY.RoleRight ( RoleId, RightId, CreatedBy, CreatedDate )
	SELECT r.RoleId, R2.RightId, @UserId, GETUTCDATE() 
		FROM Security.Role R CROSS JOIN security.[Right] R2 
	WHERE R.ShortName = 'Role-IntMgr' AND R.ProductId = 39 AND R.OrgPartyID IS NULL AND r2.RightName = 'Role-IntMgr' AND R2.ProductId = 39
END

GO

;WITH oldimrole ( personaid, oldvalue ) AS (
	SELECT personaid, CASE WHEN value = 'Role-IntMgr' THEN 'Role-IntMgr' ELSE CASE WHEN value = 'Role-IntVwr' THEN 'Role-IntVwr' ELSE 'Role-IntVwr' end end FROM ident.SamlUserAttribute sua INNER JOIN ident.SamlAttribute sa ON sa.SamlAttributeId = sua.SamlAttributeId WHERE sa.Name = 'RoleCode' AND sua.ProductId = 39 
)
, newpersonarole ( personaid, roleid )
	AS ( SELECT personaid, roleid FROM oldimrole om INNER JOIN security.Role R ON om.oldvalue = r.ShortName AND r.ProductId = 39 )
INSERT INTO Security.PersonaRole (PersonaId,RoleId,FromDate,ThruDate,CreatedBy,CreatedDate)
	SELECT np.personaid, np.roleid, GETUTCDATE(), NULL, 480, GETUTCDATE() FROM newpersonarole np LEFT JOIN security.PersonaRole pr ON pr.RoleId = np.roleid AND pr.PersonaId = np.personaid
		WHERE pr.PersonaId IS NULL AND pr.RoleId IS NULL

GO

-- UPDATE THE PRODUCT SETTING
IF NOT EXISTS (
	SELECT TOP (1) 1 FROM enterprise.GlobalProductConfiguration gpc 
		INNER JOIN enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
		INNER JOIN enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId 
		INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
	WHERE gpc.ProductId = 39
	AND gpc.ThruDate IS NULL
	AND pc.ThruDate IS NULL
	AND ps.ThruDate IS null
	AND pst.Name = 'ProductIntegrationType' AND ps.VALUE = 'UPFM' )
BEGIN
	IF EXISTS (
		SELECT TOP (1) 1 FROM enterprise.GlobalProductConfiguration gpc 
			INNER JOIN enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			INNER JOIN enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId 
			INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
		WHERE gpc.ProductId = 39
		AND gpc.ThruDate IS NULL
		AND pc.ThruDate IS NULL
		AND ps.ThruDate IS null
		AND pst.Name = 'ProductIntegrationType' AND ps.VALUE = 'Legacy')
	BEGIN
		DECLARE @ProductSettingId INT
		SELECT TOP (1) @ProductSettingId = PS.ProductSettingId FROM enterprise.GlobalProductConfiguration gpc 
			INNER JOIN enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId
			INNER JOIN enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId 
			INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
			WHERE gpc.ProductId = 39
			AND gpc.ThruDate IS NULL
			AND pc.ThruDate IS NULL
			AND ps.ThruDate IS null
			AND pst.Name = 'ProductIntegrationType' AND ps.VALUE = 'Legacy'
		ORDER BY PC.ProductConfigurationId DESC
        UPDATE Enterprise.ProductSetting SET value = 'UPFM' WHERE ProductSettingId = @ProductSettingId AND value = 'Legacy'
	END
END

GO

