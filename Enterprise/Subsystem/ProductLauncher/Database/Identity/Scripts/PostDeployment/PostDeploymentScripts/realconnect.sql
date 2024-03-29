
DECLARE @UserId BIGINT, @Now DATETIME = GETUTCDATE()
DECLARE @Role1 int,@Role2 int,@Role3 int,@Role4 int,@Role5 INT, @OrgType1 INT,@OrgType2 INT
DECLARE @LearnerId INT, @ManagerId INT

SELECT TOP 1 @UserId = UserId
FROM   Ident.UserLogin
WHERE  LoginName LIKE '%@realpage.com'

IF NOT EXISTS(SELECT * FROM [Security].[Role] WHERE ShortName = 'student')
BEGIN
	INSERT [Security].[Role] ([RoleName], [ShortName], [Description], [RoleTypeID], [OrgPartyID], [ProductId], [CreatedBy], [CreatedDate]) 
	VALUES (N'Student', N'student', N'Student', 3, NULL, 94, @UserId, @Now)
END
IF NOT EXISTS(SELECT * FROM [Security].[Role] WHERE ShortName = 'sublicense-manager')
BEGIN
	INSERT [Security].[Role] ([RoleName], [ShortName], [Description], [RoleTypeID], [OrgPartyID], [ProductId], [CreatedBy], [CreatedDate]) 
	VALUES (N'Sublicense Manager', N'sublicense-manager', N'sublicense-manager', 3, NULL, 94, @UserId, @Now)
END

IF NOT EXISTS(SELECT * FROM [Security].[Role] WHERE ShortName = 'customer-instructor')
BEGIN
	INSERT [Security].[Role] ([RoleName], [ShortName], [Description], [RoleTypeID], [OrgPartyID], [ProductId], [CreatedBy], [CreatedDate]) 
	VALUES (N'Instructor', N'customer-instructor', N'customer-instructor', 3, NULL, 94, @UserId, @Now)
END

IF NOT EXISTS(SELECT * FROM [Security].[Role] WHERE ShortName = 'customer-admin')
BEGIN
	INSERT [Security].[Role] ([RoleName], [ShortName], [Description], [RoleTypeID], [OrgPartyID], [ProductId], [CreatedBy], [CreatedDate]) 
	VALUES (N'Admin', N'customer-admin', N'customer-admin', 3, NULL, 94, @UserId, @Now)
END

IF NOT EXISTS(SELECT * FROM [Security].[Role] WHERE ShortName = 'customer-reporting-only')
BEGIN
	INSERT [Security].[Role] ([RoleName], [ShortName], [Description], [RoleTypeID], [OrgPartyID], [ProductId], [CreatedBy], [CreatedDate]) 
	VALUES (N'Reports', N'customer-reporting-only', N'customer-reporting-only', 3, NULL, 94, @UserId, @Now)
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
	VALUES(N'LearnerId',1,NULL)
END
IF NOT EXISTS (SELECT * FROM Ident.SamlAttribute WHERE Name = 'ManagerId')
BEGIN
	INSERT INTO ident.SamlAttribute(Name,SamlAttributeTypeId,DisplayName)
	VALUES(N'ManagerId',1,NULL)
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