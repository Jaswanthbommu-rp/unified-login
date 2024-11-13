--User Story 1887331: RealConnect - Add a Rights called Access to RealConnect Administration Tool.

Go
DECLARE @UserId bigint, @RightId bigint, @OrgPartyId bigint

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT TOP (1) 1 FROM security.[Right] where Rightname = 'AccessToRealConnectAdministrationTool')
BEGIN
	INSERT INTO security.[Right] (RightName	,[Description],	[Value],	StatusTypeId,	VisibilityStatusId,	ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
	VALUES('AccessToRealConnectAdministrationTool','Access to RealConnect Administration Tool',	'Access to RealConnect Administration Tool',11,10,3,3,@UserId,GETUTCDATE(),0,0)
END


SELECT @RightId = RightID from security.[Right] where Rightname = 'AccessToRealConnectAdministrationTool'
SELECT @OrgPartyId = PartyId from Enterprise.Organization where Name = 'Realpage Employee'
	
IF NOT EXISTS (SELECT TOP (1) 1 FROM security.OrganizationOverRideRight where RightId = @RightId AND OrgPartyId = @OrgPartyId)
BEGIN
	INSERT INTO Security.OrganizationOverRideRight(RightId,OrgPartyId,VisibilityStatusId,CreatedBy,CreatedDate)
	VALUES(@RightId , @OrgPartyId,9,@UserId,GETUTCDATE())

END

IF NOT EXISTS (SELECT TOP (1) 1 FROM security.RoleRight where RightId = @RightId AND RoleId = 1)
BEGIN
	INSERT INTO Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
	VALUES(1, @RightId, @UserId,GETUTCDATE())
END
Go