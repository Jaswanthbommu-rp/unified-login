--User Story 1971650: RealConnect - Add a new Right called "Manage RealConnect Product Access"

GO
Declare @createdate datetime, @CreatedBy bigint, @RightId int;
select @createdate = GETUTCDATE()
SELECT  @CreatedBy = UserId
FROM  Ident.UserLogin
WHERE LoginName LIKE 'realpagead@%'
IF NOT EXISTS(SELECT TOP 1 1 FROM security.[Right] where RightName ='ManageRealConnectProductAccess')
BEGIN
  INSERT INTO Security.[Right] (RightName,[Description],  [Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
  VALUES('ManageRealConnectProductAccess','Manage RealConnect Product Access','Manage RealConnect Product Access',13,9,3,94,@CreatedBy,@createdate,0,0)
END
select @RightId = RightID from Security.[Right] where Rightname = 'ManageRealConnectProductAccess'
IF NOT EXISTS(SELECT TOP 1 1 FROM security.[RoleRight] where RoleId = 1 AND RightID = @RightId)
BEGIN
  INSERT INTO Security.RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
  VALUES(1,@RightID, @CreatedBy, @createdate)
END
IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.ProductRight where ProductId = 97 AND RightShortName = 'ManageRealConnectProductAccess')
BEGIN
  INSERT INTO Enterprise.ProductRight(ProductId, RightShortName,DependantProductId)
  VALUES(97,'ManageRealConnectProductAccess',NULL)
END
Go