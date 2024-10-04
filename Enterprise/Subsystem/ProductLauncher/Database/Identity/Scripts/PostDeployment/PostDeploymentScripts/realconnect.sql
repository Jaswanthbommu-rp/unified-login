Go
DECLARE @UserId BIGINT, @Now DATETIME = GETUTCDATE()
DECLARE @Role1 int,@Role2 int,@Role3 int,@Role4 int,@Role5 INT, @OrgType1 INT,@OrgType2 INT
DECLARE @LearnerId INT, @ManagerId INT

SELECT TOP 1 @UserId = UserId
FROM   Ident.UserLogin
WHERE  LoginName LIKE '%@realpage.com'

IF NOT EXISTS(SELECT * FROM [Security].[Role] WHERE ShortName = 'student' AND ProductId = 94)
BEGIN
	INSERT [Security].[Role] ([RoleName], [ShortName], [Description], [RoleTypeID], [OrgPartyID], [ProductId], [CreatedBy], [CreatedDate]) 
	VALUES (N'Student', N'student', N'The student role is enabled by default for all users in RealConnect to be able to launch courses.', 3, NULL, 94, @UserId, @Now)
END
IF NOT EXISTS(SELECT * FROM [Security].[Role] WHERE ShortName = 'sublicense-manager' AND ProductId = 94)
BEGIN
	INSERT [Security].[Role] ([RoleName], [ShortName], [Description], [RoleTypeID], [OrgPartyID], [ProductId], [CreatedBy], [CreatedDate]) 
	VALUES (N'Sublicense Manager', N'sublicense-manager', N'The Sublicense Manager role grants a user access over selected sublicense(s) (group of users).', 3, NULL, 94, @UserId, @Now)
END

IF NOT EXISTS(SELECT * FROM [Security].[Role] WHERE ShortName = 'customer-instructor' AND ProductId = 94)
BEGIN
	INSERT [Security].[Role] ([RoleName], [ShortName], [Description], [RoleTypeID], [OrgPartyID], [ProductId], [CreatedBy], [CreatedDate]) 
	VALUES (N'Instructor', N'customer-instructor', N'The Instructor role is used to allow users to create and manage live sessions that users can attend.', 3, NULL, 94, @UserId, @Now)
END

IF NOT EXISTS(SELECT * FROM [Security].[Role] WHERE ShortName = 'customer-admin' AND ProductId = 94)
BEGIN
	INSERT [Security].[Role] ([RoleName], [ShortName], [Description], [RoleTypeID], [OrgPartyID], [ProductId], [CreatedBy], [CreatedDate]) 
	VALUES (N'Admin', N'customer-admin', N'The Admin role is used to administrate all aspects of RealConnect including users, courses, enrollments, and run reports.', 3, NULL, 94, @UserId, @Now)
END

IF NOT EXISTS(SELECT * FROM [Security].[Role] WHERE ShortName = 'customer-reporting-only' AND ProductId = 94)
BEGIN
	INSERT [Security].[Role] ([RoleName], [ShortName], [Description], [RoleTypeID], [OrgPartyID], [ProductId], [CreatedBy], [CreatedDate]) 
	VALUES (N'Reporting Only', N'customer-reporting-only', N'The Reporting Only role is used to allow users to run reports without having access to change RealConnect settings.', 3, NULL, 94, @UserId, @Now)
END

SELECT @Role1=RoleId FROM Security.Role WHERE ShortName = 'student'
SELECT @Role2=RoleId FROM Security.Role WHERE ShortName = 'sublicense-manager'
SELECT @Role3=RoleId FROM Security.Role WHERE ShortName = 'customer-instructor'
SELECT @Role4=RoleId FROM Security.Role WHERE ShortName = 'customer-admin'
SELECT @Role5=RoleId FROM Security.Role WHERE ShortName = 'customer-reporting-only'

SELECT @OrgType1 = OrganizationTypeId FROM Enterprise.OrganizationType WHERE Name = 'Multifamily'
SELECT @OrgType2 = OrganizationTypeId FROM Enterprise.OrganizationType WHERE Name = 'Other'

IF NOT EXISTS(SELECT * FROM Security.RoleOrganizationType WHERE RoleId = @Role1 AND OrganizationTypeId = @OrgType1)
BEGIN
	INSERT INTO Security.RoleOrganizationType(RoleId,OrganizationTypeId,CreatedBy,CreatedDate)
	VALUES(@Role1,@OrgType1,@UserId,DEFAULT)
END
IF NOT EXISTS(SELECT * FROM Security.RoleOrganizationType WHERE RoleId = @Role2 AND OrganizationTypeId = @OrgType1)
BEGIN
	INSERT INTO Security.RoleOrganizationType(RoleId,OrganizationTypeId,CreatedBy,CreatedDate)
	VALUES(@Role2,@OrgType1,@UserId,DEFAULT)
END
IF NOT EXISTS(SELECT * FROM Security.RoleOrganizationType WHERE RoleId = @Role3 AND OrganizationTypeId = @OrgType1)
BEGIN
	INSERT INTO Security.RoleOrganizationType(RoleId,OrganizationTypeId,CreatedBy,CreatedDate)
	VALUES(@Role3,@OrgType1,@UserId,DEFAULT)
END
IF NOT EXISTS(SELECT * FROM Security.RoleOrganizationType WHERE RoleId = @Role4 AND OrganizationTypeId = @OrgType1)
BEGIN
	INSERT INTO Security.RoleOrganizationType(RoleId,OrganizationTypeId,CreatedBy,CreatedDate)
	VALUES(@Role4,@OrgType1,@UserId,DEFAULT)
