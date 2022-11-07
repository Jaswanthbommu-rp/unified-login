declare @RightId int
declare @MenuId int

--task 1
select  @RightId = RightId from Security.[Right] where Value = 'Employee Access to Settings Admin Console'
select @MenuId = Id from Enterprise.NavigationMenu where Title = 'Settings Activity Log' and Origin = 'unified-settings'
select @RightId, @MenuId
IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId and RightId = @RightId)
BEGIN
	INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
	VALUES(@MenuId, @RightId)
END


--task 2
IF EXISTS (select top 1 1 from Security.[Right] where Value = 'Employee Access to Platform Settings and Service')
BEGIN
	update Security.[Right] set Value ='Employee Access to Platform Settings and Service' where Value = 'Employee Access to Login Page Setup'
END

select @RightId = RightId from Security.[Right] where Value = 'Internal Admin Access to Manage Best Practice Settings Templates'

select @MenuId = Id from Enterprise.NavigationMenu where Title = 'Manage Settings' and Origin = 'unified-settings'

IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId and RightId = @RightId)
BEGIN
INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
VALUES(@MenuId, @RightId)
END

select @MenuId = Id from Enterprise.NavigationMenu where Title = 'settings-activity' and Origin = 'unified-settings'
IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId and RightId = @RightId)
BEGIN
	INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
	VALUES(@MenuId, @RightId)
END
--task 3

IF EXISTS (select top 1 1 from Security.[Right] where Value = 'Employee Access to Platform Settings and Service')
BEGIN
update Security.[Right] set Value ='Internal Admin Access to Manage Best Practice Settings Templates' where Value = 'Employee Access to Manage Settings templates'
END

select @RightId = RightId from Security.[Right] where Value = 'Internal Admin Access to Manage Best Practice Settings Templates'
select @MenuId = Id from Enterprise.NavigationMenu where Title = 'Manage Templates' and Origin = 'unified-settings'

IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId and RightId = @RightId)
BEGIN
	INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
	VALUES(@MenuId, @RightId)
END

--task4
select   @RightId = RightId from Security.[Right] where Value ='View Settings Templates'
select @MenuId = Id from Enterprise.NavigationMenu where Title = 'Manage Templates' and Origin = 'unified-settings'

IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId and RightId = @RightId)
BEGIN
	INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
	VALUES(@MenuId, @RightId)
END
--task 5 (No need as of now)
--task 6
IF NOT EXISTS(select top 1 1 from Security.[Right] where Value = 'View Settings Templates')
BEGIN
	INSERT INTO Security.[Right](RightName, [Description], [Value], StatusTypeId, VisibilityStatusId, ProductId, TargetProductId,CreatedBy,CreatedDate,PersistRight)
	values('ViewSettingsTemplates',	'This right allows a user access to Settings > Manage Templates to view templates.',	'View Settings Templates',	13,	9,	3,	56,	4325,GETUTCDATE(),0)
END


select   @RightId = RightId from Security.[Right] where Value ='View Settings Templates'

select @MenuId = Id from Enterprise.NavigationMenu where Title = 'Manage Templates' and Origin = 'unified-settings'
IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId and RightId = @RightId)
BEGIN
INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
VALUES(@MenuId, @RightId)
END

