GO

DECLARE @CreatedById bigint,
		@RouteId bigint,
		@RightId bigint,
		@Now datetime = GETDATE(),
		@PartyId bigint,
		@RoleId bigint

SELECT @CreatedById = UserId
FROM Ident.UserLogin
WHERE LoginName LIKE 'RealPageAd@test.com'

IF NOT EXISTS (SELECT 1 FROM [Security].[Right] WHERE RightName = 'EmployeeAccessToCompanySetup')
BEGIN
	INSERT INTO [Security].[Right] VALUES
	('EmployeeAccessToCompanySetup', 'Allow an authorized RealPage employee the ability to navigate the Configurations icon','Allow an authorized RealPage employee the ability to navigate the Configurations icon', 13,10, 3, 3, @CreatedById, @Now)
END

--RightRoute
SELECT @RightId = RightId
FROM [Security].[Right]
WHERE RightName = 'EmployeeAccessToCompanySetup'

SELECT @RouteId = RouteId
FROM [Security].[Route]
WHERE RouteValue = 'SideMenu'

IF NOT EXISTS (SELECT 1 FROM [Security].[RightRoute] WHERE RightId = @RightId AND RouteId = @RouteId)
BEGIN
	INSERT INTO [Security].[RightRoute] VALUES
	(@RightId, @RouteId, 'Employee Access to Company Setup', @CreatedById, @Now)
END
--RoleRight
SELECT @RoleId = RoleId 
FROM [Security].[Role]
WHERE RoleName like 'User Administrator'

IF NOT EXISTS (SELECT 1 FROM [Security].[RoleRight] WHERE RoleId = @RoleId AND RightId = @RightId)
BEGIN
	INSERT INTO [Security].[RoleRight] VALUES
	(@RoleId, @RightId, @CreatedById, @Now)
END

--OrganizationOverRideRight
SELECT @PartyId = PartyId
FROM [Enterprise].[Organization] 
WHERE [Name] = 'RealPage Employee'

IF NOT EXISTS (SELECT 1 FROM [Security].[OrganizationOverRideRight]  WHERE RightId = @RightId AND OrgPartyId = @PartyId)
BEGIN
	INSERT INTO [Security].[OrganizationOverRideRight] VALUES
	(@RightId, @PartyId, 9, @CreatedById, @Now)
END
GO

