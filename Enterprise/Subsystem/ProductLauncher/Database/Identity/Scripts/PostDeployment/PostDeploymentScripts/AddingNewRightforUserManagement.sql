-- Adding new right for User management in Realpage Employee company only
Go
DECLARE @UserId bigint
DECLARE @RightId bigint
DECLARE @RouteId bigint
DECLARE @OrgPartyId bigint
SELECT @UserId = UserId
       FROM   Ident.UserLogin
       WHERE  LoginName LIKE 'realpagead@%'
IF NOT EXISTS (Select 1 From [Security].[Right] Where RightName = 'RealPageEmployeeUserManagement')
BEGIN
	INSERT INTO Security.[Right] (RightName,[Description],[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight)
	VALUES('RealPageEmployeeUserManagement','RealPage Employee Company User Management','RealPage Employee Company User Management',13,10,3,3,@UserId,GETUTCDATE(),0)
END
Select @RightId = RightId From [Security].[Right] Where RightName = 'RealPageEmployeeUserManagement'
Select @RouteId = RouteId From [Security].[Route] Where RouteValue = 'UsersList'

IF NOT EXISTS (Select 1 From [Security].[RightRoute] Where RouteId = @RouteId AND RightId = @RightId)
BEGIN
    INSERT INTO Security.RightRoute(RightId,RouteId,CreatedBy,CreatedDate)
	VALUES(@RightId,@RouteId,@UserId,GETUTCDATE())
END

select @OrgPartyId = PartyId from Enterprise.Organization where Name like 'Realpage Employee'

IF NOT EXISTS (Select 1 From Security.OrganizationOverRideRight Where OrgPartyId = @OrgPartyId AND RightId = @RightId)
BEGIN
    INSERT INTO Security.OrganizationOverRideRight(RightId,OrgPartyId,VisibilityStatusId,CreatedBy,CreatedDate)
	VALUES(@RightId,@OrgPartyId,9,@UserId,GETUTCDATE())
END

Go
