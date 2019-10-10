

SELECT @ServerName=@@ServerName;

SET @DBName='Identity';

IF(@ServerName='RCDUSODBSQL001'
   OR @ServerName='RCTUSODBSQL001')
  AND @DBName='Identity'
BEGIN
DECLARE
  @TRoleId INT;
DECLARE
  @TRoleName NVARCHAR(200);
DECLARE
  @TRoleDesc NVARCHAR(200);
DECLARE
  @TRightId INT;
DECLARE
  @TRightName NVARCHAR(200);
DECLARE
  @TRightDesc NVARCHAR(200);
--DECLARE @RoleId INT;
--DECLARE @RightId INT;
DECLARE
  @RightCategory INT;
DECLARE
  @RoleCategory INT;
--DECLARE @PartyId INT;
--DECLARE @ProductId INT;
--DECLARE @RoleName NVARCHAR(200);
DECLARE
  @RightName NVARCHAR(200);
--DECLARE @RoleTypeID INT;
DECLARE
  @PerosonaP INT;
IF OBJECT_ID( 'tempdb..#Role' ) IS NOT NULL
    DROP TABLE #Role;
IF OBJECT_ID( 'tempdb..#Right' ) IS NOT NULL
    DROP TABLE #Right;
IF OBJECT_ID( 'tempdb..#Mapping' ) IS NOT NULL
    DROP TABLE #Mapping;
SELECT @RoleTypeId=PartyROleTypeId
FROM enterprise.roletype
WHERE Name='Product Role';
SELECT @PartyId=PartyId
FROM enterprise.Organization
WHERE Name='RealPage Employee';
SELECT @ProductId=ProductId
FROM Enterprise.Product
WHERE name='Research Application';
SELECT @RoleCategory=ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType
AS STCT
JOIN
Enterprise.StatusTypeCategory
AS STC
ON STCT.StatusTypeCategoryTypeId=STC.StatusTypeCategoryTypeId
    JOIN
    Enterprise.StatusTypeCategoryClassification
AS STCC
    ON STCC.StatusTypeCategoryId=STC.StatusTypeCategoryId
        JOIN
        Enterprise.StatusType
AS ST
        ON ST.StatusTypeId=STCC.StatusTypeId
WHERE STC.Name='Role Type'
      AND ST.Name='Default';
SELECT @RightCategory=ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType
AS STCT
JOIN
Enterprise.StatusTypeCategory
AS STC
ON STCT.StatusTypeCategoryTypeId=STC.StatusTypeCategoryTypeId
    JOIN
    Enterprise.StatusTypeCategoryClassification
AS STCC
    ON STCC.StatusTypeCategoryId=STC.StatusTypeCategoryId
        JOIN
        Enterprise.StatusType
AS ST
        ON ST.StatusTypeId=STCC.StatusTypeId
WHERE STC.Name='Right Type'
      AND ST.Name='Default';
CREATE TABLE #Role
(
  RoleID      INT,
  Name        NVARCHAR(100),
  Description NVARCHAR(100));
INSERT INTO #Role
(RoleId,
 Name,
 Description)
VALUES
(
  1, 'Research Analyst', 'analyst'),
(
  2, 'Research QA', 'QA'),
(
  3, 'Research Manager', 'manager'),
(
  4, 'Black-Book Director', 'director'),
(
  5, 'Executive', 'executive'),
(
  6, 'External Analyst', 'external');
CREATE TABLE #Right
(
  RightId     INT,
  Name        NVARCHAR(100),
  description NVARCHAR(100));
INSERT INTO #Right
(rightid,
 name,
 description)
VALUES
(
  1, 'Work on the different queues', 'queue.work'),
(
  2, 'Edit the master properties', 'property.edit'),
(
  3, 'Create master properties', 'property.create'),
(
  4, 'Manage FAQ', 'FAQ.edit'),
(
  5, 'Manage Users', 'users.edit'),
