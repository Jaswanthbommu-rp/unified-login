
--1019723
GO
Declare @NavigationMenuID BIGINT, @RightID BIGINT;
Select @RightID = RightId from Security.[Right] where value = 'Employee Access to Manage Settings Templates';
Select @NavigationMenuID = Id  from Enterprise.NavigationMenu where Title = 'Manage Templates' and PageId ='manage-templates' and Origin ='unified-settings';
IF NOT EXISTS (Select Top 1 1 from ENterprise.NavigationMenuRights where NavigationMenuId =@NavigationMenuID and RightId = @RightID)
BEGIN
INSERT INTO ENterprise.NavigationMenuRights (NavigationMenuId,RightId)
                                 Values (@NavigationMenuID,@RightID);
END

GO
--User Story 1052090: DB/API: Enhancement to persist rights based on the users role in employee company and AD group(s)
UPDATE [Security].[Right] SET PersistRight = 1 WHERE RightName IN (
'ACCESSTOUNIFIEDPLATFORM','VIEWUNIFIEDSETTINGS','MANAGEUNIFIEDSETTINGS'
,'INTERNALADMINACCESSTOUNIFIEDSETTINGS','MANAGECUSTOMFIELDS','MANAGEPLATFORMSECURITY'
,'MANAGESETTINGSTEMPLATES','ACCESSSETTINGSADMIN','EMPLOYEEACCESSTOMANAGESETTINGSTEMPLATES'
,'ABILITYTOIMPORTUSERS','MANAGENOTIFICATIONS','VIEWONLYSUPPORTTOOLACCESS')