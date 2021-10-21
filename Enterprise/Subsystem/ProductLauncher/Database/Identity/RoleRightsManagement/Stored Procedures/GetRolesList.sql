CREATE PROCEDURE [Security].[GetRolesList] (  
 @FilterBy nvarchar(max),  
 @SortBy nvarchar(max),  
 @RowsPerPage int = 0,  
 @PageNumber int = 1  
) 
AS
BEGIN
DECLARE @PartyId bigint,   
  @filterName nvarchar(512),  
  @filterRoleTypeId int = NULL,
  @filterOrgPartyIdForVisibility int = NULL,
  @csvProduct varchar(max),
  @sortValue int = 100,
  @productFilterCount int = 0

--GET FILTER PROPERTIES
IF (ISJSON(@FilterBy) = 0)  
 BEGIN  
  SET @FilterBy = NULL  
 END 
 
 DECLARE @tblFilterBy TABLE (  
  ColumnName varchar(128),  
  SearchValue varchar(max)  
 )  

 INSERT INTO @tblFilterBy (  
  ColumnName,  
  SearchValue  
 )  
 SELECT ColumnName,  SearchValue      
 FROM OPENJSON (JSON_QUERY(@FilterBy, '$.filterBy'))  
 WITH (  
     ColumnName nvarchar(max) '$.ColumnName',  
     SearchValue nvarchar(max) '$.SearchValue'  
    )  
 WHERE ISJSON(@FilterBy) > 0 
 
 SELECT @filterName = SearchValue  
 FROM  @tblFilterBy  
 WHERE ColumnName = 'Name'  
 AND   SearchValue NOT IN ( '%', '')  

 SELECT @filterRoleTypeId = CONVERT(int, SearchValue)  
 FROM  @tblFilterBy  
 WHERE ColumnName = 'RoleType'  
 AND   ISNUMERIC(SearchValue) = 1  

 SELECT @filterOrgPartyIdForVisibility = CONVERT(int, SearchValue)  
 FROM  @tblFilterBy  
 WHERE ColumnName = 'OrgPartyIdForVisibility'  
 AND   ISNUMERIC(SearchValue) = 1  

 DECLARE @filterProduct TABLE (  
  ProductId int PRIMARY KEY  
 )  

 SELECT @csvProduct = SearchValue  
 FROM  @tblFilterBy  
 WHERE ColumnName = 'Product'  
 AND   SearchValue NOT IN ( '%', '')  

 IF (LEN(@csvProduct) > 0)  
 BEGIN  
  INSERT INTO @filterProduct (  
   ProductId  
  )  
  SELECT CONVERT(int, value)  
  FROM STRING_SPLIT(@csvProduct, ',');  

  SELECT @productFilterCount = COUNT(*)
  FROM @filterProduct
 END  
--GET SORT PROPERTIES
IF (ISJSON(@SortBy) = 0)  
 BEGIN  
  SET @SortBy = NULL  
 END  

 SELECT @SortValue =  
   CASE ColumnName  
    WHEN N'InitialSort' THEN 100  
    WHEN N'Name' THEN 100  
    WHEN N'Description' THEN 101  
    WHEN N'RoleType' THEN 102  
    WHEN N'VisibilityType' THEN 103  
    WHEN N'Product' THEN 104  
    WHEN N'Rights' THEN 105  
    WHEN N'Excluded' THEN 106  
    ELSE 100  
   END * CASE SortDirection WHEN N'ASC' THEN 1 ELSE -1 END   
 FROM OPENJSON (JSON_QUERY(@SortBy, '$.sortBy'))  
 WITH (  
     ColumnName nvarchar(max) '$.ColumnName',  
     SortDirection nvarchar(max) '$.SortDirection'  
    )  
 WHERE ISJSON(@SortBy) > 0;  
 --END SORT PROPERTIES
END

DROP TABLE IF EXISTS #RoleDetails  

 CREATE TABLE #RoleDetails 
 (  
  RoleId bigint,  
  RoleName varchar(max),  
  ShortName varchar(max),  
  [Description] varchar(max),  
  RoleTypeId int,  
  RoleType varchar(max),  
  ProductId int,  
  ProductName varchar(max),
  OrgPartyID bigint,
  Visibility varchar(max),
  Rights varchar(max),
  Excluded int,
  TotalRecords int
 ) 

INSERT INTO #RoleDetails(
  RoleId,  
  RoleName,  
  ShortName,  
  [Description],  
  RoleTypeId,  
  RoleType,  
  ProductId,  
  ProductName,
  OrgPartyID,
  Visibility,
  Rights,
  Excluded
)
SELECT R.RoleId, 
	   R.RoleName, 
	   R.ShortName,
	   R.[Description],
	   RT.RoleTypeId,
	   RT.[Value] [RoleType],
	   P.ProductId,
	   P.[Name] [ProductName],
	   R.OrgPartyID,
	   CASE WHEN R.OrgPartyID IS NULL THEN 'All'
	   ELSE (SELECT [Name] FROM Enterprise.Organization WHERE PartyId = R.OrgPartyID)
	   END [Visibility],
	   (SELECT COUNT(1) FROM [Security].RoleRight RR WHERE R.RoleId = RR.RoleId) [Rights],
	   (SELECT COUNT(1) FROM [Security].OrganizationOverrideRole OOR WHERE R.RoleId = OOR.RoleId) [Excluded]
