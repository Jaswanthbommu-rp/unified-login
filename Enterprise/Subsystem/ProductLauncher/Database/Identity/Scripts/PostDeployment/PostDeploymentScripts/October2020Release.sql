
/*This script is to create new right into security schema.*/
GO
 DECLARE @RightName nvarchar(200),
		 @RightDescription nvarchar(200),
		 @RightValue nvarchar(200),
		 @StatusTypeId int,
		 @OrgVisibilityStatusId INT = NULL,
		 @RightVisibilityStatusId INT =NULL,
		 @ProductId INT,
		 @TargetProductId int,
		 @UserId bigint,
		 @Now datetime = GETDATE(),
		 @RightId int,
		 @RouteId int,
		 @RouteName nvarchar(100),
		 @RoleId INT,
		 @OrgPartyId INT,
		 @ServerName SYSNAME = @@SERVERNAME;

DECLARE @TargetRoleName TABLE (RoleName nvarchar(100))
DECLARE @TargetOrganization TABLE (PartyId INT)
DECLARE @HoldRoleId TABLE (RoleId int)
DECLARE @HoldOrgPartyId TABLE (PartyId INT)
DECLARE @HoldRouteId TABLE (RouteId INT)

IF @ServerName IN ('RCDUSODBSQL001','RCTUSODBSQL001','RCQUSODBSQL001')
BEGIN

	SET @RightName = 'HelpCenterContactSupport'; 
	SET @RightDescription = 'Allows the user to create a Product Support request';
	SET @RightValue = 'Help Center Contact Support';
	SET @StatusTypeId = 13;
	SET @OrgVisibilityStatusId = 9;
	SET @RightVisibilityStatusId = 10;
	SET @ProductId =3;
	SET @TargetProductId = 3;
	SET @RouteName='RolesAndRights';
	INSERT INTO @TargetRoleName VALUES('Basic End User'),('Read Only for Unified Platform'),('User Administrator');
	INSERT INTO @TargetOrganization VALUES(350),(10728),(6967),(3),(7193),(51072),(12980),(21184);   

	--UserId
	SELECT	@UserId = UserId
	FROM	Ident.UserLogin
	WHERE	LoginName LIKE 'realpagead@%'

	--RouteId
	INSERT INTO @HoldRouteId
	SELECT RouteId 
	FROM [Security].[Route]
	WHERE RouteValue=@RouteName

	--RoleId
	INSERT INTO @HolDRoleId
	SELECT RoleId FROM [Security].[Role] 
	WHERE RoleName IN (SELECT RoleName FROM @TargetRoleName)
	AND ProductId=3 AND RoleTypeID=1

	--Org PartyId
	INSERT INTO @HoldOrgPartyId
	SELECT PartyId From [Enterprise].[Organization]
	WHERE PartyId IN (SELECT PartyId FROM @TargetOrganization)


	 IF NOT EXISTS
		(
			SELECT 1
			FROM [Security].[Right]
			WHERE RightName = @Rightname
		)
	BEGIN
			---Create Right
			INSERT INTO [Security].[Right]
				(	RightName,
					Description, 
					Value,
					StatusTypeId,
					VisibilityStatusId,
					ProductId,
					TargetProductId,
					CreatedBy,
					CreatedDate
				)
				VALUES ( 
						@RightName,
						@RightDescription,
						@RightValue,
						@StatusTypeId, 
						@RightVisibilityStatusId,
						@ProductId,
						@TargetProductId,
						@UserId,
						@Now
					   )

	END

	IF EXISTS
		(
			SELECT 1
			FROM [Security].[Right]
			WHERE RightName = @Rightname
		)
	BEGIN
			SELECT @RightId=RightId 
			FROM [Security].[Right]
			WHERE RightName=@RightName

				--Cursor Mapping Right with Route
				DECLARE curMapRightWithRoute CURSOR FOR
				SELECT RouteId
				FROM @HoldRouteId

				OPEN curMapRightWithRoute
				FETCH NEXT FROM curMapRightWithRoute INTO @RouteId

				WHILE @@FETCH_STATUS = 0
				BEGIN
					IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[RightRoute] WHERE RouteId = @RouteId AND RightId=@RightId)
					BEGIN
							INSERT INTO [Security].[RightRoute]
									  (	
										RightId,
										RouteId, 
										RightName,
										CreatedBy,
										CreatedDate
										)
							VALUES ( 
									@RightId,
									@RouteId,
									@RightName,
									@UserId,
									@Now
							)
					END;
					FETCH NEXT FROM curMapRightWithRoute INTO @RouteId
				END
				CLOSE curMapRightWithRoute
				DEALLOCATE curMapRightWithRoute

				--Cursor Mapping Role with Right
				DECLARE curMapRoleWithRight CURSOR FOR
				SELECT RoleId
				FROM @HolDRoleId

				OPEN curMapRoleWithRight
				FETCH NEXT FROM curMapRoleWithRight INTO @RoleId

				WHILE @@FETCH_STATUS = 0
				BEGIN
					IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[RoleRight] WHERE RoleId = @RoleId AND RightId=@RightId)
					BEGIN
						INSERT INTO [Security].[RoleRight]
						(	RoleId,
							RightId, 
							CreatedBy,
							CreatedDate
						)
						VALUES ( 
								@RoleId,
								@RightId,
								@UserId,
								@Now
							   )
					END;
					FETCH NEXT FROM curMapRoleWithRight INTO @RoleId
				END
				CLOSE curMapRoleWithRight
				DEALLOCATE curMapRoleWithRight

				--Cursor for Mapping Organization with Right
				DECLARE curMapOrganizationWithRight CURSOR FOR
				SELECT PartyId
				FROM @HoldOrgPartyId

				OPEN curMapOrganizationWithRight
				FETCH NEXT FROM curMapOrganizationWithRight INTO @OrgPartyId

				WHILE @@FETCH_STATUS = 0
				BEGIN
					IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[OrganizationOverRideRight] WHERE OrgPartyId = @OrgPartyId AND RightId=@RightId)
					BEGIN
						INSERT INTO [Security].[OrganizationOverRideRight]
						(	RightId,
							OrgPartyId, 
							VisibilityStatusId,
							CreatedBy,
							CreatedDate
						)
						VALUES ( 
								@RightId,
								@OrgPartyId,
								@OrgVisibilityStatusId,
								@UserId,
								@Now
							   )
					END;
					FETCH NEXT FROM curMapOrganizationWithRight INTO @OrgPartyId
				END
				CLOSE curMapOrganizationWithRight
				DEALLOCATE curMapOrganizationWithRight
		 
	END
END
GO
--ADD RIGHT
GO
DECLARE @CreatedById bigint,
		@RouteId bigint,
		@RightId bigint,
		@Now datetime = GETDATE(),
		@PartyId bigint,
		@RoleId bigint

SELECT @CreatedById = UserId
FROM Ident.UserLogin
WHERE LoginName = 'RealPageAd@test.com'

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
WHERE RoleName = 'User Administrator'

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
