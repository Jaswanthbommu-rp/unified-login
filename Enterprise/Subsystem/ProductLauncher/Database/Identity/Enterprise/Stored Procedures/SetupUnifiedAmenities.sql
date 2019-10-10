
CREATE PROCEDURE [Enterprise].[SetupUnifiedAmenities](@PartyId BIGINT)
AS
     BEGIN
         SET NOCOUNT ON;
         DECLARE @PersonaId INT;
         DECLARE @FromDate DATETIME;
         DECLARE @TRoleId INT;
         DECLARE @TRoleName NVARCHAR(500);
         DECLARE @TRoleDesc NVARCHAR(500);
         DECLARE @TRoleShortName NVARCHAR(100);
         DECLARE @TRightShortName NVARCHAR(100);
         DECLARE @TRightId INT;
         DECLARE @TRightName NVARCHAR(500);
         DECLARE @TRightDesc NVARCHAR(500);
         DECLARE @RoleId INT;
         DECLARE @RightId INT;
         DECLARE @RightCategory INT;
         DECLARE @RoleCategory INT;
         DECLARE @ProductId INT;
         DECLARE @RoleName NVARCHAR(500);
         DECLARE @RightName NVARCHAR(500);
         DECLARE @RoleTypeID INT;
         DECLARE @PerosonaP INT;
         DECLARE @PartyRowNum INT;
         SET @FromDate = GETUTCDATE();

         SELECT @RoleTypeId = PartyROleTypeId
         FROM enterprise.roletype
         WHERE Name = 'Product Role';
         SELECT @ProductId = ProductId
         FROM Enterprise.Product
         WHERE name = 'Unified Amenities';
         SELECT @RoleCategory = ST.StatusTypeId
         FROM Enterprise.StatusTypeCategoryType AS STCT
              JOIN Enterprise.StatusTypeCategory AS STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
              JOIN Enterprise.StatusTypeCategoryClassification AS STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
              JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = STCC.StatusTypeId
         WHERE STC.Name = 'Role Type'
               AND ST.Name = 'Default';
         SELECT @RightCategory = ST.StatusTypeId
         FROM Enterprise.StatusTypeCategoryType AS STCT
              JOIN Enterprise.StatusTypeCategory AS STC ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
              JOIN Enterprise.StatusTypeCategoryClassification AS STCC ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
              JOIN Enterprise.StatusType AS ST ON ST.StatusTypeId = STCC.StatusTypeId
         WHERE STC.Name = 'Right Type'
               AND ST.Name = 'Default';
         DECLARE @UARole TABLE
			(RoleID      INT,
			 Name        NVARCHAR(500),
			 ShortName   VARCHAR(100),
			 Description NVARCHAR(500)
			);
         INSERT INTO @UARole
			(RoleId,
			 Name,
			 ShortName,
			 Description
			)
         VALUES
			(1,
			 'Manage Amenity Status',
			 'amenity.status',
			 'Ability to inactivate master amenities '
			),
			(2,
			 'Manage Amenity No Pricing',
			 'amenity.no.pricing',
			 'Create and update master amenity marketing content but not pricing '
			),
			(3,
			 'Manage Amenity With Pricing',
			 'amenity.with.pricing',
			 'Create and update master amenity pricing content'
			),
			(4,
			 'Manage Property Amenity No Pricing',
			 'prop.amenity.no.pricing',
			 'Create and update property amenity marketing content but not pricing '
			),
			(5,
			 'Manage Property Amenity With Pricing',
			 'prop.amenity.with.pricing',
			 'Create and update property amenity pricing '
			),
			(6,
			 'View Amenities',
			 'view.amenities',
			 'View master amenities (default access to application - only allows users to view (read-only) amenities list)'
			);
         DECLARE @UARight TABLE
			(RightId     INT,
			 Name        NVARCHAR(500),
			 ShortName   NVARCHAR(100),
			 description NVARCHAR(500)
			);
         INSERT INTO @UARight
			(rightid,
			 name,
			 shortname,
			 description
			)
         VALUES
			(1,
			 'Assign Unit Amenities',
			 'assign.unit',
			 'Ability to Assign Unit Amenities'
			),
			(2,
			 'Un-Assign Unit Amenities',
			 'unassign.unit',
			 'Ability to Un-Assign Unit Amenities'
			),
			(3,
			 'Assign Floorplan Amenities',
			 'assign.floorplan',
			 'Ability to Assign Floorplan Amenities'
			),
			(4,
			 'Un-Assign Floorplan Amenities',
			 'unassign.floorplan',
			 'Ability to Un-Assign Floorplan Amenities'
			),
			(5,
			 'Add Common Area Amenity',
			 'ca.add',
			 'Ability to Add Common Area Amenity'
			),
			(6,
			 'Edit Common Area Amenity',
			 'ca.edit',
			 'Ability to Edit Common Area Amenity'
			),
			(7,
			 'Delete Common Area Amenity',
			 'ca.delete',
			 'Ability to Delete Common Area Amenity'
			),
			(8,
			 'Add Floorplan Unit Amenity',
			 'fpu.add',
			 'Ability to Add Floorplan Unit Amenity'
			),
			(9,
			 'Edit Floorplan Unit Amenity',
			 'fpu.edit',
			 'Ability to Edit Floorplan Unit Amenity'
			),
			(10,
			 'Delete Floorplan Unit Amenity',
			 'fpu.delete',
			 'Ability to Delete Floorplan Unit Amenity'
			),
			(11,
			 'Merge Common Area Amenities',
			 'ca.merge',
			 'Ability to Merge Common Area Amenities'
			),
			(12,
			 'Merge Floorplan Unit Amenities',
			 'fpu.merge',
			 'Ability to Merge Floorplan Unit Amenities'
			),
			(13,
			 'Export Common Area Amenities',
			 'ca.export',
			 'Ability to Export Common Area Amenities'
			),
			(14,
			 'Export Floorplan unit Amenities',
			 'fpu.export',
			 'Ability to Export Floorplan unit Amenities'
			),
			(15,
			 'Assign to Properties',
			 'assignto.prop',
			 'Ability to Assign to Properties'
			),
			(16,
			 'Add Default Pricing',
			 'defaultpricing.add',
			 'Ability to Add Default Pricing'
			),
			(17,
			 'Edit Default Pricing',
			 'defaultpricing.edit',
			 'Ability to Edit Default Pricing'
			),
			(18,
			 'Delete Default Pricing',
			 'defaultpricing.delete',
			 'Ability to Delete Default Pricing'
			),
			(19,
			 'Turn On Depriciation Setting',
			 'depriciation.on',
			 'Ability to Turn On Depriciation Setting'
			),
			(20,
			 'Turn Off Depriciation Setting',
			 'depriciation.off',
			 'Ability to Turn Off Depriciation Setting'
			),
			(21,
			 'Add Property Common Area Amenity',
			 'prop.ca.add',
			 'Ability to Add Property Common Area Amenity'
			),
			(22,
			 'Edit Property Common Area Amenity',
			 'prop.ca.edit',
			 'Ability to Edit Property Common Area Amenity'
			),
			(23,
			 'Delete Property Common Area Amenity',
			 'prop.ca.delete',
			 'Ability to Delete Property Common Area Amenity'
			),
			(24,
			 'Add Property Floorplan Unit Amenity',
			 'prop.fpu.add',
			 'Ability to Add Property Floorplan Unit Amenity'
			),
			(25,
			 'Edit Property Floorplan Unit Amenity',
			 'prop.fpu.edit',
			 'Ability to Edit Property Floorplan Unit Amenity'
			),
			(26,
			 'Delete Property Floorplan Unit Amenity',
			 'prop.fpu.delete',
			 'Ability to Delete Property Floorplan Unit Amenity'
			),
			(27,
			 'Merge Property Common Area Amenities',
			 'prop.ca.merge',
			 'Ability to Merge Property Common Area Amenities'
			),
			(28,
			 'Merge Property Floorplan Unit Amenities',
			 'prop.fpu.merge',
			 'Ability to Merge Property Floorplan Unit Amenities'
			),
			(29,
			 'Export Property Common Area Amenities',
			 'prop.ca.export',
			 'Ability to Export Property Common Area Amenities'
			),
			(30,
			 'Export Property Flroorplan unit Amenities',
			 'prop.fpu.export',
			 'Ability to Export Property Flroorplan unit Amenities'
			),
			(31,
			 'Add Price Points',
			 'price.point.add',
			 'Ability to Add Price Points'
			),
			(32,
			 'Edit Price Points',
			 'price.point.edit',
			 'Ability to Edit Price Points'
			),
			(33,
			 'Delete Price Points',
			 'price.point.delete',
			 'Ability to Delete Price Points'
			),
			(34,
			 'Add Depreciation Schedule',
			 'depreciation.add',
			 'Ability to Add Depreciation Schedule'
			),
			(35,
			 'Edit Depreciation Schedule',
			 'depreciation.edit',
			 'Ability to Edit Depreciation Schedule'
			),
			(36,
			 'View Common Area Amenites Master List',
			 'ca.master.view',
			 'Ability to View Common Area Amenites Master List'
			),
			(37,
			 'View Common Area Amenites by Property',
			 'ca.property.view',
			 'Ability to View Common Area Amenites by Property'
			),
			(38,
			 'View Floorplan Unit amenities master list',
			 'fpu.master.view',
			 'Ability to View Floorplan Unit amenities master list'
			),
			(39,
			 'View Floorplan Unit amenities by property',
			 'fpu.property.view',
			 'Ability to View Floorplan Unit amenities by property'
			),
			(40,
			 'View Activity Page',
			 'activity.view',
			 'Ability to View Activity Page'
			);
         DECLARE @UAMapping TABLE
			(RoleId  INT,
			 RightId INT
			);

