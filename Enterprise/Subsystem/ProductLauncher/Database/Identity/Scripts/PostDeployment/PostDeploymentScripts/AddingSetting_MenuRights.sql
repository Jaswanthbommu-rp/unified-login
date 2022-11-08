declare @RightId int
declare @MenuId int
--Setting 1
select  @RightId = RightId from Security.[Right] where RightName = 'AccessSettingsAdmin'
select @MenuId = Id from Enterprise.NavigationMenu where Title = 'Admin Console' and Origin = 'unified-settings'

IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId and RightId = @RightId)
BEGIN
	INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
	VALUES(@MenuId, @RightId)
END

--Setting 2
IF EXISTS (select top 1 1 from Security.[Right] where Value = 'Employee Access to Login Page Setup')
BEGIN
	update Security.[Right] set Value ='Employee Access to Platform Settings and Service' where Value = 'Employee Access to Login Page Setup'
END
select @RightId = RightId from Security.[Right] where Value = 'Employee Access to Platform Settings and Service'
select @MenuId = Id from Enterprise.NavigationMenu where Title = 'Manage Settings' and Origin = 'unified-settings'
IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId and RightId = @RightId)
BEGIN
INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
VALUES(@MenuId, @RightId)
END

--Setting 3
IF EXISTS (select top 1 1 from Security.[Right] where Value = 'Employee Access to Manage Settings templates')
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

--Setting 4

select   @RightId = RightId from Security.[Right] where Value ='Internal Admin Access to Unified Settings'
select @MenuId = Id from Enterprise.NavigationMenu where Title = 'Manage Templates' and Origin = 'unified-settings'
IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId and RightId = @RightId)
BEGIN
	INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
	VALUES(@MenuId, @RightId)
END

--Setting 5 

select   @RightId = RightId from Security.[Right] where Value ='Manage Settings Templates'
select @MenuId = Id from Enterprise.NavigationMenu where Title = 'Manage Settings' and Origin = 'unified-settings'
IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId and RightId = @RightId)
BEGIN
INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
VALUES(@MenuId, @RightId)
END


--Setting 6
Declare @userId int
select @userId = userid from Ident.UserLogin where LoginName like '%realpagead@%'
IF NOT EXISTS(select top 1 1 from Security.[Right] where Value = 'View Settings Templates')
BEGIN
	INSERT INTO Security.[Right](RightName, [Description], [Value], StatusTypeId, VisibilityStatusId, ProductId, TargetProductId,CreatedBy,CreatedDate,PersistRight)
	values('ViewSettingsTemplates',	'This right allows a user access to Settings > Manage Templates to view templates.',	'View Settings Templates',	13,	9,	3,	56,	@userId,GETUTCDATE(),0)
END
select   @RightId = RightId from Security.[Right] where Value ='View Settings Templates'
select @MenuId = Id from Enterprise.NavigationMenu where Title = 'Manage Settings' and Origin = 'unified-settings'
IF NOT EXISTS(select top 1 1 from Enterprise.NavigationMenuRights where NavigationMenuId = @MenuId and RightId = @RightId)
BEGIN
INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId,RightId)
VALUES(@MenuId, @RightId)
END