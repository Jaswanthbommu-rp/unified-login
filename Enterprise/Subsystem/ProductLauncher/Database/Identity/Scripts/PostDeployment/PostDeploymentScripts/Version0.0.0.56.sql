
SET NOCOUNT ON;
--DECLARE @PersonaId INT;
--DECLARE @FromDate DATETIME;
--DECLARE @TRoleId INT;
--DECLARE @TRoleName NVARCHAR(500);
--DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRoleShortName NVARCHAR(100);
DECLARE @TRightShortName NVARCHAR(100);
--DECLARE @TRightId INT;
--DECLARE @TRightName NVARCHAR(500);
--DECLARE @TRightDesc NVARCHAR(500);
--DECLARE @RoleId INT;
--DECLARE @RightId INT;
--DECLARE @RightCategory INT;
--DECLARE @RoleCategory INT;
--DECLARE @PartyId INT;
--DECLARE @ProductId INT;
--DECLARE @RoleName NVARCHAR(500);
--DECLARE @RightName NVARCHAR(500);
--DECLARE @RoleTypeID INT;
--DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;
SET @FromDate = GETUTCDATE();

IF OBJECT_ID('tempdb..#HoldPartyId') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyId;
END;

IF OBJECT_ID('tempdb..#UARole') IS NOT NULL
BEGIN
	DROP TABLE #UARole;
END;

IF OBJECT_ID('tempdb..#UARight') IS NOT NULL
BEGIN
	DROP TABLE #UARight;
END;

IF OBJECT_ID('tempdb..#UAMapping') IS NOT NULL
BEGIN
	DROP TABLE #UAMapping;
END;

SELECT @RoleTypeId = PartyROleTypeId
FROM enterprise.roletype
WHERE Name = 'Product Role';

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Amenities';

SELECT @RoleCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'Default';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'Default';

CREATE TABLE #UARole
( 
			 RoleID int, Name nvarchar(500), ShortName varchar(100), Description nvarchar(500)
);

INSERT INTO #UARole( RoleId, Name, ShortName, Description )
VALUES( 1, 'Manage Amenity Status', 'Amenity.Status', 'Ability to inactivate master amenities ' ), ( 2, 'Manage Amenity No Pricing', 'Amenity.No.Pricing', 'Create and update master amenity marketing content but not pricing ' ), ( 3, 'Manage Amenity With Pricing', 'Amenity.With.Pricing', 'Create and update master amenity pricing content' ), ( 4, 'Manage Property Amenity No Pricing', 'Prop.Amenity.No.Pricing', 'Create and update property amenity marketing content but not pricing ' ), ( 5, 'Manage Property Amenity With Pricing', 'Prop.Amenity.With.Pricing', 'Create and update property amenity pricing ' ), ( 6, 'View Amenities', 'View.Amenities', 'View master amenities (default access to application - only allows users to view (read-only) amenities list)' );

CREATE TABLE #UARight
( 
			 RightId int, Name nvarchar(500), ShortName nvarchar(100), description nvarchar(500)
);

