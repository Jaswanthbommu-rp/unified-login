
--User Story 624898 Independent Facilities Product Integrations - standalone

Declare @UserId BigINT;

SELECT @UserId = UserId FROM Ident.UserLogin WHERE LoginName like 'realpagead@%';

Declare @Role1 BIGINT,@Role2 BIGINT,@Role3 BIGINT,@Role4 BIGINT,@Role5 BIGINT,@Role6 BIGINT,@Role7 BIGINT,@Role8 BIGINT,@Role9 BIGINT,@Role10 BIGINT;
Declare @rightId1 BIGINT,@rightId2 BIGINT,@rightId3 BIGINT,@rightId4 BIGINT,@rightId5 BIGINT,@rightId6 BIGINT,@rightId7 BIGINT,@rightId8 BIGINT,
        @rightId9 BIGINT,@rightId10 BIGINT,@rightId11 BIGINT,@rightId12 BIGINT,@rightId13 BIGINT,@rightId14 BIGINT,@rightId15 BIGINT,@rightId16 BIGINT,
		@rightId17 BIGINT,@rightId18 BIGINT,@rightId19 BIGINT,@rightId20 BIGINT,@rightId21 BIGINT,@rightId22 BIGINT,@rightId23 BIGINT,@rightId24 BIGINT,
		@rightId25 BIGINT,@rightId26 BIGINT,@rightId27 BIGINT,@rightId28 BIGINT,@rightId29 BIGINT,@rightId30 BIGINT,@rightId31 BIGINT,@rightId32 BIGINT,
		@rightId33 BIGINT,@rightId34 BIGINT,@rightId35 BIGINT,@rightId36 BIGINT,@rightId37 BIGINT,@rightId38 BIGINT,@rightId39 BIGINT,@rightId40 BIGINT,
		@rightId41 BIGINT,@rightId42 BIGINT,@rightId43 BIGINT,@rightId44 BIGINT,@rightId45 BIGINT,@rightId46 BIGINT,@rightId47 BIGINT,@rightId48 BIGINT
		;
