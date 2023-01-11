

GO
-- Manage Setting.
 declare @rightId1 bigint,@rightId2 bigint,@rightId3 bigint,@rightId4 bigint,@rightId5 bigint,@rightId6 bigint,@rightId7 bigint,@rightId8 bigint;
 declare @MenuId1 Bigint,@MenuId2 Bigint,@MenuId3 Bigint,@MenuId4 Bigint;

select @rightId1 = RightId from Security.[Right] where RightName = 'EmployeeAccesstoManageSettingsTemplates';
select @rightId2 = RightId from Security.[Right] where  RightName = 'Viewallcompanylevelsettings';
select @rightId3 = RightId from Security.[Right] where RightName = 'Viewallpropertylevelsettings';
select @rightId4 = RightId from Security.[Right] where RightName = 'Managepropertylevelsettings';

select @MenuId1 = Id from Enterprise.NavigationMenu where Title = 'Manage Settings' and Origin = 'unified-settings'

IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId1 and RightId in (@rightId1,@rightId2,@rightId3,@rightId4))
BEGIN
INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
                  VALUES(@MenuId1, @rightId1),(@MenuId1,@rightId2),(@MenuId1,@rightId3),(@MenuId1,@rightId4);
END

 -- Manage Template

select @rightId5 = RightId from Security.[Right] where RightName = 'ManageSettingsTemplates'
select @rightId6 = RightId from Security.[Right] where RightName = 'ViewSettingsTemplates';

select @MenuId2 = Id from Enterprise.NavigationMenu where Title = 'Manage Templates' and Origin = 'unified-settings';
 IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId2 and RightId in (@rightId5,@rightId6))
BEGIN
INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
                  VALUES(@MenuId2, @rightId5),(@MenuId2,@rightId6);
END

 --Settings Activity Log

select  @rightId7 = RightId from Security.[Right] where RightName = 'AccessSettingsAdmin';
select @MenuId3 = Id from Enterprise.NavigationMenu where Title = 'Settings Activity Log' and Origin = 'unified-settings'

IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId3 and RightId = @rightId7)
BEGIN
	INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
	VALUES(@MenuId3, @rightId7)
END
GO