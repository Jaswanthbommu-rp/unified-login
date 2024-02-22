DECLARE @UserId bigint

SELECT @UserId = UserId
FROM   Ident.UserLogin
WHERE  LoginName LIKE 'realpagead@%'

IF NOT EXISTS(SELECT TOP 1 1 FROM UserManagement.ControlType  where [Name] = 'Auto Assign')
BEGIN 
	INSERT INTO UserManagement.ControlType([Name], [Description] , CreatedBy, CreatedDate)
	VALUES('Auto Assign',	'Assign roles based on Role Type',	@UserId	,GETUTCDATE())
END

IF NOT EXISTS(SELECT TOP 1 1 FROM security.[RoleType] where [Value] = 'Admin')
BEGIN 
	INSERT INTO security.[RoleType](RoleTypeId,ParentRoleTypeId, [Value], [Description], [CreatedBy], [CreatedDate])
	VALUES(NULL,'Admin','Admin role', @UserId ,GETUTCDATE())
END
