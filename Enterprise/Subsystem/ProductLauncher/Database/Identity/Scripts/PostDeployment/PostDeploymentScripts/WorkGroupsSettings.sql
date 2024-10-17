

Declare @createdate datetime, @CreatedBy bigint,@adminRoleId int
select @adminRoleId = RoleId from [Security].[Role] where RoleName = 'User Administrator'
 

select @createdate = GETUTCDATE()
SELECT	@CreatedBy = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[Right] where RightName = 'ManageCompanyLevelFacilitiesSettingsforWorkGroupsTechniciansAssignment')
BEGIN
INSERT INTO [Security].[Right](
RightName,[Description],[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation
) VALUES(
	'ManageCompanyLevelFacilitiesSettingsforWorkGroupsTechniciansAssignment',	
	'Ability to manage Facilities settings for Work Groups, Technicians & Assignment only',
	'Manage Company Level Facilities Settings for Work Groups, Technicians & Assignment',	13,	9,	3,	56,	@CreatedBy,	@createdate,0,	0)

END


IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[Right] where RightName = 'ManagePropertyLevelFacilitiesSettingsforWorkGroupsTechniciansAssignment')
BEGIN
INSERT INTO [Security].[Right](
	RightName,	[Description],	[Value],	StatusTypeId,	VisibilityStatusId	,ProductId	,TargetProductId	,CreatedBy	,CreatedDate	,PersistRight,
	IsExcludeRightFromImpersonation) VALUES (
	'ManagePropertyLevelFacilitiesSettingsforWorkGroupsTechniciansAssignment',	
	'Ability to manage Facilities settings for Work Groups, Technicians & Assignment only',	
	'Manage Property Level Facilities Settings for Work Groups, Technicians & Assignment',	13,	9,	3,	56,	@CreatedBy,	@createdate,	0,	0)
END

IF NOT EXISTS (SELECT TOP 1 1 FROM [Security].[Right] where RightName = 'ManageTemplateLevelFacilitiesSettingsforWorkGroupsTechniciansAssignment')
BEGIN
INSERT INTO [Security].[Right](
	RightName	,[Description],	[Value],	StatusTypeId,	VisibilityStatusId,	ProductId,	TargetProductId,	CreatedBy,	CreatedDate,	PersistRight	,
	IsExcludeRightFromImpersonation)
	VALUES (
	'ManageTemplateLevelFacilitiesSettingsforWorkGroupsTechniciansAssignment',	
	'Ability to manage Facilities settings for Work Groups, Technicians & Assignment only',	
	'Manage Template Level Facilities Settings for Work Groups, Technicians & Assignment',	13,	9,	3,	56,	@CreatedBy,	@createdate,	0,	0)

END

declare @ManageCompanyLevelRightId int = (select top 1 RightId from [Security].[Right] where RightName = 'ManageCompanyLevelFacilitiesSettingsforWorkGroupsTechniciansAssignment')

declare @ManagePropertyLevelRightId int = (select top 1 RightId from [Security].[Right] where RightName = 'ManagePropertyLevelFacilitiesSettingsforWorkGroupsTechniciansAssignment')

declare @ManageTemplateLevelRightId int = (select top 1 RightId from [Security].[Right] where RightName = 'ManageTemplateLevelFacilitiesSettingsforWorkGroupsTechniciansAssignment')

declare @SettingMenutId int = (select top 1 Id from [Enterprise].[NavigationMenu] where PageId = 'settings')

declare @ManageSettingsMenuId int = (select top 1 Id from [Enterprise].[NavigationMenu] where PageId = 'manage-settings')

declare @ManageTemplatesMenuId int = (select top 1 Id from [Enterprise].[NavigationMenu] where PageId = 'manage-templates')

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].RoleRight where RightId = @ManageCompanyLevelRightId and RoleId = @adminRoleId)
BEGIN
INSERT INTO Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
VALUES(@adminRoleId,@ManageCompanyLevelRightId,@CreatedBy,@createdate)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].RoleRight where RightId = @ManagePropertyLevelRightId and RoleId = @adminRoleId)
BEGIN
INSERT INTO Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
VALUES(@adminRoleId,@ManagePropertyLevelRightId,@CreatedBy,@createdate)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].RoleRight where RightId = @ManageTemplateLevelRightId and RoleId = @adminRoleId)
BEGIN
INSERT INTO Security.RoleRight(RoleId,RightId,CreatedBy,CreatedDate)
VALUES(@adminRoleId,@ManageTemplateLevelRightId,@CreatedBy,@createdate)
END

IF NOT EXISTS (SELECT TOP 1 1 FROM [Enterprise].[NavigationMenuRights] WHERE NavigationMenuId = @SettingMenutId AND RightId = @ManageCompanyLevelRightId)
BEGIN 
insert into [Enterprise].[NavigationMenuRights]
select @SettingMenutId,@ManageCompanyLevelRightId
END
IF NOT EXISTS (SELECT TOP 1 1 FROM [Enterprise].[NavigationMenuRights] WHERE NavigationMenuId = @SettingMenutId AND RightId = @ManagePropertyLevelRightId)
BEGIN 
insert into [Enterprise].[NavigationMenuRights]
select @SettingMenutId,@ManagePropertyLevelRightId
END

IF NOT EXISTS (SELECT TOP 1 1 FROM [Enterprise].[NavigationMenuRights] WHERE NavigationMenuId = @SettingMenutId AND RightId = @ManageTemplateLevelRightId)
BEGIN 
insert into [Enterprise].[NavigationMenuRights]
select @SettingMenutId,@ManageTemplateLevelRightId
END

IF NOT EXISTS (SELECT TOP 1 1 FROM [Enterprise].[NavigationMenuRights] WHERE NavigationMenuId = @ManageSettingsMenuId AND RightId = @ManageCompanyLevelRightId)
BEGIN 
insert into [Enterprise].[NavigationMenuRights]
select @ManageSettingsMenuId,@ManageCompanyLevelRightId
END

IF NOT EXISTS (SELECT TOP 1 1 FROM [Enterprise].[NavigationMenuRights] WHERE NavigationMenuId = @ManageSettingsMenuId AND RightId = @ManagePropertyLevelRightId)
BEGIN 
insert into [Enterprise].[NavigationMenuRights]
select @ManageSettingsMenuId,@ManagePropertyLevelRightId
END

IF NOT EXISTS (SELECT TOP 1 1 FROM [Enterprise].[NavigationMenuRights] WHERE NavigationMenuId = @ManageSettingsMenuId AND RightId = @ManageTemplateLevelRightId)
BEGIN 
insert into [Enterprise].[NavigationMenuRights]
select @ManageSettingsMenuId,@ManageTemplateLevelRightId
END

IF NOT EXISTS (SELECT TOP 1 1 FROM [Enterprise].[NavigationMenuRights] WHERE NavigationMenuId = @ManageTemplatesMenuId AND RightId = @ManageTemplateLevelRightId)
BEGIN 
insert into [Enterprise].[NavigationMenuRights]
select @ManageTemplatesMenuId,@ManageTemplateLevelRightId
END