FROM [Security].[Role] R
INNER JOIN [Security].RoleType RT ON R.RoleTypeID = RT.RoleTypeId
INNER JOIN Enterprise.Product P ON R.ProductId = P.ProductId
WHERE RT.RoleTypeId != 2

SELECT DISTINCT ProductId, ProductName
FROM #RoleDetails
ORDER BY ProductName

SELECT DISTINCT RoleTypeId, RoleType
FROM #RoleDetails
ORDER BY RoleType

SELECT DISTINCT OrgPartyID, Visibility
FROM #RoleDetails
ORDER BY Visibility

;WITH cteRolesFinal  
 (  
  RoleId,  
  RoleName,  
  ShortName,  
  [Description],  
  RoleTypeId,  
  RoleType,  
  ProductId,  
  ProductName,
  OrgPartyID,
  Visibility,
  Rights,
  Excluded,
  TotalRecords
 ) 
 AS
(
SELECT R.RoleId, 
	   R.RoleName, 
	   R.ShortName,
	   R.[Description],
	   RT.RoleTypeId,
	   RT.[Value] [RoleType],
	   P.ProductId,
	   P.[Name] [ProductName],
	   R.OrgPartyID,
	   CASE WHEN R.OrgPartyID IS NULL THEN 'All'
	   ELSE (SELECT [Name] FROM Enterprise.Organization WHERE PartyId = R.OrgPartyID)
	   END [Visibility],
	   (SELECT COUNT(1) FROM [Security].RoleRight RR WHERE R.RoleId = RR.RoleId) [Rights],
	   (SELECT COUNT(1) FROM [Security].OrganizationOverrideRole OOR WHERE R.RoleId = OOR.RoleId) [Excluded],
	   COUNT(1) OVER () AS TotalRecords
FROM [Security].[Role] R
INNER JOIN [Security].RoleType RT ON R.RoleTypeID = RT.RoleTypeId
INNER JOIN Enterprise.Product P ON R.ProductId = P.ProductId
WHERE RT.RoleTypeId != 2
AND  (    
    (@filterName IS NULL)    
    OR (CHARINDEX(@filterName, R.RoleName, 1) > 0)    
    OR (CHARINDEX(@filterName, R.[Description], 1) > 0)    
    OR (CHARINDEX(@filterName, RT.[Value], 1) > 0)    
    OR (CHARINDEX(@filterName, (SELECT [Name] FROM Enterprise.Organization WHERE PartyId = R.OrgPartyID), 1) > 0)  
	OR (CHARINDEX(@filterName, P.[Name], 1) > 0)
      )    
    AND  ((@filterRoleTypeId IS NULL) OR (RT.RoleTypeId = @filterRoleTypeId)) 
	AND  ((@filterOrgPartyIdForVisibility IS NULL) OR (@filterOrgPartyIdForVisibility != 0 AND R.OrgPartyID = @filterOrgPartyIdForVisibility) OR (@filterOrgPartyIdForVisibility = 0 AND R.OrgPartyID IS NULL))
	AND  ((@productFilterCount = 0) OR P.ProductId IN (SELECT ProductId FROM @filterProduct))
)

SELECT RoleId,  
	   RoleName,  
	   ShortName,  
	   [Description],  
	   RoleTypeId,  
	   RoleType,  
	   ProductId,  
	   ProductName,
	   OrgPartyID,
	   Visibility,
	   Rights,
	   Excluded,
	   CASE @sortValue    
		WHEN 100 THEN ROW_NUMBER() OVER (ORDER BY RoleName ASC)    
		WHEN 101 THEN ROW_NUMBER() OVER (ORDER BY ISNULL([Description], '') ASC, RoleName ASC)    
		WHEN 102 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(RoleType, '') ASC, RoleName ASC)    
		WHEN 103 THEN ROW_NUMBER() OVER (ORDER BY Visibility ASC, RoleName ASC)    
		WHEN 104 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(ProductName, '') ASC, RoleName ASC)    
		WHEN 105 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(Rights, 0) ASC, RoleName ASC)  
		WHEN 106 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(Excluded, 0) ASC, RoleName)   
		WHEN -100 THEN ROW_NUMBER() OVER (ORDER BY RoleName DESC)    
		WHEN -101 THEN ROW_NUMBER() OVER (ORDER BY ISNULL([Description], '') DESC, RoleName ASC)    
		WHEN -102 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(RoleType, '') DESC, RoleName ASC)    
		WHEN -103 THEN ROW_NUMBER() OVER (ORDER BY Visibility DESC, RoleName ASC)    
		WHEN -104 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(ProductName, '') DESC, RoleName ASC)    
		WHEN -105 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(Rights, 0) DESC, RoleName ASC)    
		WHEN -106 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(Excluded, 0) DESC, RoleName ASC)   
     END AS RowNumber,
	 TotalRecords
FROM cteRolesFinal
 ORDER BY RowNumber  
 OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS  
 FETCH NEXT(@RowsPerPage) ROWS ONLY  
 OPTION (RECOMPILE) 

 DROP TABLE IF EXISTS #RoleDetails