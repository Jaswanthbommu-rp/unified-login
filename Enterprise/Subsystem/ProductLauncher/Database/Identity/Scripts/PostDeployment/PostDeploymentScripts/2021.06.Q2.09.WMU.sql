GO
Declare @ControlId INT 
SELECT @ControlId = ControlId FROM UserManagement.Control WHERE UIId='On-SiteProductAccessAssignnewpropertiesautomaticallyPropertiesSwitchUIId' AND DisplayName = 'Assign new properties automatically'
IF (@ControlId IS NOT NULL)
BEGIN
UPDATE UserManagement.Control SET DisplayName = 'Assign current and new properties automatically' Where ControlId = @ControlId
END
 
GO
