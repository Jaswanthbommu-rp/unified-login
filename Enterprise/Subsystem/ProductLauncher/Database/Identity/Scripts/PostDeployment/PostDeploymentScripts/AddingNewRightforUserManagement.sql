-- Adding new right for User management in Realpage Employee company only
Go
DECLARE @UserId bigint
DECLARE @RightId bigint
DECLARE @RouteId bigint
SELECT @UserId = UserId
       FROM   Ident.UserLogin
       WHERE  LoginName LIKE 'realpagead@%'
IF NOT EXISTS (Select 1 From [Security].[Right] Where RightName = 'RealPageEmployeeUserManagement')
BEGIN
	INSERT INTO Security.[Right] (RightName,[Description],[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight)
	VALUES('RealPageEmployeeUserManagement','RealPage Employee Company User Management','RealPage Employee Company User Management',13,9,3,3,@UserId,GETUTCDATE(),0)
END
Select @RightId = RightId From [Security].[Right] Where RightName = 'RealPageEmployeeUserManagement'
Select @RouteId = RouteId From [Security].[Route] Where RouteValue = 'UsersList'

IF NOT EXISTS (Select 1 From [Security].[RoleRight] Where RoleId = 1 AND RightId = @RightId)
BEGIN
    INSERT INTO Security.RightRoute(RightId,RouteId,CreatedBy,CreatedDate)
	VALUES(@RightId,@RouteId,@UserId,GETUTCDATE())
END
Go