INSERT INTO @UAMapping( RoleId, RightId )
VALUES( 1, 1 ), ( 1, 2 ), ( 1, 3 ), ( 1, 4 ), ( 2, 1 ), ( 2, 2 ), ( 2, 3 ), ( 2, 4 ), ( 3, 1 ), ( 3, 2 ), ( 3, 3 ), ( 3, 4 ), ( 4, 1 ), ( 4, 2 ), ( 4, 3 ), ( 4, 4 ), ( 5, 1 ), ( 5, 2 ), ( 5, 3 ), ( 5, 4 ), ( 2, 5 ), ( 2, 6 ), ( 2, 7 ), ( 2, 8 ), ( 2, 9 ), ( 2, 10 ), ( 2, 11 ), ( 2, 12 ), ( 2, 13 ), ( 2, 14 ), ( 2, 15 ), ( 3, 5 ), ( 3, 6 ), ( 3, 7 ), ( 3, 8 ), ( 3, 9 ), ( 3, 10 ), ( 3, 11 ), ( 3, 12 ), ( 3, 13 ), ( 3, 14 ), ( 3, 15 ), ( 3, 16 ), ( 3, 17 ), ( 3, 18 ), ( 3, 19 ), ( 3, 20 ), ( 2, 21 ), ( 2, 22 ), ( 2, 23 ), ( 2, 24 ), ( 2, 25 ), ( 2, 26 ), ( 2, 27 ), ( 2, 28 ), ( 2, 29 ), ( 2, 30 ), ( 3, 21 ), ( 3, 22 ), ( 3, 23 ), ( 3, 24 ), ( 3, 25 ), ( 3, 26 ), ( 3, 27 ), ( 3, 28 ), ( 3, 29 ), ( 3, 30 ), ( 4, 21 ), ( 4, 22 ), ( 4, 23 ), ( 4, 24 ), ( 4, 25 ), ( 4, 26 ), ( 4, 27 ), ( 4, 28 ), ( 4, 29 ), ( 4, 30 ), ( 5, 21 ), ( 5, 22 ), ( 5, 23 ), ( 5, 24 ), ( 5, 25 ), ( 5, 26 ), ( 5, 27 ), ( 5, 28 ), ( 5, 29 ), ( 5, 30 ), ( 3, 31 ), ( 3, 32 ), ( 3, 33 ), ( 3, 34 ), ( 3, 35 ), ( 5, 31 ), ( 5, 32 ), ( 5, 33 ), ( 5, 34 ), ( 5, 35 ), ( 1, 36 ), ( 1, 37 ), ( 1, 38 ), ( 1, 39 ), ( 1, 40 ), ( 2, 36 ), ( 2, 37 ), ( 2, 38 ), ( 2, 39 ), ( 2, 40 ), ( 3, 36 ), ( 3, 37 ), ( 3, 38 ), ( 3, 39 ), ( 3, 40 ), ( 4, 36 ), ( 4, 37 ), ( 4, 38 ), ( 4, 39 ), ( 4, 40 ), ( 5, 36 ), ( 5, 37 ), ( 5, 38 ), ( 5, 39 ), ( 5, 40 ), ( 6, 36 ), ( 6, 37 ), ( 6, 38 ), ( 6, 39 ), ( 6, 40 );
         DECLARE @HoldPartyid TABLE
			(ROwnumber           INT IDENTITY(1, 1),
			 OrganizationPartyId INT,
			 PStatus             BIT DEFAULT 0
			);
         INSERT INTO @HoldPartyid(OrganizationPartyId)
         VALUES(@PartyId);