INSERT INTO #UARight( rightid, name, shortname, description )
VALUES( 1, 'Assign Unit Amenities', 'assign.unit', 'Ability to Assign Unit Amenities' ), ( 2, 'Un-Assign Unit Amenities', 'unassign.unit', 'Ability to Un-Assign Unit Amenities' ), ( 3, 'Assign Floorplan Amenities', 'assign.floorplan', 'Ability to Assign Floorplan Amenities' ), ( 4, 'Un-Assign Floorplan Amenities', 'unassign.floorplan', 'Ability to Un-Assign Floorplan Amenities' ), ( 5, 'Add Common Area Amenity', 'CA.add', 'Ability to Add Common Area Amenity' ), ( 6, 'Edit Common Area Amenity', 'CA.edit', 'Ability to Edit Common Area Amenity' ), ( 7, 'Delete Common Area Amenity', 'CA.delete', 'Ability to Delete Common Area Amenity' ), ( 8, 'Add Floorplan Unit Amenity', 'FPU.add', 'Ability to Add Floorplan Unit Amenity' ), ( 9, 'Edit Floorplan Unit Amenity', 'FPU.edit', 'Ability to Edit Floorplan Unit Amenity' ), ( 10, 'Delete Floorplan Unit Amenity', 'FPU.delete', 'Ability to Delete Floorplan Unit Amenity' ), ( 11, 'Merge Common Area Amenities', 'CA.merge', 'Ability to Merge Common Area Amenities' ), ( 12, 'Merge Floorplan Unit Amenities', 'FPU.merge', 'Ability to Merge Floorplan Unit Amenities' ), ( 13, 'Export Common Area Amenities', 'CA.export', 'Ability to Export Common Area Amenities' ), ( 14, 'Export Flroorplan unit Amenities', 'FPU.export', 'Ability to Export Flroorplan unit Amenities' ), ( 15, 'Assign to Properties', 'assignto.PROP', 'Ability to Assign to Properties' ), ( 16, 'Add Default Pricing', 'defaultpricing.add', 'Ability to Add Default Pricing' ), ( 17, 'Edit Default Pricing', 'defaultpricing.edit', 'Ability to Edit Default Pricing' ), ( 18, 'Delete Default Pricing', 'defaultpricing.delete', 'Ability to Delete Default Pricing' ), ( 19, 'Turn On Depriciation Setting', 'depriciation.on', 'Ability to Turn On Depriciation Setting' ), ( 20, 'Turn Off Depriciation Setting', 'depriciation.off', 'Ability to Turn Off Depriciation Setting' ), ( 21, 'Add Property Common Area Amenity', 'PROP.CA.add', 'Ability to Add Property Common Area Amenity' ), ( 22, 'Edit Property Common Area Amenity', 'PROP.CA.edit', 'Ability to Edit Property Common Area Amenity' ), ( 23, 'Delete Property Common Area Amenity', 'PROP.CA.delete', 'Ability to Delete Property Common Area Amenity' ), ( 24, 'Add Property Floorplan Unit Amenity', 'PROP.FPU.add', 'Ability to Add Property Floorplan Unit Amenity' ), ( 25, 'Edit Property Floorplan Unit Amenity', 'PROP.FPU.edit', 'Ability to Edit Property Floorplan Unit Amenity' ), ( 26, 'Delete Property Floorplan Unit Amenity', 'PROP.FPU.delete', 'Ability to Delete Property Floorplan Unit Amenity' ), ( 27, 'Merge Property Common Area Amenities', 'PROP.CA.merge', 'Ability to Merge Property Common Area Amenities' ), ( 28, 'Merge Property Floorplan Unit Amenities', 'PROP.FPU.merge', 'Ability to Merge Property Floorplan Unit Amenities' ), ( 29, 'Export Property Common Area Amenities', 'PROP.CA.export', 'Ability to Export Property Common Area Amenities' ), ( 30, 'Export Property Flroorplan unit Amenities', 'PROP.FPU.export', 'Ability to Export Property Flroorplan unit Amenities' ), ( 31, 'Add Price Points', 'price.point.add', 'Ability to Add Price Points' ), ( 32, 'Edit Price Points', 'price.point.edit', 'Ability to Edit Price Points' ), ( 33, 'Delete Price Points', 'price.point.delete', 'Ability to Delete Price Points' ), ( 34, 'Add Depreciation Schedule', 'depreciation.add', 'Ability to Add Depreciation Schedule' ), ( 35, 'Edit Depreciation Schedule', 'depreciation.edit', 'Ability to Edit Depreciation Schedule' ), ( 36, 'View Common Area Amenites Master List', 'CA.master.view', 'Ability to View Common Area Amenites Master List' ), ( 37, 'View Common Area Amenites by Property', 'CA.property.view', 'Ability to View Common Area Amenites by Property' ), ( 38, 'View Floorplan Unit amenities master list', 'FPU.master.view', 'Ability to View Floorplan Unit amenities master list' ), ( 39, 'View Floorplan Unit amenities by property', 'FPU.property.view', 'Ability to View Floorplan Unit amenities by property' ), ( 40, 'View Activity Page', 'activity.view', 'Ability to View Activity Page' );

CREATE TABLE #UAMapping
( 
			 RoleId int, RightId int
);


