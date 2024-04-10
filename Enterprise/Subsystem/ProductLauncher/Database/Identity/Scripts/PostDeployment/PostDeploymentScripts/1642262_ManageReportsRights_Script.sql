--User Story 1642262: UL/REP: Update Internal Admin Access to Manage Best Practice Report Groups (AD) right to access Manage Reports in left nav
Go
Declare @createdate datetime, @CreatedBy bigint, @RightId int, @PartyId int, @NavigationMenuId int;
select @createdate = GETUTCDATE()
SELECT	@CreatedBy = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

	
IF NOT EXISTS(SELECT TOP 1 1 FROM security.[Right] where value ='Internal Admin Access to Manage Best Practice Report Groups (AD)')
BEGIN
 INSERT INTO Security.[Right] VALUES('ManageBestPracticeReportGroupsAD','Internal Admin Access to Manage Best Practice Report Groups (AD)','Internal Admin Access to Manage Best Practice Report Groups (AD)',13,10,3,67,@CreatedBy,@createdate,0,0)
END

SELECT @RightId = RightId FROM security.[Right] where value ='Internal Admin Access to Manage Best Practice Report Groups (AD)'
SELECT @PartyId = PartyId  FROM Enterprise.Organization where name = 'Realpage Employee'

IF NOT EXISTS(SELECT TOP 1 1 FROM security.OrganizationOverRideRight where RightId = @RightId and OrgPartyId = @PartyId)
BEGIN
 	insert into Security.OrganizationOverRideRight VALUES(@RightId,@PartyId,9,@CreatedBy,@createdate)
END

select @NavigationMenuId = Id from Enterprise.NavigationMenu where Title = 'Manage Reports'

IF EXISTS(SELECT TOP 1 1 FROM enterprise.navigationmenurights where NavigationMenuId = @NavigationMenuId and RightId <> @RightId)
BEGIN
 	DELETE FROM enterprise.navigationmenurights where NavigationMenuId = @NavigationMenuId and RightId <> @RightId
END

IF NOT EXISTS(SELECT TOP 1 1 FROM enterprise.navigationmenurights where NavigationMenuId = @NavigationMenuId and RightId = @RightId)
BEGIN
 	INSERT INTO enterprise.navigationmenurights  VALUES(@NavigationMenuId,@RightId)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM security.RightRoute where RightId = @RightId)
BEGIN
 	INSERT INTO security.RightRoute values(@RightId,9,@CreatedBy,@createdate)
END

Go