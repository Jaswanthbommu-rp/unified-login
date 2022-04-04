GO
IF NOT EXISTS ( SELECT TOP (1) (1) FROM Enterprise.ThirdPartyRelationship )
BEGIN
	INSERT INTO [Enterprise].[ThirdPartyRelationship] (ThirdPartyRelationshipId, ThirdPartyRelationship )
		VALUES (1, 'Operator'),(2,'Owner'),(3,'Third Party Vendor')

END

GO
   --Independent- facilities Roles and RolesforRights.
Declare @UserId BigINT;
Declare @Role1 BIGINT,@Role2 BIGINT,@Role3 BIGINT,@Role4 BIGINT,@Role5 BIGINT,@Role6 BIGINT,@Role7 BIGINT,@Role8 BIGINT,@Role9 BIGINT,@Role10 BIGINT;
Declare @rightId1 BIGINT,@rightId2 BIGINT,@rightId3 BIGINT,@rightId4 BIGINT,@rightId5 BIGINT,@rightId6 BIGINT,@rightId7 BIGINT,@rightId8 BIGINT,
        @rightId9 BIGINT,@rightId10 BIGINT;
SELECT @UserId = UserId FROM Ident.UserLogin WHERE LoginName like 'realpagead@%';

IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'AssistantPropertyManager' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('AssistantPropertyManager', 'AssistantPropertyManager', 'Assistant Property Manager', 3, NULL, 82, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'LeasingConsultant' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('LeasingConsultant', 'LeasingConsultant', 'Leasing Consultant', 3, NULL, 82, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'PropertyAdministrator' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('PropertyAdministrator', 'PropertyAdministrator', 'Property Administrator', 3, NULL, 82, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'PropertyInspector' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('PropertyInspector', 'PropertyInspector', 'Property Inspector', 3, NULL, 82, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'PropertyMaintenanceLead' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('PropertyMaintenanceLead', 'PropertyMaintenanceLead', 'Property Maintenance Lead', 3, NULL, 82, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'PropertyMaintenanceWorker' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('PropertyMaintenanceWorker', 'PropertyMaintenanceWorker', 'Property Maintenance Worker', 3, NULL, 82, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'PropertyManager' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('PropertyManager', 'PropertyManager', 'Property Manager', 3, NULL, 82, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'RegionalPropertyManager' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('RegionalPropertyManager', 'RegionalPropertyManager', 'Regional Property Manager', 3, NULL, 82, @UserId, GETDATE())	
END	

SELECT @Role1 = RoleId FROM Security.Role WHERE RoleName = 'AssistantPropertyManager' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role2 = RoleId FROM Security.Role WHERE RoleName = 'LeasingConsultant' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role3 = RoleId FROM Security.Role WHERE RoleName = 'PropertyAdministrator' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role4 = RoleId FROM Security.Role WHERE RoleName = 'PropertyInspector' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role5 = RoleId FROM Security.Role WHERE RoleName = 'PropertyMaintenanceLead' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role6 = RoleId FROM Security.Role WHERE RoleName = 'PropertyMaintenanceWorker' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role7 = RoleId FROM Security.Role WHERE RoleName = 'PropertyManager' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role8 = RoleId FROM Security.Role WHERE RoleName = 'RegionalPropertyManager' AND OrgPartyID IS NULL AND ProductId = 82;

IF NOT EXISTS (Select Top 1 1 from Security.[Right] where RightName = 'ManageIndependentFacilitiesProductAccess' and ProductId = 82)
BEGIN
  INSERT INTO Security.[Right](RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate)
	VALUES('ManageIndependentFacilitiesProductAccess', 'Manage Independent Facilities Product Access', 'Manage Independent Facilities Product Access', 13, 9, 3, 82, @UserId, GETDATE())
END

IF NOT EXISTS(SELECT 1 FROM Security.[Right] WHERE RightName in ('Addmakeready','Addservicerequest','Cancelservicerequest','Closeservicerequest', 
                                                                   'Editservicerequest','Reopenservicerequest','Viewasset','Viewservicerequest',
																   'AssignschedulePM','Undomakeready') )
BEGIN
	INSERT INTO Security.[Right](RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate)
	VALUES('Addmakeready', 'Add make ready', 'Add make ready', 13, 9, 82, 82, @UserId, GETDATE())
	     ,('Addservicerequest', 'Add service request', 'Add service request', 13, 9, 82, 82, @UserId, GETDATE())
		 ,('Cancelservicerequest', 'Cancel service request', 'Cancel service request', 13, 9, 82, 82, @UserId, GETDATE())
		 ,('Closeservicerequest', 'Close service request', 'Close service request', 13, 9, 82, 82, @UserId, GETDATE())
		 ,('Editservicerequest', 'Edit service request', 'Edit service request', 13, 9, 82, 82, @UserId, GETDATE())
		 ,('Reopenservicerequest', 'Reopen service request', 'Reopen service request', 13, 9, 82, 82, @UserId, GETDATE())
		 ,('Viewasset', 'View asset', 'View asset', 13, 9, 82, 82, @UserId, GETDATE())
		 ,('Viewservicerequest', 'View service request', 'View service request', 13, 9, 82, 82, @UserId, GETDATE())
		 ,('AssignschedulePM', 'Assign schedule PM', 'Assign schedule PM', 13, 9, 82, 82, @UserId, GETDATE())
		 ,('Undomakeready', 'Undo make ready', 'Undo make ready', 13, 9, 82, 82, @UserId, GETDATE())
END

SELECT @Role1 = RoleId FROM Security.Role WHERE RoleName = 'AssistantPropertyManager' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role2 = RoleId FROM Security.Role WHERE RoleName = 'LeasingConsultant' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role3 = RoleId FROM Security.Role WHERE RoleName = 'PropertyAdministrator' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role4 = RoleId FROM Security.Role WHERE RoleName = 'PropertyInspector' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role5 = RoleId FROM Security.Role WHERE RoleName = 'PropertyMaintenanceLead' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role6 = RoleId FROM Security.Role WHERE RoleName = 'PropertyMaintenanceWorker' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role7 = RoleId FROM Security.Role WHERE RoleName = 'PropertyManager' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role8 = RoleId FROM Security.Role WHERE RoleName = 'RegionalPropertyManager' AND OrgPartyID IS NULL AND ProductId = 82;

Select @rightId1  = RightId from Security.[Right] where RightName ='Addmakeready';
Select @rightId2  = RightId from Security.[Right] where RightName ='Addservicerequest';
Select @rightId3  = RightId from Security.[Right] where RightName ='Cancelservicerequest';
Select @rightId4  = RightId from Security.[Right] where RightName ='Closeservicerequest';
Select @rightId5  = RightId from Security.[Right] where RightName ='Editservicerequest';
Select @rightId6  = RightId from Security.[Right] where RightName ='Reopenservicerequest';
Select @rightId7  = RightId from Security.[Right] where RightName ='Viewasset';
Select @rightId8  = RightId from Security.[Right] where RightName ='Viewservicerequest';
Select @rightId9  = RightId from Security.[Right] where RightName ='AssignschedulePM';
Select @rightId10  = RightId from Security.[Right] where RightName ='Undomakeready';

--RoleRightScript

IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role1 and RightId in (@rightId1,@rightId2,@rightId3,@rightId4,@rightId5,@rightId6,@rightId7,@rightId8))
BEGIN
  INSERT INTO Security.RoleRight values (@Role1,@rightId1,@UserId,GETDATE())
                                       ,(@Role1,@rightId2,@UserId,GETDATE())
									   ,(@Role1,@rightId3,@UserId,GETDATE())
									   ,(@Role1,@rightId4,@UserId,GETDATE())
									   ,(@Role1,@rightId5,@UserId,GETDATE())
									   ,(@Role1,@rightId6,@UserId,GETDATE())
									   ,(@Role1,@rightId7,@UserId,GETDATE())
									   ,(@Role1,@rightId8,@UserId,GETDATE())
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role2 and RightId in (@rightId1,@rightId8))
BEGIN
  INSERT INTO Security.RoleRight values (@Role2,@rightId1,@UserId,GETDATE())
                                       ,(@Role2,@rightId8,@UserId,GETDATE())				   
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role3 and RightId in (@rightId1,@rightId2,@rightId3,@rightId4,@rightId5,@rightId6,@rightId7,@rightId8,@rightId9,@rightId10))
BEGIN
  INSERT INTO Security.RoleRight values (@Role3,@rightId1,@UserId,GETDATE())
                                       ,(@Role3,@rightId2,@UserId,GETDATE())
									   ,(@Role3,@rightId3,@UserId,GETDATE())
									   ,(@Role3,@rightId4,@UserId,GETDATE())
									   ,(@Role3,@rightId5,@UserId,GETDATE())
									   ,(@Role3,@rightId6,@UserId,GETDATE())
									   ,(@Role3,@rightId7,@UserId,GETDATE())
									   ,(@Role3,@rightId8,@UserId,GETDATE())
									   ,(@Role3,@rightId9,@UserId,GETDATE())
									   ,(@Role3,@rightId10,@UserId,GETDATE())
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role4 and RightId in (@rightId1,@rightId2,@rightId5,@rightId6,@rightId7,@rightId8))
BEGIN
  INSERT INTO Security.RoleRight values (@Role4,@rightId1,@UserId,GETDATE())
                                       ,(@Role4,@rightId2,@UserId,GETDATE())
									   ,(@Role4,@rightId5,@UserId,GETDATE())
									   ,(@Role4,@rightId6,@UserId,GETDATE())
									   ,(@Role4,@rightId7,@UserId,GETDATE())
									   ,(@Role4,@rightId8,@UserId,GETDATE())
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role5 and RightId in (@rightId1,@rightId2,@rightId5,@rightId6,@rightId7,@rightId8))
BEGIN
  INSERT INTO Security.RoleRight values (@Role5,@rightId1,@UserId,GETDATE())
                                       ,(@Role5,@rightId2,@UserId,GETDATE())
									   ,(@Role5,@rightId5,@UserId,GETDATE())
									   ,(@Role5,@rightId7,@UserId,GETDATE())
									   ,(@Role5,@rightId8,@UserId,GETDATE())
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role6 and RightId in (@rightId1,@rightId4,@rightId6,@rightId10))
BEGIN
  INSERT INTO Security.RoleRight values (@Role6,@rightId1,@UserId,GETDATE())
                                       ,(@Role6,@rightId4,@UserId,GETDATE())
									   ,(@Role6,@rightId6,@UserId,GETDATE())
									   ,(@Role6,@rightId10,@UserId,GETDATE())
									  
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role7 and RightId in (@rightId1,@rightId2,@rightId3,@rightId4,@rightId5,@rightId6,@rightId10,@rightId7))
BEGIN
  INSERT INTO Security.RoleRight values (@Role7,@rightId1,@UserId,GETDATE())
                                       ,(@Role7,@rightId2,@UserId,GETDATE())
									   ,(@Role7,@rightId3,@UserId,GETDATE())
									   ,(@Role7,@rightId4,@UserId,GETDATE())
									   ,(@Role7,@rightId5,@UserId,GETDATE())
									   ,(@Role7,@rightId6,@UserId,GETDATE())
									   ,(@Role7,@rightId10,@UserId,GETDATE())
									   ,(@Role7,@rightId7,@UserId,GETDATE())
									  
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role8 and RightId in (@rightId1,@rightId2,@rightId3,@rightId4,@rightId5,@rightId6,@rightId7,@rightId8,@rightId9,@rightId10))
BEGIN
  INSERT INTO Security.RoleRight values (@Role8,@rightId1,@UserId,GETDATE())
                                       ,(@Role8,@rightId2,@UserId,GETDATE())
									   ,(@Role8,@rightId3,@UserId,GETDATE())
									   ,(@Role8,@rightId4,@UserId,GETDATE())
									   ,(@Role8,@rightId5,@UserId,GETDATE())
									   ,(@Role8,@rightId6,@UserId,GETDATE())
									   ,(@Role8,@rightId7,@UserId,GETDATE())
									   ,(@Role8,@rightId8,@UserId,GETDATE())
									   ,(@Role8,@rightId9,@UserId,GETDATE())
									   ,(@Role8,@rightId10,@UserId,GETDATE())
END

GO

--User Story 1052090: DB/API: Enhancement to persist rights based on the users role in employee company and AD group(s)
UPDATE [Security].[Right] SET PersistRight = 1 WHERE RightName IN (
'ACCESSTOUNIFIEDPLATFORM','VIEWUNIFIEDSETTINGS','MANAGEUNIFIEDSETTINGS'
,'INTERNALADMINACCESSTOUNIFIEDSETTINGS','MANAGECUSTOMFIELDS','MANAGEPLATFORMSECURITY'
,'MANAGESETTINGSTEMPLATES','ACCESSSETTINGSADMIN','EMPLOYEEACCESSTOMANAGESETTINGSTEMPLATES'
,'ABILITYTOIMPORTUSERS','MANAGENOTIFICATIONS')
