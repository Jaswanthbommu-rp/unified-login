DECLARE @createdate datetime, @CreatedBy bigint, @RightId INT, @RouteId INT, @NavigationMenuId INT;
select @createdate = GETUTCDATE()
SELECT  @CreatedBy = UserId
FROM  Ident.UserLogin
WHERE LoginName LIKE 'realpagead@%'
IF NOT EXISTS(SELECT TOP (1) 1 FROM [security].[Right] where RightName ='abilitytomanagemessagesonthebulletinboard')
BEGIN
  INSERT INTO [security].[Right] (RightName,[Description],[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
  VALUES('abilitytomanagemessagesonthebulletinboard','User will be able to navigate to the Bulletin Board from the left navigation and ass/edit/delete messages that post to the Bulleting Board','Ability to manage messages on the Bulletin Board',13,9,3,3,@CreatedBy,@createdate,0,0)
END
select @RightId = RightID from [security].[Right] where Rightname = 'abilitytomanagemessagesonthebulletinboard'
IF NOT EXISTS(SELECT TOP (1) 1 FROM [security].[RoleRight] where RoleId = 1 AND RightID = @RightId)
BEGIN
  INSERT INTO [security].RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
  VALUES(1,@RightID, @CreatedBy, @createdate)
END
SELECT @RouteId = RouteId FROM [security].[Route] WHERE RouteValue = 'SideMenu'
IF NOT EXISTS(SELECT TOP (1) 1 FROM [security].RightRoute where RightId = @RightId AND RouteId = @RouteId)
BEGIN
  INSERT INTO [security].RightRoute(RightId, RouteId, CreatedBy, CreatedDate)
  VALUES(@RightId,@RouteId, @CreatedBy, @createdate)
END

--Add right to sidemenu
IF NOT EXISTS(SELECT TOP (1) 1 FROM Enterprise.NavigationMenu where PageId = 'bulletin-board')
BEGIN
	INSERT INTO Enterprise.NavigationMenu(Title,PageId,Icon,[URL],OrderIndex,ParentId,Origin)
	VALUES('Bulletin Board','bulletin-board','chat-bubble-square-1','/home/notifications/bulletinboard',151,NULL,N'unified-login')
END
IF NOT EXISTS(SELECT TOP (1) 1 FROM Enterprise.NavigationMenuRights where RightId = @RightId)
BEGIN
	SELECT @NavigationMenuId=Id FROM Enterprise.NavigationMenu WHERE PageId = 'bulletin-board'
	INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
	VALUES(@NavigationMenuId, @RightId)
END