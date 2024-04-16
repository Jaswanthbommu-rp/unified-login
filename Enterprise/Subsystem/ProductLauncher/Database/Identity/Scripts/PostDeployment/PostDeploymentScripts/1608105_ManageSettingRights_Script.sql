
Go
Declare @createdate datetime, @CreatedBy bigint;

select @createdate = GETUTCDATE()
SELECT	@CreatedBy = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'


-- Creating the Rights if they are not available
IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[Right] where Value = 'Manage Company Affordable Settings Only')
BEGIN
	INSERT INTO Security.[Right] (RightName,[Description],[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
	VALUES('ManageCompanyAffordableSettingsOnly','Settings testing use only','Manage Company Affordable Settings Only',13,9,3,56,@CreatedBy,@createdate,0,0)
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[Right] where Value = 'Manage Property Affordable Settings Only')
BEGIN
	INSERT INTO Security.[Right] (RightName,[Description],[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
	VALUES('ManagePropertyAffordableSettingsOnly','Dont use Manage Property Affordable Settings Only','Manage Property Affordable Settings Only',13,9,3,56,@CreatedBy,@createdate,0,0)
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[Right] where Value = 'Manage Templates and Global Updates for Affordable Settings Only')
BEGIN
	INSERT INTO Security.[Right] (RightName,[Description],[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
	VALUES('ManageTemplatesandGlobalAffordableSettingsOnly','Settings testing use only','Manage Templates and Global Updates for Affordable Settings Only',13,9,3,56,@CreatedBy,@createdate,0,0)
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[Right] where Value = 'Manage Company Facilities Settings Only')
BEGIN
	INSERT INTO Security.[Right] (RightName,[Description],[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
	VALUES('ManageCompanyFacilitiesSettingsOnly','Manage Company Facilities Settings Only','Manage Company Facilities Settings Only',13,9,3,56,@CreatedBy,@createdate,0,0)
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[Right] where Value = 'Manage Properties Facilities Settings Only')
BEGIN
	INSERT INTO Security.[Right] (RightName,[Description],[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
	VALUES('ManagePropertiesFacilitiesSettingsOnly','Manage Properties Facilities Settings Only','Manage Properties Facilities Settings Only',13,9,3,56,@CreatedBy,@createdate,0,0)
END


IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[Right] where Value = 'Manage Templates and Global Updates for Facilities Settings Only')
BEGIN
	INSERT INTO Security.[Right] (RightName,[Description],[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
	VALUES('ManageTemplatesandGlobalUpdatesFacilitiesSettings','Manage Templates and Global Updates for Facilities Settings Only','Manage Templates and Global Updates for Facilities Settings Only',13,9,3,56,@CreatedBy,@createdate,0,0)
END

IF NOT EXISTS (SELECT TOP 1 1 FROM Security.[Right] where Value = 'Manage Property Level Concessions')
BEGIN
	INSERT INTO Security.[Right] (RightName,[Description],[Value],StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight,IsExcludeRightFromImpersonation)
	VALUES('ManagePropertyLevelConcessions','Manage Property Level Concessions  ManagePropertyLevelConcessions','Manage Property Level Concessions',13,9,3,56,@CreatedBy,@createdate,0,0)
END

-- Giving access to Sidemenus based on the rights

declare @managesettingid int,@managetemplateid int,@settingactivitylogid int, @rightId int, @routeId int

select @routeId = RouteId from Security.Route where RouteValue = 'SideMenu'
select @managesettingid = id from Enterprise.NavigationMenu where Title = 'Manage Settings'
select @managetemplateid = id from Enterprise.NavigationMenu where Title = 'Manage Templates'
select @settingactivitylogid = id from Enterprise.NavigationMenu where Title = 'Settings Activity Log'

SELECT @rightId = RightId FROM Security.[Right] where Value = 'Manage Company Affordable Settings Only'

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId = @managesettingid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@managesettingid,@rightId)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId= @settingactivitylogid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@settingactivitylogid,@rightId)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Security.RightRoute where RightId = @rightId and RouteId = @routeId)
BEGIN
	insert into Security.RightRoute(RightId, RouteId,CreatedBy,CreatedDate)
	values(@rightId,@routeId,@CreatedBy,@createdate)
END

SELECT @rightId = RightId FROM Security.[Right] where Value = 'Manage Property Affordable Settings Only'
IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId = @managesettingid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@managesettingid,@rightId)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId = @settingactivitylogid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@settingactivitylogid,@rightId)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Security.RightRoute where RightId = @rightId and RouteId = @routeId)
BEGIN
	insert into Security.RightRoute(RightId, RouteId,CreatedBy,CreatedDate)
	values(@rightId,@routeId,@CreatedBy,@createdate)
END


SELECT @rightId = RightId FROM Security.[Right] where Value = 'Manage Templates and Global Updates for Affordable Settings Only'
IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId =@managesettingid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@managesettingid,@rightId)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId =@managetemplateid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@managetemplateid,@rightId)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId = @settingactivitylogid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@settingactivitylogid,@rightId)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Security.RightRoute where RightId = @rightId and RouteId = @routeId)
BEGIN
	insert into Security.RightRoute(RightId, RouteId,CreatedBy,CreatedDate)
	values(@rightId,@routeId,@CreatedBy,@createdate)
