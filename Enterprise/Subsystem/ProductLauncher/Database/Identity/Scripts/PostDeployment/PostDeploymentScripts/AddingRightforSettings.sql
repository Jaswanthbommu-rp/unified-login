
-- Manage Setting.
 declare @rightId1 bigint,@rightId2 bigint,@rightId3 bigint,@rightId4 bigint,@rightId5 bigint,@rightId6 bigint,@rightId7 bigint,@rightId8 bigint;
 declare @MenuId1 Bigint,@MenuId2 Bigint,@MenuId3 Bigint,@MenuId4 Bigint;

select @rightId1 = RightId from Security.[Right] where Value = 'Internal Admin Access to Manage Best Practice Settings Templates';
select @rightId2 = RightId from Security.[Right] where Value = 'View all company-level settings';
select @rightId3 = RightId from Security.[Right] where Value = 'View all property-level settings';
select @rightId4 = RightId from Security.[Right] where Value = 'Manage property-level settings';

select @MenuId1 = Id from Enterprise.NavigationMenu where Title = 'Manage Settings' and Origin = 'unified-settings'

IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId1 and RightId in (@rightId1,@rightId2,@rightId3,@rightId4))
BEGIN
INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
                  VALUES(@MenuId1, @rightId1),(@MenuId1,@rightId2),(@MenuId1,@rightId3),(@MenuId1,@rightId4);
END

 -- Manage Template

select @rightId5 = RightId from Security.[Right] where Value = 'Manage Settings Templates & Global Updates';
select @rightId6 = RightId from Security.[Right] where Value = 'View Settings Templates';

select @MenuId2 = Id from Enterprise.NavigationMenu where Title = 'Manage Templates' and Origin = 'unified-settings';
 IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId2 and RightId in (@rightId5,@rightId6))
BEGIN
INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
                  VALUES(@MenuId2, @rightId5),(@MenuId2,@rightId6);
END

 --Settings Activity Log

select  @rightId7 = RightId from Security.[Right] where Value = 'Employee Access to Settings Admin Console';
select @MenuId3 = Id from Enterprise.NavigationMenu where Title = 'Settings Activity Log' and Origin = 'unified-settings'

IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId3 and RightId = @rightId7)
BEGIN
	INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
	VALUES(@MenuId3, @rightId7)
END
