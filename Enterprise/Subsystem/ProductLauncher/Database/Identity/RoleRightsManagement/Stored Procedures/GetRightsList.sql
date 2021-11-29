CREATE PROCEDURE [Security].[GetRightsList] (
	@RightId INT = 0,
	@FilterByName VARCHAR(MAX) = NULL,  
	@FilterByVisibility INT = 0,
	@FilterByStatusType INT = 0,
	@FilterByProduct VARCHAR(MAX) = NULL,
	@FilterByTargetProduct VARCHAR(MAX) = NULL,
	@SortColumn    VARCHAR(256) = 'RightName',  
	@SortDirection   VARCHAR(4) = 'Asc',  
	@RowsPerPage   INT     = 0,  
	@PageNumber    INT     = 1 
)   
AS  
BEGIN

DECLARE @sortValue INT  
 SELECT @RowsPerPage = CASE WHEN @RowsPerPage <= 0 THEN 2147483647 ELSE @RowsPerPage END  
  
--Product Filter  
 DECLARE @ProductFilter TABLE (  
  ProductId int PRIMARY KEY  
 )  
 INSERT INTO @ProductFilter (  
  ProductId  
 )  
 SELECT CONVERT(int, value)  
 FROM  STRING_SPLIT(@FilterByProduct, ',');

 --Target Product Filter  
 DECLARE @TargetProductFilter TABLE (  
  ProductId int PRIMARY KEY  
 )  
 INSERT INTO @TargetProductFilter (  
  ProductId  
 )  
 SELECT CONVERT(int, value)  
 FROM  STRING_SPLIT(@FilterByTargetProduct, ',');
 
 SELECT @sortValue =  
  CASE @SortColumn  
   WHEN N'RightName' THEN 100  
   WHEN N'Description' THEN 101  
   WHEN N'Product' THEN 102  
   WHEN N'Roles' THEN 103
   WHEN N'Excluded' THEN 104 
   WHEN N'Routes' THEN 105
   WHEN N'TargetProduct' THEN 106
   WHEN N'Visibility' THEN 107
   ELSE 107
  END * CASE UPPER(@SortDirection) WHEN N'ASC' THEN 1 WHEN N'DESC' THEN -1 END;
    
WITH RolesForRight 
AS
(
	 SELECT RR.RightId 
			,COUNT(RR.ROLEID) AS RoleCount
	 FROM Security.RoleRight RR
		INNER JOIN Security.Role R ON R.RoleId = RR.RoleId
		WHERE R.RoleTypeID NOT IN ( 2 )
	 GROUP BY RR.RightId
),
RoutesForRight
AS
(
	SELECT	RightId
			,COUNT(ROUTEID) AS RouteCount
	FROM Security.RightRoute
	GROUP BY RightId
),
OverrideRoute
AS
(
	SELECT	RightId
			,COUNT(OrgPartyId) AS Excluded
	FROM Security.OrganizationOverRideRight
	GROUP BY RightId
),
CTERightsWithFilter
AS
(
	SELECT 
		R.RightId AS RightId
		,R.RightName AS  RightName
		,ISNULL(R.Description,'') AS Description
		,ISNULL(R.Value,R.RightName) As Value
		,ISNULL(r.VisibilityStatusId,'0') AS VisibilityStatusId
		,ISNULL(ST.Name,'') AS Visibility		
		,R.ProductId as ProductId
		,P.Name  AS Product	
		,ISNULL(RFR.RoleCount,0) AS Roles
		,ISNULL(ORR.Excluded,0) AS Excluded
		,ISNULL(RR.RouteCount,0) AS [Routes]
		,R.TargetProductId AS TargetProductId
		,P1.Name AS TargetProduct	
	FROM Security.[Right] R 
		LEFT OUTER JOIN Enterprise.StatusType ST ON ST.StatusTypeId = R.VisibilityStatusId
		LEFT OUTER JOIN Enterprise.Product P ON P.ProductId = R.ProductId 
		LEFT OUTER JOIN RolesForRight RFR on RFR.RightId = R.RightId
		LEFT OUTER JOIN RoutesForRight RR ON RR.RightId = R.RightId
		LEFT OUTER JOIN OverrideRoute ORR ON ORR.RightId = R.RightId
		LEFT OUTER JOIN Enterprise.Product p1 on p1.ProductId = r.TargetProductId
	WHERE
		R.ProductId IS NOT NULL
		AND r.TargetProductId IS NOT NULL
		AND
		((@RightId = 0) OR (R.RightId = @RightId))
		AND	(((@FilterByName IS NULL) OR (R.RightName LIKE '%' + @FilterByName + '%'))
				OR ((@FilterByName IS NULL) OR (R.Description LIKE '%' + @FilterByName + '%'))
				OR ((@FilterByName IS NULL) OR (P.[Name] LIKE '%' + @FilterByName + '%'))				
			)
		AND ((@FilterByVisibility = 0) OR (R.VisibilityStatusId = @FilterByVisibility))
		AND ((@FilterByStatusType = 0) OR (R.StatusTypeId = @FilterByStatusType))
		AND ((@FilterByProduct IS NULL) OR (ISNULL(R.ProductId,0) IN (SELECT ProductId FROM @ProductFilter)))
		AND ((@FilterByTargetProduct IS NULL) OR (ISNULL(R.TargetProductId,0) IN (SELECT ProductId FROM @TargetProductFilter)))
			
),

