CREATE PROCEDURE [Security].[GetAdGroupDetails] (      
 @FilterBy nvarchar(max),      
 @SortBy nvarchar(max),      
 @RowsPerPage int = 0,      
 @PageNumber int = 1      
)     
AS    
BEGIN    
DECLARE @filterName nvarchar(512), @filterByStatus bit,       
  @sortValue int = 100      
      
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
     
 select @filterByStatus = SearchValue        
 FROM  @tblFilterBy        
 WHERE ColumnName = 'Status'        
 AND SearchValue IN (0, 1)      
     
--GET SORT PROPERTIES      
IF (ISJSON(@SortBy) = 0)        
 BEGIN        
  SET @SortBy = NULL        
 END        
      
 SELECT @SortValue =        
   CASE ColumnName        
    WHEN N'InitialSort' THEN 100        
 WHEN N'DisplayName' THEN 100       
    WHEN N'Id' THEN 101        
    WHEN N'ProductCount' THEN 102      
    WHEN N'RightCount' THEN 103      
	WHEN N'OrgTypeCount' THEN 104   
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
      
DROP TABLE IF EXISTS #ADGroupDetails        
      
 CREATE TABLE #ADGroupDetails       
 (        
  ADGroupId INT,        
  DisplayName varchar(max),        
  ActiveDirectoryId uniqueidentifier,         
  ProductCount int,        
  RightCount int,
  OrgTypeCount INT,
  Status bit,    
  TotalRecords int      
 )       
      
INSERT INTO #ADGroupDetails(      
  ADGroupId,        
  DisplayName,        
  ActiveDirectoryId,      
  ProductCount,      
  RightCount,
  OrgTypeCount,
  Status    
)      
SELECT ADG.ADGroupId,       
    ADG.DisplayName,       
    ADG.ActiveDirectoryId,      
    (SELECT COUNT(1) FROM [Security].ADGroupProduct ADP WHERE ADG.ADGroupId = ADP.ADGroupId) [ProductCount],      
    (SELECT COUNT(1) FROM [Security].ADGroupRight ADR WHERE ADG.ADGroupId = ADR.ADGroupId) [RightCount],    
	(SELECT COUNT(1) FROM [Security].ADGroupOrganizationType AOT WHERE ADG.ADGroupId = AOT.ADGroupId) [OrgTypeCount], 
 IsActive as [Status]    
FROM [Security].ADGroup ADG      
WHERE ADG.IsActive = 1      
      
;WITH cteADGroupFinal        
 (        
  ADGroupId,        
  DisplayName,        
  ActiveDirectoryId,      
  ProductCount,      
  RightCount,
  OrgTypeCount,
  Status,    
  TotalRecords      
 )       
 AS      
(      
SELECT ADG.ADGroupId,       
    ADG.DisplayName,       
    ADG.ActiveDirectoryId,      
    (SELECT COUNT(1) FROM [Security].ADGroupProduct ADP WHERE ADG.ADGroupId = ADP.ADGroupId) [ProductCount],      
    (SELECT COUNT(1) FROM [Security].ADGroupRight ADR WHERE ADG.ADGroupId = ADR.ADGroupId) [RightCount],     
	(SELECT COUNT(1) FROM [Security].ADGroupOrganizationType AOT WHERE ADG.ADGroupId = AOT.ADGroupId) [OrgTypeCount],
 ADG.IsActive as [Status],    
    COUNT(1) OVER () AS TotalRecords      
FROM [Security].ADGroup ADG      
WHERE  (          
 --ADG.IsActive = 1 AND     
    ((@filterByStatus is null) OR (ADG.IsActive = @filterByStatus))    and    
    ((@filterName IS NULL)          
    OR (CHARINDEX(@filterName, ADG.DisplayName, 1) > 0))        
))      
      
SELECT  ADGroupId,        
  DisplayName,        
  ActiveDirectoryId,      
  ProductCount,      
  RightCount,
  OrgTypeCount,
  Status,    
  CASE @sortValue          
  WHEN 100 THEN ROW_NUMBER() OVER (ORDER BY DisplayName ASC)          
  WHEN 101 THEN ROW_NUMBER() OVER (ORDER BY ISNULL([ADGroupId], '') ASC, DisplayName ASC)          
  WHEN 102 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(ProductCount, '') ASC, DisplayName ASC)          
  WHEN 103 THEN ROW_NUMBER() OVER (ORDER BY RightCount ASC, DisplayName ASC)            
  WHEN 104 THEN ROW_NUMBER() OVER (ORDER BY OrgTypeCount ASC, DisplayName ASC)
  WHEN -100 THEN ROW_NUMBER() OVER (ORDER BY DisplayName DESC)          
  WHEN -101 THEN ROW_NUMBER() OVER (ORDER BY ISNULL([ADGroupId], '') DESC, DisplayName ASC)          
  WHEN -102 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(ProductCount, '') DESC, DisplayName ASC)          
  WHEN -103 THEN ROW_NUMBER() OVER (ORDER BY RightCount DESC, DisplayName ASC)          
  WHEN -104 THEN ROW_NUMBER() OVER (ORDER BY OrgTypeCount DESC, DisplayName ASC)  
     END AS RowNumber,      
  TotalRecords      
FROM cteADGroupFinal      
 ORDER BY RowNumber        
 OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS        
 FETCH NEXT(@RowsPerPage) ROWS ONLY        
 OPTION (RECOMPILE)       
      
 DROP TABLE IF EXISTS #ADGroupDetails