--select * from enterprise.organization where name  In (N'American Landmark Management LLC')
--select * from enterprise.dataimportmapping where sourceid in (2416 , 1076 )


         WHILE EXISTS
		(
			SELECT 1
			FROM @HoldPartyid
			WHERE PStatus = 0
		)
             BEGIN
                 SELECT TOP 1 @PartyRowNum = Rownumber,
                              @PartyId = OrganizationPartyID
                 FROM @HoldPartyid
                 WHERE PStatus = 0;
                 DECLARE Roles CURSOR
                 FOR SELECT RoleId,
                            Name,
                            ShortName,
                            Description
                     FROM @UARole;
                 OPEN Roles;
                 FETCH Roles INTO @TRoleId, @TRoleName, @TRoleShortName, @TRoleDesc;
                 WHILE @@FETCH_STATUS = 0
                     BEGIN
                         EXECUTE Enterprise.CreateRole
                                 @RoleName = @TRoleName,
                                 @Shortname = @TRoleShortName,
                                 @Description = @TRoleDesc,
                                 @RoleTypeId = @RoleTypeId,
                                 @RoleCategoryId = @RoleCategory,
                                 @PartyId = @PartyId,
                                 @RoleId = @RoleId OUTPUT;
                         DECLARE RightsL CURSOR
                         FOR SELECT Name,
                                    Description,
                                    ShortName,
                                    b.RightId
                             FROM @UARight AS a
                                  INNER JOIN @UAMapping AS b ON a.RightId = b.RightId
                                                                AND b.RoleID = @TRoleId;
                         OPEN RightsL;
                         FETCH RightsL INTO @TRightName, @TRightDesc, @TRightShortName, @TRightId;
                         WHILE @@FETCH_STATUS = 0
                             BEGIN
                                 EXECUTE Enterprise.CreateRight
                                         @RoleId = @RoleId,
                                         @RightName = @TRightName,
                                         @RightCategoryId = @RightCategory,
                                         @PartyId = @PartyId,
                                         @ProductId = @ProductId,
                                         @Shortname = @TRightShortName,
                                         @Description = @TRightDesc,
                                         @targetProductid = @productId,
                                         @RightId = @RightId OUTPUT;
                                 FETCH RightsL INTO @TRightName, @TRightDesc, @TRightShortName, @TRightId;
                             END;
                         CLOSE RightsL;
                         DEALLOCATE RightsL;
                         FETCH Roles INTO @TRoleId, @TRoleName, @TRoleShortName, @TRoleDesc;
                     END;
                 CLOSE Roles;
                 DEALLOCATE Roles;
                 UPDATE @HoldPartyid
                   SET
                       PStatus = 1
                 WHERE RowNumber = @PartyRowNum;
             END;
         UPDATE r
           SET
               DefaultRole = 1
         FROM enterprise.rolevaluetype rv
              INNER JOIN enterprise.role r ON r.rolevaluetypeid = rv.rolevaluetypeid
         WHERE rv.value = 'view amenities'
               AND R.PartyId = @PartyId;
     END;