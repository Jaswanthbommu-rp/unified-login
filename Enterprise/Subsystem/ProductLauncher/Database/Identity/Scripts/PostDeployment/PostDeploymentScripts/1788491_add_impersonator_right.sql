Go
Declare @createdate datetime, @CreatedBy bigint, @rightid bigint, @orgPartyId bigint

select @createdate = GETUTCDATE()
SELECT	@CreatedBy = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

select @orgPartyId = PartyId from Enterprise.Organization where Name = 'Realpage Employee'

IF NOT EXISTS ( select TOP 1 1 from security.[Right] where RightName = 'ImpersonateUser')
BEGIN
 INSERT INTO security.[Right] (RightName, [Description], [Value], StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate, PersistRight, IsExcludeRightFromImpersonation)
 VALUES('ImpersonateUser','Ability to Impersonate User','Ability to Impersonate User', 13, 9, 3, 3, @CreatedBy ,GETUTCDATE(),1,1)
END
select @rightid = RightId FROM Security.[Right] Where RightName = 'ImpersonateUser' 

IF NOT EXISTS ( select TOP 1 1 from security.RoleRight where RightId = @rightid)
BEGIN
  INSERT INTO Security.RightRoute(RightId, RouteId,CreatedBy,CreatedDate)
  VALUES(@rightid, 10, @CreatedBy, @createdate)
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Security.OrganizationOverRideRight where RightId = @rightid)
BEGIN
  INSERT INTO Security.OrganizationOverRideRight(RightId,OrgPartyId,VisibilityStatusId,CreatedBy,CreatedDate)
  VALUES(@rightid, @orgPartyId,9,@CreatedBy,@createdate)
END