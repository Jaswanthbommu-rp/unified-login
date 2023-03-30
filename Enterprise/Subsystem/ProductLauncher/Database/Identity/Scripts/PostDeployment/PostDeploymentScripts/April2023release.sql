--User Story 1453155
IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenu WHERE PageId = 'Report Activity Log')
BEGIN 
	BEGIN TRAN
	DECLARE @parentId int;
	SELECT TOP 1 @parentId = Id FROM Enterprise.NavigationMenu WHERE PageId = N'reporting';
	DECLARE @menuEntryId int;
	INSERT INTO Enterprise.NavigationMenu(Title, PageId, Icon, [URL], OrderIndex, ParentId, Origin)
	VALUES (N'Report Activity Log', N'Report Activity Log', NULL, '/reporting/activity-log', 220, @parentId, 'unified-reporting');
	SET @menuEntryId = SCOPE_IDENTITY();
	INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
	SELECT @menuEntryId, RightId FROM [Security].[Right] WHERE RightName = 'AccessUnifiedReporting'
	COMMIT TRAN
END
GO