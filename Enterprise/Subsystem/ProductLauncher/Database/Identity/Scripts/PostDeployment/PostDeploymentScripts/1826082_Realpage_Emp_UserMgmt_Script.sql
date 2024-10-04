Go
Declare @createdate datetime, @CreatedBy bigint, @rightid bigint, @orgPartyId bigint

select @createdate = GETUTCDATE()
SELECT	@CreatedBy = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

select @orgPartyId = PartyId from Enterprise.Organization where Name = 'Realpage Employee'

IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[Right] Where RightName = 'RealPageEmployeeUserManagementViewOnly')
BEGIN
 INSERT INTO Security.[Right] (RightName,[Description],[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
 VALUES('RealPageEmployeeUserManagementViewOnly','RealPage Employee Company User Management: View Only','RealPage Employee Company User Management: View Only',13,9,3,3,@CreatedBy,@createdate,0,0)
END

select @rightid = RightId FROM Security.[Right] Where RightName = 'RealPageEmployeeUserManagementViewOnly' 

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights (NavigationMenuId, RightId)
 VALUES(2,@rightid),
 (3,@rightid)
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Security.RightRoute where RightId = @rightid)
BEGIN
 INSERT INTO Security.RightRoute(RightId, RouteId,CreatedBy,CreatedDate)
 VALUES(@rightid, 2, @CreatedBy, @createdate),
 (@rightid, 5, @CreatedBy, @createdate),
 (@rightid, 9, @CreatedBy, @createdate),
 (@rightid, 12, @CreatedBy, @createdate)
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Security.OrganizationOverRideRight where RightId = @rightid)
BEGIN
  INSERT INTO Security.OrganizationOverRideRight(RightId,OrgPartyId,VisibilityStatusId,CreatedBy,CreatedDate)
  VALUES(@rightid, @orgPartyId,9,@CreatedBy,@createdate)
END

Go