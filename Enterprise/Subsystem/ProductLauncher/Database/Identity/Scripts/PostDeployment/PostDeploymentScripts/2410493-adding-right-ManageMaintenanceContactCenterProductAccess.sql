--User Story 2410493 : Add New Right for "Manage Contact Center Maintenance Product Access 2.0"

DECLARE @UserId bigint, @RightId bigint

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT TOP 1 1 FROM security.[Right] where Rightname = 'ManageMaintenanceContactCenterProductAccess')
BEGIN
	INSERT INTO security.[Right] (RightName	,[Description],	[Value],	StatusTypeId,	VisibilityStatusId,	ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
	VALUES('ManageMaintenanceContactCenterProductAccess','Manage Contact Center Maintenance Product Access',	'Manage Contact Center Maintenance Product Access 2.0',13,9,3,105,@UserId,GETUTCDATE(),0,0)
	
END


SELECT @RightId = RightID from security.[Right] where Rightname = 'ManageMaintenanceContactCenterProductAccess'

IF NOT EXISTS (SELECT TOP 1 1 FROM security.RoleRight where RightId = @RightId AND RoleId = 1)
BEGIN
	INSERT INTO Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	VALUES(1, @RightId, @UserId,GETUTCDATE())
END
Go
