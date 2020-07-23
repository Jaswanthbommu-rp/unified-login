
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
			 'Assign unit amenities',
			 'assign.unit',
			 'Ability to assign unit amenities'
			),
			(2,
			 'Unassign unit amenities',
			 'unassign.unit',
			 'Ability to unassign unit amenities'
			),
			(3,
			 'Assign floorplan amenities',
			 'assign.floorplan',
			 'Ability to assign floorplan amenities'
			),
			(4,
			 'Unassign floorplan amenities',
			 'unassign.floorplan',
			 'Ability to unassign floorplan amenities'
			),
			(5,
			 'Add common area amenity',
			 'ca.add',
			 'Ability to add common area amenity'
			),
			(6,
			 'Edit common area amenity',
			 'ca.edit',
			 'Ability to edit common area amenity'
			),
			(7,
			 'Delete common area amenity',
			 'ca.delete',
			 'Ability to delete common area amenity'
			),
			(8,
			 'Add floorplan unit amenity',
			 'fpu.add',
			 'Ability to add floorplan unit amenity'
			),
			(9,
			 'Edit floorplan unit amenity',
			 'fpu.edit',
			 'Ability to edit floorplan unit amenity'
			),
			(10,
			 'Delete floorplan unit amenity',
			 'fpu.delete',
			 'Ability to delete floorplan unit amenity'
			),
			(11,
			 'Merge common area amenities',
			 'ca.merge',
			 'Ability to merge common area amenities'
			),
			(12,
			 'Merge floorplan unit amenities',
			 'fpu.merge',
			 'Ability to merge floorplan unit amenities'
			),
			(13,
			 'Export common area amenities',
			 'ca.export',
			 'Ability to export common area amenities'
			),
			(14,
			 'Export floorplan unit amenities',
			 'fpu.export',
			 'Ability to export floorplan unit amenities'
			),
			(15,
			 'Assign to properties',
			 'assignto.prop',
			 'Ability to assign to properties'
			),
			(16,
			 'Add default pricing',
			 'defaultpricing.add',
			 'Ability to add default pricing'
			),
			(17,
			 'Edit default pricing',
			 'defaultpricing.edit',
			 'Ability to edit default pricing'
			),
			(18,
			 'Delete default pricing',
			 'defaultpricing.delete',
			 'Ability to delete default pricing'
			),
			(19,
			 'Turn on depreciation setting',
			 'depriciation.on',
			 'Ability to turn on depreciation setting'
			),
			(20,
			 'Turn off depreciation setting',
			 'depriciation.off',
			 'Ability to turn off depreciation setting'
			),
			(21,
			 'Add property common area amenity',
			 'prop.ca.add',
			 'Add property common area amenity'
			),
			(22,
			 'Edit property common area amenity',
			 'prop.ca.edit',
			 'Ability to edit property common area amenity'
			),
			(23,
			 'Delete property common area amenity',
			 'prop.ca.delete',
			 'Ability to delete property common area amenity'
			),
			(24,
			 'Add property floorplan unit amenity',
			 'prop.fpu.add',
			 'Ability to add property floorplan unit amenity'
			),
			(25,
			 'Edit property floorplan unit amenity',
			 'prop.fpu.edit',
			 'Ability to edit property floorplan unit amenity'
			),
			(26,
			 'Delete property floorplan unit amenity',
			 'prop.fpu.delete',
			 'Ability to delete property floorplan unit amenity'
			),
			(27,
			 'Merge property common area amenities',
			 'prop.ca.merge',
			 'Ability to merge property common area amenities'
			),
			(28,
			 'Merge property floorplan unit amenities',
			 'prop.fpu.merge',
			 'Ability to merge property floorplan unit amenities'
			),
			(29,
			 'Export property common area amenities',
			 'prop.ca.export',
			 'Ability to export property common area amenities'
			),
			(30,
			 'Export property floorplan unit amenities',
			 'prop.fpu.export',
			 'Ability to export property floorplan unit amenities'
			),
			(31,
			 'Add price points',
			 'price.point.add',
			 'Ability to add price points'
			),
			(32,
			 'Edit price points',
			 'price.point.edit',
			 'Ability to edit price points'
			),
			(33,
			 'Delete price points',
			 'price.point.delete',
			 'Ability to delete price points'
			),
			(34,
			 'Add depreciation schedule',
			 'depreciation.add',
			 'Ability to add depreciation schedule'
			),
			(35,
			 'Edit depreciation schedule',
			 'depreciation.edit',
			 'Ability to edit depreciation schedule'
			),
			(36,
			 'View common area amenities master list',
			 'ca.master.view',
			 'Ability to view common area amenities master list'
			),
			(37,
			 'View common area amenities by property',
			 'ca.property.view',
			 'Ability to view common area amenities by property'
			),
			(38,
			 'View floorplan unit amenities master list',
			 'fpu.master.view',
			 'Ability to view floorplan unit amenities master list'
			),
			(39,
			 'View floorplan unit amenities by property',
			 'fpu.property.view',
			 'Ability to view floorplan unit amenities by property'
			),
			(40,
			 'View activity page',
			 'activity.view',
			 'Ability to view activity page'
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
	    DECLARE @UserId bigint,@UARoleId Int
		SELECT	@UserId = UserId FROM	Ident.UserLogin WHERE	LoginName LIKE 'realpagead@%'

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
								 @CreatedBy = @UserId,
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