(
  6, 'View User Management', 'users.view'),
(
  7, 'View Dashboard', 'dashboard.view'),
(
  8, 'Access Reports', 'reports.view'),
(
  9, 'Modify without notes', 'property.withoutNotes'),
(
  10, 'Can Escalate work to next level', 'queue.escalate'),
(
  11, 'Edit master companies', 'company.edit'),
(
  12, 'Create new master companies', 'company.create'),
(
  13, 'Upload CSV files', 'csv.upload'),
(
  14, 'View Products', 'products.view'),
(
  15, 'View Markets', 'markets.view'),
(
  16, 'View FAQ', 'FAQ.view');
CREATE TABLE #Mapping
(
  RoleId  INT,
  RightId INT);
INSERT INTO #Mapping
(RoleId,
 RightId)
VALUES
(
  1, 1),
(
  1, 2),
(
  1, 3),
(
  1, 10),
(
  1, 11),
(
  1, 12),
(
  1, 14),
(
  1, 15),
(
  1, 16),
(
  2, 1),
(
  2, 2),
(
  2, 3),
(
  2, 10),
(
  2, 11),
(
  2, 12),
(
  2, 14),
(
  2, 15),
(
  2, 16),
(
  3, 1),
(
  3, 2),
(
  3, 3),
(
  3, 7),
(
  3, 11),
(
  3, 12),
(
  3, 14),
(
  3, 15),
(
  3, 16),
(
  4, 2),
(
  4, 3),
(
  4, 4),
(
  4, 5),
(
  4, 6),
(
  4, 7),
(
  4, 8),
(
  4, 11),
(
  4, 12),
(
  4, 13),
(
  4, 14),
(
  4, 15),
(
  4, 16),
(
  5, 6),
(
  5, 7),
(
  5, 8),
(
  5, 14),
(
  5, 15),
(
  5, 16);
SELECT *
FROM #right;
DECLARE Roles CURSOR
FOR SELECT RoleId,
           Name,
           Description
    FROM #Role;
OPEN Roles;
FETCH Roles INTO
  @TRoleId,
  @TRoleName,
  @TRoleDesc;
WHILE @@FETCH_STATUS=0
BEGIN
EXECUTE Enterprise.CreateRole
        @RoleName=@TRoleName,
        @Shortname=@TRoleDesc,
        @Description='',
        @RoleTypeId=@RoleTypeId,
        @RoleCategoryId=@RoleCategory,
        @PartyId=@PartyId,
        @RoleId=@RoleId OUTPUT;
DECLARE RightsL CURSOR
FOR SELECT Name,
           Description,
           b.RightId
    FROM #Right
    AS a
    INNER JOIN
    #Mapping
    AS b
    ON a.RightId=b.RightId
       AND b.RoleID=@TRoleId;
OPEN RightsL;
FETCH RightsL INTO
  @TRightName,
  @TRightDesc,
  @TRightId;
WHILE @@FETCH_STATUS=0
BEGIN
PRINT @TRightName;
EXECUTE Enterprise.CreateRight
        @RoleId=@RoleId,
        @RightName=@TRightName,
        @Description='',
        @RightCategoryId=@RightCategory,
        @PartyId=@PartyId,
        @ProductId=@ProductId,
        @Shortname=@TRightDesc,
        @RightId=@RightId OUTPUT;
FETCH RightsL INTO
  @TRightName,
  @TRightDesc,
  @TRightId;
END;
CLOSE RightsL;
DEALLOCATE RightsL;
EXECUTE Enterprise.LinkPersonaToRole
        @PersonaId=33,
        @RoleId=@RoleId,
        @PersonaPrivilgeID=@PerosonaP OUTPUT;
FETCH Roles INTO
  @TRoleId,
  @TRoleName,
  @TRoleDesc;
END;
CLOSE Roles;
DEALLOCATE Roles;
END;

EXEC sys.sp_updateextendedproperty
     @name=N'Build',
     @value='47';