INSERT INTO #UAMapping( RoleId, RightId )
VALUES( 1, 1 ), ( 2, 1 ), ( 4, 1 ), ( 1, 2 ), ( 2, 2 ), ( 4, 2 ), ( 1, 3 ), ( 2, 3 ), ( 4, 3 ), ( 1, 4 ), ( 2, 4 ), ( 4, 4 ), ( 2, 5 ), ( 3, 5 ), ( 2, 6 ), ( 3, 6 ), ( 2, 7 ), ( 3, 7 ), ( 2, 8 ), ( 3, 8 ), ( 2, 9 ), ( 3, 9 ), ( 2, 10 ), ( 3, 10 ), ( 2, 11 ), ( 3, 11 ), ( 2, 12 ), ( 3, 12 ), ( 2, 13 ), ( 3, 13 ), ( 2, 14 ), ( 3, 14 ), ( 2, 15 ), ( 3, 15 ), ( 3, 16 ), ( 3, 17 ), ( 3, 18 ), ( 3, 19 ), ( 3, 20 ), ( 4, 21 ), ( 5, 21 ), ( 4, 22 ), ( 5, 22 ), ( 4, 23 ), ( 5, 23 ), ( 4, 24 ), ( 5, 24 ), ( 4, 25 ), ( 5, 25 ), ( 4, 26 ), ( 5, 26 ), ( 4, 27 ), ( 5, 27 ), ( 4, 28 ), ( 5, 28 ), ( 4, 29 ), ( 5, 29 ), ( 4, 30 ), ( 5, 30 ), ( 5, 31 ), ( 5, 32 ), ( 5, 33 ), ( 5, 34 ), ( 5, 35 ), ( 1, 36 ), ( 2, 36 ), ( 3, 36 ), ( 4, 36 ), ( 5, 36 ), ( 6, 36 ), ( 1, 37 ), ( 2, 37 ), ( 3, 37 ), ( 4, 37 ), ( 5, 37 ), ( 6, 37 ), ( 1, 38 ), ( 2, 38 ), ( 3, 38 ), ( 4, 38 ), ( 5, 38 ), ( 6, 38 ), ( 1, 39 ), ( 2, 39 ), ( 3, 39 ), ( 4, 39 ), ( 5, 39 ), ( 6, 39 ), ( 1, 40 ), ( 2, 40 ), ( 3, 40 ), ( 4, 40 ), ( 5, 40 ), ( 6, 40 );

CREATE INDEX IDX1
ON #UARole
( RoleId
);

CREATE INDEX IDX2
ON #UARight
( RightId
);

CREATE INDEX IDX3
ON #UAMapping
( RoleId
);

CREATE INDEX IDX4
ON #UAMapping
( RightId
);

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyId
FROM Person.Persona;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyId
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
	FROM #HoldPartyId
	WHERE PStatus = 0;
	DECLARE Roles CURSOR
	FOR SELECT RoleId, Name, ShortName, Description
		FROM #UARole;
	OPEN Roles;
	FETCH Roles INTO @TRoleId, @TRoleName, @TRoleShortName, @TRoleDesc;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		--Print 'Role: ' + @TRoleName + ' for Party: ' + convert(varchar, @PartyId)
		EXECUTE Enterprise.CreateRole @RoleName = @TRoleName, @Shortname = @TRoleShortName , @Description = @TRoleDesc, @RoleTypeId = @RoleTypeId, @RoleCategoryId = @RoleCategory, @PartyId = @PartyId, @RoleId = @RoleId OUTPUT;
		DECLARE RightsL CURSOR
		FOR SELECT Name, Description, ShortName,  b.RightId
			FROM #UARight AS a
				 INNER JOIN
				 #UAMapping AS b
				 ON a.RightId = b.RightId AND 
					b.RoleID = @TRoleId;
		OPEN RightsL;
		FETCH RightsL INTO @TRightName, @TRightDesc, @TRightShortName, @TRightId;
		WHILE @@FETCH_STATUS = 0
		BEGIN
			--Print '--Role: ' + @TRoleName + ' RightId: ' + convert(varchar, @TRightName)
			EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @RightId = @RightId OUTPUT;
			FETCH RightsL INTO @TRightName, @TRightDesc, @TRightShortName, @TRightId;
		END;
		CLOSE RightsL;
		DEALLOCATE RightsL;
		FETCH Roles INTO @TRoleId, @TRoleName, @TRoleShortName, @TRoleDesc;
	END;
	CLOSE Roles;
	DEALLOCATE Roles;
	UPDATE #HoldPartyId
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

EXEC sys.sp_updateextendedproperty @name = N'Build', @value = '57';