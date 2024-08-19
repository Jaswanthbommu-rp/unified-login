Go
Declare @creatuserrightId bigint, @viewuserrightid bigint, @empusermgmtrightId bigint
Declare @createdate datetime, @CreatedBy bigint
Declare @adduserrouteid int, @edituserrouteid int

select @createdate = GETUTCDATE()
SELECT	@CreatedBy = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

select @creatuserrightId = RightId  from Security.[Right] where RightName ='CreateUser'
select @viewuserrightid = RightId from Security.[Right] where RightName ='ViewUsers'
select @empusermgmtrightId = RightId from Security.[Right] where RightName ='RealPageEmployeeUserManagement'

select @adduserrouteid = RouteId  from Security.Route where RouteValue = 'AddUser'
select @edituserrouteid = RouteId  from Security.Route where RouteValue = 'EditUser'

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @empusermgmtrightId)
BEGIN
	INSERT INTO Enterprise.NavigationMenuRights (NavigationMenuId, RightId)
	select NavigationMenuId, @empusermgmtrightId from Enterprise.NavigationMenuRights where RightId = @viewuserrightid
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Security.RightRoute where RightId = @empusermgmtrightId and RouteId in (@adduserrouteid, @edituserrouteid))
BEGIN
	INSERT INTO Security.RightRoute(RightId, RouteId, CreatedBy, CreatedDate)
	VALUES(@empusermgmtrightId, @adduserrouteid, @createdby, @createdate)

	INSERT INTO Security.RightRoute(RightId, RouteId, CreatedBy, CreatedDate)
	VALUES(@empusermgmtrightId, @edituserrouteid, @createdby, @createdate)
	
END
Go