CTERightsWithSortAndFilter
AS
(
	SELECT
		RightId
		, RightName
		, Description
		, Value
		, VisibilityStatusId
		, Visibility
		, Product
		, ProductId
		, Roles
		, Excluded
		, [Routes]
		, TargetProductId
		, [TargetProduct]
		, COUNT(1) OVER () AS [TotalRecords]
		, CASE @sortValue  
			WHEN 100 THEN ROW_NUMBER() OVER (ORDER BY RightName ASC)  
			WHEN 101 THEN ROW_NUMBER() OVER (ORDER BY Description ASC)  
			WHEN 102 THEN ROW_NUMBER() OVER (ORDER BY Product ASC)  
			WHEN 103 THEN ROW_NUMBER() OVER (ORDER BY Roles ASC)
			WHEN 104 THEN ROW_NUMBER() OVER (ORDER BY Excluded ASC)  
			WHEN 105 THEN ROW_NUMBER() OVER (ORDER BY [Routes] ASC)
			WHEN 106 THEN ROW_NUMBER() OVER (ORDER BY [TargetProduct] ASC) 
			WHEN 107 THEN ROW_NUMBER() OVER (ORDER BY [VisibilityStatusId] ASC) 
			WHEN -100 THEN ROW_NUMBER() OVER (ORDER BY RightName DESC)  
			WHEN -101 THEN ROW_NUMBER() OVER (ORDER BY Description DESC)  
			WHEN -102 THEN ROW_NUMBER() OVER (ORDER BY Product DESC)  
			WHEN -103 THEN ROW_NUMBER() OVER (ORDER BY Roles DESC)
			WHEN -104 THEN ROW_NUMBER() OVER (ORDER BY Excluded DESC)  
			WHEN -105 THEN ROW_NUMBER() OVER (ORDER BY [Routes] DESC) 
			WHEN -106 THEN ROW_NUMBER() OVER (ORDER BY [TargetProduct] DESC)
			WHEN -107 THEN ROW_NUMBER() OVER (ORDER BY [VisibilityStatusId] DESC) 
	   END AS [RowNumber]
	FROM CTERightsWithFilter
)

--Resultset-1: Right list 
SELECT
		RightId
		, RightName
		, Description
		, Value
		, VisibilityStatusId
		, Visibility
		, Product
		, ProductId
		, Roles
		, Excluded
		, [Routes]
		, TargetProductId
		, [TargetProduct]
		, [TotalRecords]
		, [RowNumber]
		from CTERightsWithSortAndFilter
ORDER BY RowNumber  
 OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS  
    FETCH NEXT @RowsPerPage ROWS ONLY 

--Resultset- 2 : Distinct Products

SELECT distinct
	p.ProductId
	, p.Name as ProductName
	FROM 
	SECURITY.[Right] R INNER JOIN Enterprise.Product p on p.ProductId = r.ProductId	

--Resultset- 3 : Distinct Visibility Types

SELECT distinct
	st.StatusTypeId as Id
	, st.Name
FROM SECURITY.[Right] r INNER JOIN Enterprise.StatusType st on st.StatusTypeId = r.VisibilityStatusId

--Resultset- 4 : Disctinct TragetProducts

SELECT distinct
	p.ProductId
	, p.Name as ProductName
	FROM 
	SECURITY.[Right] R INNER JOIN Enterprise.Product p on p.ProductId = r.TargetProductId	

--Resultset- 5: Distinct StatusTypes

SELECT distinct
	st.StatusTypeId as Id
	, st.Name
FROM SECURITY.[Right] r INNER JOIN Enterprise.StatusType st on st.StatusTypeId = r.StatusTypeId
END	