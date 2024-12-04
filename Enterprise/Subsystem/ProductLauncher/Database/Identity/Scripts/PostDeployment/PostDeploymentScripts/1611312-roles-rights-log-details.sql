
IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenu WHERE PageId = 'Roles & Rights Activity Log')
BEGIN 
	DECLARE @parentId int;
	SELECT TOP 1 @parentId = Id FROM Enterprise.NavigationMenu WHERE PageId = N'rolesRights';
	DECLARE @menuEntryId int;
	INSERT INTO Enterprise.NavigationMenu(Title, PageId, Icon, [URL], OrderIndex, ParentId, Origin)
	VALUES (N'Roles & Rights Activity Log', N'Roles & Rights Activity Log', '', '/home/roles-rights-activity-log', 71, @parentId, 'unified-login');
	SET @menuEntryId = SCOPE_IDENTITY();
	INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
	SELECT @menuEntryId, RightId FROM [Security].[Right] WHERE RightName = 'ViewRoleRight'
END
GO