END
IF NOT EXISTS(SELECT * FROM Security.RoleOrganizationType WHERE RoleId = @Role5 AND OrganizationTypeId = @OrgType1)
BEGIN
	INSERT INTO Security.RoleOrganizationType(RoleId,OrganizationTypeId,CreatedBy,CreatedDate)
	VALUES(@Role5,@OrgType1,@UserId,DEFAULT)
END

IF NOT EXISTS(SELECT * FROM Security.RoleOrganizationType WHERE RoleId = @Role1 AND OrganizationTypeId = @OrgType2)
BEGIN
	INSERT INTO Security.RoleOrganizationType(RoleId,OrganizationTypeId,CreatedBy,CreatedDate)
	VALUES(@Role1,@OrgType2,@UserId,DEFAULT)
END
IF NOT EXISTS(SELECT * FROM Security.RoleOrganizationType WHERE RoleId = @Role2 AND OrganizationTypeId = @OrgType2)
BEGIN
	INSERT INTO Security.RoleOrganizationType(RoleId,OrganizationTypeId,CreatedBy,CreatedDate)
	VALUES(@Role2,@OrgType2,@UserId,DEFAULT)
END
IF NOT EXISTS(SELECT * FROM Security.RoleOrganizationType WHERE RoleId = @Role3 AND OrganizationTypeId = @OrgType2)
BEGIN
	INSERT INTO Security.RoleOrganizationType(RoleId,OrganizationTypeId,CreatedBy,CreatedDate)
	VALUES(@Role3,@OrgType2,@UserId,DEFAULT)
END
IF NOT EXISTS(SELECT * FROM Security.RoleOrganizationType WHERE RoleId = @Role4 AND OrganizationTypeId = @OrgType2)
BEGIN
	INSERT INTO Security.RoleOrganizationType(RoleId,OrganizationTypeId,CreatedBy,CreatedDate)
	VALUES(@Role4,@OrgType2,@UserId,DEFAULT)
END
IF NOT EXISTS(SELECT * FROM Security.RoleOrganizationType WHERE RoleId = @Role5 AND OrganizationTypeId = @OrgType2)
BEGIN
	INSERT INTO Security.RoleOrganizationType(RoleId,OrganizationTypeId,CreatedBy,CreatedDate)
	VALUES(@Role5,@OrgType2,@UserId,DEFAULT)
END


IF NOT EXISTS (SELECT * FROM Ident.SamlAttribute WHERE Name = 'LearnerId')
BEGIN
	INSERT INTO ident.SamlAttribute(Name,SamlAttributeTypeId,DisplayName)
	VALUES(N'LearnerId',1,N'LearnerId')
END
IF NOT EXISTS (SELECT * FROM Ident.SamlAttribute WHERE Name = 'ManagerId')
BEGIN
	INSERT INTO ident.SamlAttribute(Name,SamlAttributeTypeId,DisplayName)
	VALUES(N'ManagerId',1,N'ManagerId')
END

SELECT @LearnerId = SamlAttributeId FROM ident.SamlAttribute WHERE Name='LearnerId'
SELECT @ManagerId = SamlAttributeId FROM ident.SamlAttribute WHERE Name='ManagerId'

IF NOT EXISTS(SELECT * FROM Ident.SamlProductAttribute WHERE ProductId = 94 AND SamlAttributeId = @LearnerId)
BEGIN
	INSERT INTO ident.SamlProductAttribute(ProductId,SamlAttributeId)
	VALUES(94,@LearnerId)
END

IF NOT EXISTS(SELECT * FROM Ident.SamlProductAttribute WHERE ProductId = 94 AND SamlAttributeId = @ManagerId)
BEGIN
	INSERT INTO ident.SamlProductAttribute(ProductId,SamlAttributeId)
	VALUES(94,@ManagerId)
END

IF NOT EXISTS(SELECT * FROM Ident.samlProductAttribute WHERE ProductId = 94 AND SamlAttributeId = 1)
BEGIN
	INSERT INTO Ident.SamlProductAttribute(ProductId,SamlAttributeId)
	VALUES(94,1)
END
GO

DECLARE @DualRole INT

IF NOT EXISTS (SELECT * FROM Ident.SamlAttribute WHERE Name = 'DualRole')
BEGIN
	INSERT INTO ident.SamlAttribute(Name,SamlAttributeTypeId,DisplayName)
	VALUES(N'DualRole',1,N'DualRole')
END

SELECT @DualRole = SamlAttributeId FROM ident.SamlAttribute WHERE Name='DualRole'

IF NOT EXISTS(SELECT * FROM Ident.SamlProductAttribute WHERE ProductId = 94 AND SamlAttributeId = @DualRole)
BEGIN
	INSERT INTO ident.SamlProductAttribute(ProductId,SamlAttributeId)
	VALUES(94,@DualRole)
END

GO
--Data fix for existing personas to add DualRole attribute who has ManagerId attribute only
WITH dualRoleUsers
AS
(SELECT PersonaId, COUNT(SamlAttributeId) AS samlattrcnt FROM Ident.SamlUserAttribute WHERE ProductId=94
GROUP BY PersonaId
HAVING COUNT(SamlAttributeId) =3)
--SELECT * FROM dualRoleUsers
INSERT INTO Ident.SamlUserAttribute(PersonaId,ProductId,SamlAttributeId,Value,FromDate,ThruDate)
SELECT dualRoleUsers.PersonaId,94,18,'true',GETUTCDATE(),null FROM dualRoleUsers

GO