SELECT @UserId = UserId FROM Ident.UserLogin WHERE LoginName like 'realpagead@%';
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'Assistant Property Manager' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('Assistant Property Manager', 'Assistant Property Manager', 'Assistant Property Manager', 3, NULL, 82, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'Leasing Consultant' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('Leasing Consultant', 'Leasing Consultant', 'Leasing Consultant', 3, NULL, 82, @UserId, GETDATE())	
END		
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'Property Inspector' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('Property Inspector', 'Property Inspector', 'Property Inspector', 3, NULL, 82, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'Property Maintenance Lead' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('Property Maintenance Lead', 'Property Maintenance Lead', 'Property Maintenance Lead', 3, NULL, 82, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'Property Maintenance Worker' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('Property Maintenance Worker', 'Property Maintenance Worker', 'Property Maintenance Worker', 3, NULL, 82, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'PropertyManager' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('Property Manager', 'Property Manager', 'Property Manager', 3, NULL, 82, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'Regional Property Manager' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('Regional Property Manager', 'Regional Property Manager', 'Regional Property Manager', 3, NULL, 82, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'Property Maintenance Manager' AND OrgPartyID IS NULL AND ProductId = 82)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('Property Maintenance Manager', 'Property Maintenance Manager', 'Property Maintenance Manager', 3, NULL, 82, @UserId, GETDATE())	
END	
SELECT @Role1 = RoleId FROM Security.Role WHERE RoleName = 'Assistant Property Manager' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role2 = RoleId FROM Security.Role WHERE RoleName = 'Leasing Consultant' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role3 = RoleId FROM Security.Role WHERE RoleName = 'Property Maintenance Manager' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role4 = RoleId FROM Security.Role WHERE RoleName = 'Property Inspector' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role5 = RoleId FROM Security.Role WHERE RoleName = 'Property Maintenance Lead' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role6 = RoleId FROM Security.Role WHERE RoleName = 'Property Maintenance Worker' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role7 = RoleId FROM Security.Role WHERE RoleName = 'Property Manager' AND OrgPartyID IS NULL AND ProductId = 82;
SELECT @Role8 = RoleId FROM Security.Role WHERE RoleName = 'Regional Property Manager' AND OrgPartyID IS NULL AND ProductId = 82;
IF NOT EXISTS (Select Top 1 1 from Security.[Right] where RightName = 'ManageIndependentFacilitiesProductAccess' and ProductId = 82)
BEGIN
    INSERT INTO Security.[Right] (RightName,Description,Value,StatusTypeId,VisibilityStatusId,ProductId,TargetProductId,CreatedBy,CreatedDate,PersistRight)
	VALUES('ManageIndependentFacilitiesProductAccess', 'Manage Independent Facilities Product Access', 'Manage Independent Facilities Product Access', 13, 9, 3, 82, @UserId, GETDATE(),1)
END
IF NOT EXISTS(SELECT 1 FROM Security.[Right] WHERE RightName in ('81addeditpmtemplate','81assignschedpm','81compchecklist','83editadvancetimesheet',
'83viewalltechs','86ManageMCAmenityContent','87bulkupdateturnboard','87editturnboard','87editturnboardchecklist','87gensemturn','87viewturnboard',
'89assignwarrantytoasset','89editwarrantytemplate','8addasset','8abtrlebal','8addmr','8addmrrenov','8addsr','8approveresidentcharge','8assignsr',
'8bdonholdsr','8bulkcloseservicerequests','8cancelsr','8canreopensr','8closemr','8editasset','8editemergency','8editclosedwo','8editSRcompletedate',
'8editSRLocation','8edittimesheets','8edmr','8edsr','8edsronholddate','8edwrkgrps','8entersrwithfuturedate','8manageassets','8reports','8ManageOpsUsers',
'8undomakeready','8uploadCompanyLogo','8vwallsr','8vwasset','8vwmr','8vwsr','8WGBillToResident','8closesr'
))
BEGIN
	INSERT INTO Security.[Right](RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate,PersistRight)
	VALUES('81addeditpmtemplate', 'Ability to create and edit an Inspection template', 'Ability to create and edit an Inspection template', 13, 9, 82, 82, @UserId, GETDATE(),0)
	     ,('81assignschedpm', 'Ability to create Inspection events', 'Ability to create Inspection events', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('81compchecklist', 'Ability to complete Inspection checklists', 'Ability to complete Inspection checklists', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('83editadvancetimesheet', 'Ability to edit technicians timesheet entries created by the mobile app', 'Ability to edit technicians timesheet entries created by the mobile app', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('83viewalltechs', 'Ability to view historical geo-location for all maintenance techs', 'Ability to view historical geo-location for all maintenance techs', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('86ManageMCAmenityContent', 'Permission to modify Marketing Amenity Content', 'Permission to modify Marketing Amenity Content', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('87bulkupdateturnboard', 'Ability to manage move out inspection and turn task in bulk', 'Ability to manage move out inspection and turn task in bulk', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('87editturnboard', 'Edit turn board', 'Edit turn board', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('87editturnboardchecklist', 'Ability to edit turn board checklist', 'Ability to edit turn board checklist', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('87gensemturn', 'Add a semester turn and view turn board', 'Add a semester turn and view turn board', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('87viewturnboard', 'View turn board', 'View turn board', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('89assignwarrantytoasset', 'Ability to assign a warranty to an asset', 'Ability to assign a warranty to an asset', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8abtrlebal', 'Ability to clear residents ledger balances', 'Ability to clear residents ledger balances', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8addasset', 'Add and view asset records', 'Add and view asset records', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8addmr', 'Add and view make ready requests', 'Add and view make ready requests', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8addmrrenov', 'Ability to create make ready items for renovations', 'Edit closed work order to create make ready items for renovations', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8addsr', 'Add and view service requests', 'Add and view service requests', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8approveresidentcharge', 'Can approve resident charges', 'Can approve resident charges', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8assignsr', 'Assign or reassign SR to a maintenance tech', 'Assign or reassign SR to a maintenance tech', 13, 9, 82, 82, @UserId, GETDATE(),0)
     	 ,('8bdonholdsr', 'Ability to set on hold SR dates to a previous date', 'Ability to set on hold SR dates to a previous date', 13, 9, 82, 82, @UserId, GETDATE(),0)	
		 ,('8bulkcloseservicerequests', 'Allow user to close service requests in bulk', 'Allow user to close service requests in bulk', 13, 9, 82, 82, @UserId, GETDATE(),0)
	     ,('8cancelsr', 'Cancel service requests', 'Cancel service requests', 13, 9, 82, 82, @UserId, GETDATE(),0)
	     ,('8canreopensr', 'Can re-open service request', 'Can re-open service request', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8closemr', 'Close make ready', 'Close make ready', 13, 9, 82, 82, @UserId, GETDATE(),0)
    	 ,('8editasset', 'Edit and view asset records', 'Edit and view asset records', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8editclosedwo', 'Edit closed work order', 'Edit closed work order', 13, 9, 82, 82, @UserId, GETDATE(),0)
         ,('8editemergency', 'Allow editing of emergency contact information on callcenter setup', 'Allow editing of emergency contact information on callcenter setup', 13, 9, 82, 82, @UserId, GETDATE(),0)
     	 ,('8editSRcompletedate', 'Can enter service request complete date', 'Can enter service request complete date', 13, 9, 82, 82, @UserId, GETDATE(),0)
	     ,('8editSRLocation', 'Can edit service request location', 'Can edit service request location', 13, 9, 82, 82, @UserId, GETDATE(),0)
	     ,('8edittimesheets', 'Edit time sheets for all technicians', 'Edit time sheets for all technicians', 13, 9, 82, 82, @UserId, GETDATE(),0)
	     ,('8edmr', 'Edit and view make ready requests', 'Edit and view make ready requests', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8edsr', 'Edit and view service requests', 'Edit and view service requests', 13, 9, 82, 82, @UserId, GETDATE(),0)
	     ,('8edsronholddate', 'Edit on hold dates of service requests', 'Edit on hold dates of service requests', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8edwrkgrps', 'Edit manage work groups', 'Edit manage work groups', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8entersrwithfuturedate', 'Can enter service requests with future dates', 'Can enter service requests with future dates', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8manageassets', 'Can manage assets', 'Can manage assets', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8ManageOpsUsers', 'Ability to manage Ops users', 'Ability to manage Ops users', 13, 9, 82, 82, @UserId, GETDATE(),0)
	     ,('8reports', 'Facilities reports', 'Facilities reports', 13, 9, 82, 82, @UserId, GETDATE(),0)
	     ,('8undomakeready', 'Undo Make Ready', 'Undo Make Ready', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8uploadCompanyLogo', 'Ability to manage company/site logo for service requests', 'Ability to manage company/site logo for service requests', 13, 9, 82, 82, @UserId, GETDATE(),0)
	     ,('8vwallsr', 'Can view all service requests **ONLY USED IN ONESITE MOBILE SERVICE REQUESTS', 'Can view all service requests **ONLY USED IN ONESITE MOBILE SERVICE REQUESTS', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8vwasset', 'View asset records', 'View asset records', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8vwmr', 'View make ready', 'View make ready', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8vwsr', 'View service requests', 'View service requests', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8WGBillToResident', 'Allow technicians to bill charges to residents', 'Allow technicians to bill charges to residents', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('89editwarrantytemplate', 'Ability to create and edit a warranty template', 'Ability to create and edit a warranty template', 13, 9, 82, 82, @UserId, GETDATE(),0)
		 ,('8closesr', 'Close service requests', 'Close service requests', 13, 9, 82, 82, @UserId, GETDATE(),0)
END
Select @rightId1  = RightId from Security.[Right] where Description = 'Ability to create and edit an Inspection template'
Select @rightId2  = RightId from Security.[Right] where Description = 'Ability to create Inspection events'
Select @rightId3  = RightId from Security.[Right] where Description = 'Ability to complete Inspection checklists'
Select @rightId4  = RightId from Security.[Right] where Description = 'Ability to edit technicians timesheet entries created by the mobile app'
Select @rightId5  = RightId from Security.[Right] where Description = 'Ability to view historical geo-location for all maintenance techs'
Select @rightId6  = RightId from Security.[Right] where Description = 'Permission to modify Marketing Amenity Content'
Select @rightId7  = RightId from Security.[Right] where Description = 'Ability to manage move out inspection and turn task in bulk'
Select @rightId8  = RightId from Security.[Right] where Description = 'Edit turn board'
Select @rightId9  = RightId from Security.[Right] where Description = 'Ability to edit turn board checklist'
Select @rightId10  = RightId from Security.[Right] where Description = 'Add a semester turn and view turn board'
Select @rightId11  = RightId from Security.[Right] where Description = 'View turn board'
Select @rightId13  = RightId from Security.[Right] where Description = 'Ability to assign a warranty to an asset'
Select @rightId14  = RightId from Security.[Right] where Description = 'Ability to clear residents ledger balances'
Select @rightId15  = RightId from Security.[Right] where Description = 'Add and view asset records'
Select @rightId16  = RightId from Security.[Right] where Description = 'Add and view make ready requests'
Select @rightId12  = RightId from Security.[Right] where Description = 'Ability to create make ready items for renovations'
Select @rightId17  = RightId from Security.[Right] where Description = 'Add and view service requests'
Select @rightId18  = RightId from Security.[Right] where Description = 'Can approve resident charges'
Select @rightId19  = RightId from Security.[Right] where Description = 'Assign or reassign SR to a maintenance tech'
Select @rightId20  = RightId from Security.[Right] where Description = 'Ability to set on hold SR dates to a previous date'
Select @rightId21  = RightId from Security.[Right] where Description = 'Allow user to close service requests in bulk'
Select @rightId22  = RightId from Security.[Right] where Description = 'Cancel service requests'
Select @rightId23  = RightId from Security.[Right] where Description = 'Can re-open service request'
Select @rightId24  = RightId from Security.[Right] where Description = 'Close make ready'
Select @rightId25  = RightId from Security.[Right] where Description = 'Edit and view asset records'
Select @rightId26  = RightId from Security.[Right] where Description = 'Edit closed work order'
Select @rightId27  = RightId from Security.[Right] where Description = 'Allow editing of emergency contact information on callcenter setup'
Select @rightId28  = RightId from Security.[Right] where Description = 'Can enter service request complete date'
Select @rightId29  = RightId from Security.[Right] where Description = 'Can edit service request location'
Select @rightId30  = RightId from Security.[Right] where Description = 'Edit time sheets for all technicians'
Select @rightId31  = RightId from Security.[Right] where Description = 'Edit and view make ready requests'
Select @rightId32  = RightId from Security.[Right] where Description = 'Edit and view service requests'
Select @rightId33  = RightId from Security.[Right] where Description = 'Edit on hold dates of service requests'
Select @rightId34  = RightId from Security.[Right] where Description = 'Edit manage work groups'
Select @rightId35  = RightId from Security.[Right] where Description = 'Can enter service requests with future dates'
Select @rightId36  = RightId from Security.[Right] where Description = 'Can manage assets'
Select @rightId37  = RightId from Security.[Right] where Description = 'Ability to manage Ops users'
Select @rightId38  = RightId from Security.[Right] where Description = 'Facilities reports'
Select @rightId39  = RightId from Security.[Right] where Description = 'Undo Make Ready'
Select @rightId40  = RightId from Security.[Right] where Description = 'Ability to manage company/site logo for service requests'
Select @rightId41  = RightId from Security.[Right] where Description = 'Can view all service requests **ONLY USED IN ONESITE MOBILE SERVICE REQUESTS'
Select @rightId42  = RightId from Security.[Right] where Description = 'View asset records'
Select @rightId43  = RightId from Security.[Right] where Description = 'View make ready'
Select @rightId44  = RightId from Security.[Right] where Description = 'View service requests'
Select @rightId45  = RightId from Security.[Right] where Description = 'Allow technicians to bill charges to residents'
Select @rightId46  = RightId from Security.[Right] where Description = 'Ability to create and edit a warranty template'
Select @rightId47  = RightId from Security.[Right] where Description = 'Close service requests'
--RoleRightScript
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role1 and RightId in (@rightId13,@rightId3,@rightId46,@rightId1,@rightId2,
@rightId12,@rightId4,@rightId9,@rightId40,@rightId7,@rightId37,@rightId20,@rightId5,@rightId10,@rightId15,@rightId16,@rightId45,@rightId19,@rightId18
,@rightId29,@rightId28,@rightId35,@rightId36,@rightId41,@rightId22,@rightId24,@rightId47,@rightId25,@rightId31,@rightId32,@rightId8,@rightId38,@rightId6
,@rightId39,@rightId42,@rightId43,@rightId44,@rightId11
))

BEGIN
  INSERT INTO Security.RoleRight values (@Role1,@rightId13,@UserId,GETDATE())
                                       ,(@Role1,@rightId3,@UserId,GETDATE())
									   ,(@Role1,@rightId46,@UserId,GETDATE())
									   ,(@Role1,@rightId1,@UserId,GETDATE())
									   ,(@Role1,@rightId2,@UserId,GETDATE())
									   ,(@Role1,@rightId12,@UserId,GETDATE())
									   ,(@Role1,@rightId4,@UserId,GETDATE())
									   ,(@Role1,@rightId9,@UserId,GETDATE())
									   ,(@Role1,@rightId40,@UserId,GETDATE())
                                       ,(@Role1,@rightId7,@UserId,GETDATE())
									   ,(@Role1,@rightId37,@UserId,GETDATE())
									   ,(@Role1,@rightId20,@UserId,GETDATE())
									   ,(@Role1,@rightId15,@UserId,GETDATE())
									   ,(@Role1,@rightId16,@UserId,GETDATE())
									   ,(@Role1,@rightId45,@UserId,GETDATE())
									   ,(@Role1,@rightId19,@UserId,GETDATE())
									   ,(@Role1,@rightId18,@UserId,GETDATE())
                                       ,(@Role1,@rightId29,@UserId,GETDATE())
									   ,(@Role1,@rightId28,@UserId,GETDATE())
									   ,(@Role1,@rightId35,@UserId,GETDATE())
									   ,(@Role1,@rightId36,@UserId,GETDATE())
									   ,(@Role1,@rightId41,@UserId,GETDATE())
									   ,(@Role1,@rightId22,@UserId,GETDATE())
									   ,(@Role1,@rightId24,@UserId,GETDATE())
									   ,(@Role1,@rightId47,@UserId,GETDATE())
                                       ,(@Role1,@rightId25,@UserId,GETDATE())
									   ,(@Role1,@rightId31,@UserId,GETDATE())
									   ,(@Role1,@rightId32,@UserId,GETDATE())
									   ,(@Role1,@rightId38,@UserId,GETDATE())
									   ,(@Role1,@rightId8,@UserId,GETDATE())
									   ,(@Role1,@rightId39,@UserId,GETDATE())
									   ,(@Role1,@rightId6,@UserId,GETDATE())
									   ,(@Role1,@rightId42,@UserId,GETDATE())
                                       ,(@Role1,@rightId43,@UserId,GETDATE())
									   ,(@Role1,@rightId44,@UserId,GETDATE())
									   ,(@Role1,@rightId11,@UserId,GETDATE())
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role2 and RightId in (@rightId9,@rightId16,@rightId17,@rightId19,@rightId28,@rightId35
,@rightId22,@rightId24,@rightId47,@rightId31,@rightId32,@rightId8,@rightId38,@rightId39,@rightId42,@rightId43,@rightId44,@rightId11
))
BEGIN
  INSERT INTO Security.RoleRight values (@Role2,@rightId9,@UserId,GETDATE())
                                       ,(@Role2,@rightId16,@UserId,GETDATE())
									    ,(@Role2,@rightId17,@UserId,GETDATE())
                                       ,(@Role2,@rightId19,@UserId,GETDATE())
									    ,(@Role2,@rightId28,@UserId,GETDATE())
                                       ,(@Role2,@rightId35,@UserId,GETDATE())
									   ,(@Role2,@rightId22,@UserId,GETDATE())
                                       ,(@Role2,@rightId24,@UserId,GETDATE())
									   ,(@Role2,@rightId47,@UserId,GETDATE())
                                       ,(@Role2,@rightId31,@UserId,GETDATE())
									    ,(@Role2,@rightId32,@UserId,GETDATE())
                                       ,(@Role2,@rightId8,@UserId,GETDATE())
									   ,(@Role2,@rightId38,@UserId,GETDATE())
                                       ,(@Role2,@rightId39,@UserId,GETDATE())
									   ,(@Role2,@rightId42,@UserId,GETDATE())
                                       ,(@Role2,@rightId43,@UserId,GETDATE())
									   ,(@Role2,@rightId44,@UserId,GETDATE())
                                       ,(@Role2,@rightId11,@UserId,GETDATE())
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role3 and RightId in (@rightId13,@rightId14,@rightId3,@rightId46,@rightId1
,@rightId2,@rightId12,@rightId4,@rightId9,@rightId40,@rightId7,@rightId37,@rightId20,@rightId5,@rightId10,@rightId15,@rightId16,@rightId17,
@rightId27,@rightId45,@rightId21,@rightId19,@rightId28,@rightId35,@rightId36,@rightId41,@rightId22,@rightId24,@rightId47,@rightId25,@rightId31
,@rightId32,@rightId34,@rightId33,@rightId30,@rightId8,@rightId38,@rightId6,@rightId39,@rightId42,@rightId43,@rightId44,@rightId11
))
BEGIN
  INSERT INTO Security.RoleRight values (@Role3,@rightId13,@UserId,GETDATE())
                                       ,(@Role3,@rightId14,@UserId,GETDATE())
									   ,(@Role3,@rightId3,@UserId,GETDATE())
									   ,(@Role3,@rightId46,@UserId,GETDATE())
									   ,(@Role3,@rightId1,@UserId,GETDATE())
									   ,(@Role3,@rightId2,@UserId,GETDATE())
									   ,(@Role3,@rightId12,@UserId,GETDATE())
									   ,(@Role3,@rightId4,@UserId,GETDATE())
									   ,(@Role3,@rightId9,@UserId,GETDATE())
									   ,(@Role3,@rightId40,@UserId,GETDATE())
									   ,(@Role3,@rightId7,@UserId,GETDATE())
                                       ,(@Role3,@rightId37,@UserId,GETDATE())
									   ,(@Role3,@rightId20,@UserId,GETDATE())
									   ,(@Role3,@rightId5,@UserId,GETDATE())
									   ,(@Role3,@rightId10,@UserId,GETDATE())
									   ,(@Role3,@rightId15,@UserId,GETDATE())
									   ,(@Role3,@rightId16,@UserId,GETDATE())
									   ,(@Role3,@rightId17,@UserId,GETDATE())
									   ,(@Role3,@rightId27,@UserId,GETDATE())
									   ,(@Role3,@rightId45,@UserId,GETDATE())
									   ,(@Role3,@rightId21,@UserId,GETDATE())
                                       ,(@Role3,@rightId19,@UserId,GETDATE())
									   ,(@Role3,@rightId28,@UserId,GETDATE())
									   ,(@Role3,@rightId35,@UserId,GETDATE())
									   ,(@Role3,@rightId36,@UserId,GETDATE())
									   ,(@Role3,@rightId41,@UserId,GETDATE())
									   ,(@Role3,@rightId22,@UserId,GETDATE())
									   ,(@Role3,@rightId24,@UserId,GETDATE())
									   ,(@Role3,@rightId47,@UserId,GETDATE())
									   ,(@Role3,@rightId25,@UserId,GETDATE())
									   ,(@Role3,@rightId31,@UserId,GETDATE())
                                       ,(@Role3,@rightId32,@UserId,GETDATE())
									   ,(@Role3,@rightId33,@UserId,GETDATE())
									   ,(@Role3,@rightId34,@UserId,GETDATE())
									   ,(@Role3,@rightId8,@UserId,GETDATE())
									   ,(@Role3,@rightId6,@UserId,GETDATE())
									   ,(@Role3,@rightId30,@UserId,GETDATE())
									   ,(@Role3,@rightId38,@UserId,GETDATE())
									   ,(@Role3,@rightId39,@UserId,GETDATE())
									   ,(@Role3,@rightId42,@UserId,GETDATE())
									   ,(@Role3,@rightId11,@UserId,GETDATE())
									   ,(@Role3,@rightId43,@UserId,GETDATE())
									   ,(@Role3,@rightId44,@UserId,GETDATE())
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role4 and RightId in (@rightId19,@rightId38,@rightId42,@rightId43,@rightId44))
BEGIN
  INSERT INTO Security.RoleRight values (@Role4,@rightId19,@UserId,GETDATE())
                                       ,(@Role4,@rightId38,@UserId,GETDATE())
									   ,(@Role4,@rightId42,@UserId,GETDATE())
									   ,(@Role4,@rightId43,@UserId,GETDATE())
									   ,(@Role4,@rightId44,@UserId,GETDATE())
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role5 and RightId in (@rightId13,@rightId14,@rightId3,@rightId46
,@rightId1,@rightId2,@rightId12,@rightId4,@rightId9,@rightId40,@rightId7,@rightId37,@rightId20,@rightId5,@rightId10,@rightId15,@rightId16
,@rightId17,@rightId45,@rightId21,@rightId19,@rightId36,@rightId41,@rightId22,@rightId24,@rightId47,@rightId25,@rightId31,@rightId32,
@rightId33,@rightId30,@rightId8,@rightId38,@rightId6,@rightId39,@rightId42,@rightId43,@rightId44,@rightId11
))
BEGIN
  INSERT INTO Security.RoleRight values (@Role5,@rightId13,@UserId,GETDATE())
                                       ,(@Role5,@rightId14,@UserId,GETDATE())
									   ,(@Role5,@rightId3,@UserId,GETDATE())
									   ,(@Role5,@rightId46,@UserId,GETDATE())
									   ,(@Role5,@rightId1,@UserId,GETDATE())
									   ,(@Role5,@rightId2,@UserId,GETDATE())
                                       ,(@Role5,@rightId12,@UserId,GETDATE())
									   ,(@Role5,@rightId4,@UserId,GETDATE())
									   ,(@Role5,@rightId9,@UserId,GETDATE())
									   ,(@Role5,@rightId40,@UserId,GETDATE())
									   ,(@Role5,@rightId7,@UserId,GETDATE())
                                       ,(@Role5,@rightId37,@UserId,GETDATE())
									   ,(@Role5,@rightId20,@UserId,GETDATE())
									   ,(@Role5,@rightId5,@UserId,GETDATE())
									   ,(@Role5,@rightId10,@UserId,GETDATE())
									   ,(@Role5,@rightId15,@UserId,GETDATE())
                                       ,(@Role5,@rightId16,@UserId,GETDATE())
									   ,(@Role5,@rightId17,@UserId,GETDATE())
									   ,(@Role5,@rightId45,@UserId,GETDATE())
									   ,(@Role5,@rightId21,@UserId,GETDATE())
									   ,(@Role5,@rightId19,@UserId,GETDATE())
                                       ,(@Role5,@rightId36,@UserId,GETDATE())
									   ,(@Role5,@rightId41,@UserId,GETDATE())
									   ,(@Role5,@rightId22,@UserId,GETDATE())
									   ,(@Role5,@rightId24,@UserId,GETDATE())
									   ,(@Role5,@rightId47,@UserId,GETDATE())
                                       ,(@Role5,@rightId25,@UserId,GETDATE())
									   ,(@Role5,@rightId31,@UserId,GETDATE())
									   ,(@Role5,@rightId32,@UserId,GETDATE())
									   ,(@Role5,@rightId33,@UserId,GETDATE())
									   ,(@Role5,@rightId30,@UserId,GETDATE())
                                       ,(@Role5,@rightId8,@UserId,GETDATE())
									   ,(@Role5,@rightId38,@UserId,GETDATE())
									   ,(@Role5,@rightId6,@UserId,GETDATE())
									   ,(@Role5,@rightId39,@UserId,GETDATE())
									   ,(@Role5,@rightId42,@UserId,GETDATE())
                                       ,(@Role5,@rightId43,@UserId,GETDATE())
									   ,(@Role5,@rightId44,@UserId,GETDATE())
									   ,(@Role5,@rightId11,@UserId,GETDATE())							   
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role6 and RightId in (@rightId9,@rightId15,@rightId19,@rightId36,
@rightId47,@rightId25,@rightId32,@rightId8,@rightId42,@rightId43,@rightId44,@rightId11
))
BEGIN
  INSERT INTO Security.RoleRight values (@Role6,@rightId9,@UserId,GETDATE())
                                       ,(@Role6,@rightId15,@UserId,GETDATE())
									   ,(@Role6,@rightId19,@UserId,GETDATE())
									   ,(@Role6,@rightId36,@UserId,GETDATE())
									   ,(@Role6,@rightId47,@UserId,GETDATE())
                                       ,(@Role6,@rightId25,@UserId,GETDATE())
									   ,(@Role6,@rightId32,@UserId,GETDATE())
									   ,(@Role6,@rightId8,@UserId,GETDATE())
									   ,(@Role6,@rightId42,@UserId,GETDATE())
                                       ,(@Role6,@rightId43,@UserId,GETDATE())
									   ,(@Role6,@rightId44,@UserId,GETDATE())
									   ,(@Role6,@rightId11,@UserId,GETDATE())
									  
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role7 and RightId in (
@rightId13,@rightId14,@rightId3,@rightId46,@rightId1,@rightId2,@rightId12,@rightId4,@rightId9,@rightId40,@rightId7,@rightId37,@rightId20,
@rightId5,@rightId10,@rightId15,@rightId16,@rightId45,@rightId21,@rightId19,@rightId18,@rightId29,@rightId28,@rightId35,@rightId36
,@rightId41,@rightId22,@rightId24,@rightId47,@rightId25,@rightId31,@rightId32,@rightId8,@rightId6
))
BEGIN
  INSERT INTO Security.RoleRight values (@Role7,@rightId13,@UserId,GETDATE())
                                       ,(@Role7,@rightId14,@UserId,GETDATE())
									   ,(@Role7,@rightId3,@UserId,GETDATE())
									   ,(@Role7,@rightId46,@UserId,GETDATE())
									   ,(@Role7,@rightId1,@UserId,GETDATE())
									   ,(@Role7,@rightId2,@UserId,GETDATE())
									   ,(@Role7,@rightId12,@UserId,GETDATE())
									   ,(@Role7,@rightId4,@UserId,GETDATE())
									   ,(@Role7,@rightId9,@UserId,GETDATE())
									   ,(@Role7,@rightId40,@UserId,GETDATE())
									   ,(@Role7,@rightId7,@UserId,GETDATE())
                                       ,(@Role7,@rightId37,@UserId,GETDATE())
									   ,(@Role7,@rightId20,@UserId,GETDATE())
									   ,(@Role7,@rightId5,@UserId,GETDATE())
									   ,(@Role7,@rightId10,@UserId,GETDATE())
									   ,(@Role7,@rightId15,@UserId,GETDATE())
									   ,(@Role7,@rightId16,@UserId,GETDATE())
									   ,(@Role7,@rightId17,@UserId,GETDATE())
									   ,(@Role7,@rightId45,@UserId,GETDATE())
									   ,(@Role7,@rightId21,@UserId,GETDATE())
									   ,(@Role7,@rightId19,@UserId,GETDATE())
                                       ,(@Role7,@rightId18,@UserId,GETDATE())
									   ,(@Role7,@rightId29,@UserId,GETDATE())
									   ,(@Role7,@rightId28,@UserId,GETDATE())
									   ,(@Role7,@rightId35,@UserId,GETDATE())
									   ,(@Role7,@rightId36,@UserId,GETDATE())
									   ,(@Role7,@rightId41,@UserId,GETDATE())
									   ,(@Role7,@rightId22,@UserId,GETDATE())
									   ,(@Role7,@rightId24,@UserId,GETDATE())
									   ,(@Role7,@rightId47,@UserId,GETDATE())
									   ,(@Role7,@rightId31,@UserId,GETDATE())
                                       ,(@Role7,@rightId32,@UserId,GETDATE())
									   ,(@Role7,@rightId26,@UserId,GETDATE())
									   ,(@Role7,@rightId34,@UserId,GETDATE())
									   ,(@Role7,@rightId33,@UserId,GETDATE())
									   ,(@Role7,@rightId30,@UserId,GETDATE())
									   ,(@Role7,@rightId8,@UserId,GETDATE())
									   ,(@Role7,@rightId38,@UserId,GETDATE())
									   ,(@Role7,@rightId6,@UserId,GETDATE())
									   ,(@Role7,@rightId39,@UserId,GETDATE())
									   ,(@Role7,@rightId42,@UserId,GETDATE())
                                       ,(@Role7,@rightId43,@UserId,GETDATE())
									   ,(@Role7,@rightId44,@UserId,GETDATE())
									   ,(@Role7,@rightId11,@UserId,GETDATE())  
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @Role8 and RightId in (@rightId13,@rightId14,@rightId3,@rightId46,@rightId1,
@rightId2,@rightId12,@rightId4,@rightId9,@rightId40,@rightId7,@rightId37,@rightId20,@rightId5,@rightId10,@rightId15,@rightId16,@rightId6
,@rightId43,@rightId11))
BEGIN
  INSERT INTO Security.RoleRight values (@Role8,@rightId13,@UserId,GETDATE())
                                       ,(@Role8,@rightId14,@UserId,GETDATE())
									   ,(@Role8,@rightId3,@UserId,GETDATE())
									   ,(@Role8,@rightId46,@UserId,GETDATE())
									   ,(@Role8,@rightId1,@UserId,GETDATE())
									   ,(@Role8,@rightId2,@UserId,GETDATE())
									   ,(@Role8,@rightId12,@UserId,GETDATE())
									   ,(@Role8,@rightId4,@UserId,GETDATE())
									   ,(@Role8,@rightId9,@UserId,GETDATE())
									   ,(@Role8,@rightId40,@UserId,GETDATE())
									   ,(@Role8,@rightId7,@UserId,GETDATE())
                                       ,(@Role8,@rightId37,@UserId,GETDATE())
									   ,(@Role8,@rightId20,@UserId,GETDATE())
									   ,(@Role8,@rightId5,@UserId,GETDATE())
									   ,(@Role8,@rightId10,@UserId,GETDATE())
									   ,(@Role8,@rightId15,@UserId,GETDATE())
									   ,(@Role8,@rightId16,@UserId,GETDATE())
									   ,(@Role8,@rightId17,@UserId,GETDATE())
									   ,(@Role8,@rightId45,@UserId,GETDATE())
									   ,(@Role8,@rightId21,@UserId,GETDATE())
									   ,(@Role8,@rightId19,@UserId,GETDATE())
                                       ,(@Role8,@rightId18,@UserId,GETDATE())
									   ,(@Role8,@rightId36,@UserId,GETDATE())
									   ,(@Role8,@rightId23,@UserId,GETDATE())
									   ,(@Role8,@rightId41,@UserId,GETDATE())
									   ,(@Role8,@rightId22,@UserId,GETDATE())
									   ,(@Role8,@rightId24,@UserId,GETDATE())
									   ,(@Role8,@rightId47,@UserId,GETDATE())
									   ,(@Role8,@rightId25,@UserId,GETDATE())
									   ,(@Role8,@rightId31,@UserId,GETDATE())
									   ,(@Role8,@rightId32,@UserId,GETDATE())
                                       ,(@Role8,@rightId26,@UserId,GETDATE())
									   ,(@Role8,@rightId34,@UserId,GETDATE())
									   ,(@Role8,@rightId33,@UserId,GETDATE())
									   ,(@Role8,@rightId30,@UserId,GETDATE())
									   ,(@Role8,@rightId8,@UserId,GETDATE())
									   ,(@Role8,@rightId38,@UserId,GETDATE())
									   ,(@Role8,@rightId6,@UserId,GETDATE())
									   ,(@Role8,@rightId42,@UserId,GETDATE())
									   ,(@Role8,@rightId43,@UserId,GETDATE())
									   ,(@Role8,@rightId44,@UserId,GETDATE())
									   ,(@Role8,@rightId11,@UserId,GETDATE())
END
GO