END


SELECT @rightId = RightId FROM Security.[Right] where Value = 'Manage Company Facilities Settings Only'
IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId = @managesettingid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@managesettingid,@rightId)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId = @settingactivitylogid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@settingactivitylogid,@rightId)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Security.RightRoute where RightId = @rightId and RouteId = @routeId)
BEGIN
	insert into Security.RightRoute(RightId, RouteId,CreatedBy,CreatedDate)
	values(@rightId,@routeId,@CreatedBy,@createdate)
END


SELECT @rightId = RightId FROM Security.[Right] where Value = 'Manage Properties Facilities Settings Only'
IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId = @managesettingid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@managesettingid,@rightId)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId = @settingactivitylogid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@settingactivitylogid,@rightId)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Security.RightRoute where RightId = @rightId and RouteId = @routeId)
BEGIN
	insert into Security.RightRoute(RightId, RouteId,CreatedBy,CreatedDate)
	values(@rightId,@routeId,@CreatedBy,@createdate)
END


SELECT @rightId = RightId FROM Security.[Right] where Value = 'Manage Templates and Global Updates for Facilities Settings Only'
IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId = @managesettingid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@managesettingid,@rightId)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId   = @managetemplateid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@managetemplateid,@rightId)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId = @settingactivitylogid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@settingactivitylogid,@rightId)
END



IF NOT EXISTS(SELECT TOP 1 1 FROM Security.RightRoute where RightId = @rightId and RouteId = @routeId)
BEGIN
	insert into Security.RightRoute(RightId, RouteId,CreatedBy,CreatedDate)
	values(@rightId,@routeId,@CreatedBy,@createdate)
END


SELECT @rightId = RightId FROM Security.[Right] where Value = 'Manage Property Level Concessions'
IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId = @managesettingid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@managesettingid,@rightId)
END

IF NOT EXISTS(SELECT TOP 1 1 FROM Enterprise.NavigationMenuRights where RightId = @rightId and NavigationMenuId = @settingactivitylogid)
BEGIN
 INSERT INTO Enterprise.NavigationMenuRights(NavigationMenuId, RightId)
 VALUES(@settingactivitylogid,@rightId)
END


IF NOT EXISTS(SELECT TOP 1 1 FROM Security.RightRoute where RightId = @rightId and RouteId = @routeId)
BEGIN
	insert into Security.RightRoute(RightId, RouteId,CreatedBy,CreatedDate)
	values(@rightId,@routeId,@CreatedBy,@createdate)
END

Go