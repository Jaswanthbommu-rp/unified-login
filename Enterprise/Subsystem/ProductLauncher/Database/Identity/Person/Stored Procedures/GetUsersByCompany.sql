CREATE PROCEDURE Person.GetUsersByCompany
	@FilterBy NVARCHAR(MAX),
	@OrgPartyId BIGINT,
	@SortBy NVARCHAR(MAX),
	@RowsPerPage INT = 10,
	@PageNumber INT = 1
AS
BEGIN
/*
EXEC Person.GetUsersByCompany @FilterBy='{"filterBy":[{"ColumnName":"status","SearchValue":"1,23,3,2,12"},{"ColumnName":"name","SearchValue":"a"}]}'
,@OrgPartyId=350
,@SortBy='{"sortBy":[{"ColumnName":"lastName","SortDirection":"ASC"}]}'
,@RowsPerPage = 100
,@PageNumber=1

EXEC Person.GetUsersByCompany @FilterBy=''
,@OrgPartyId=0
,@SortBy=''
,@RowsPerPage = 100
,@PageNumber=1
*/
	DECLARE @tblFilterBy TABLE (ColumnName varchar(128), SearchValue varchar(max))
	DECLARE @filterStatus TABLE (StatusTypeId int PRIMARY KEY)
	DECLARE @filterName NVARCHAR(512)
			,@csvStatus varchar(512)
			,@filterStatusTypeId INT
			,@sortValue INT = 100

	IF (ISJSON(@FilterBy) = 0)          
	BEGIN          
	 SET @FilterBy = NULL          
	END

	IF (ISJSON(@SortBy) = 0)          
	BEGIN          
	 SET @SortBy = NULL          
	END

	INSERT INTO @tblFilterBy (ColumnName, SearchValue)          
	SELECT ColumnName, SearchValue          
	FROM OPENJSON (JSON_QUERY(@FilterBy, '$.filterBy'))          
	WITH (ColumnName nvarchar(max) '$.ColumnName', SearchValue nvarchar(max) '$.SearchValue')          
	WHERE ISJSON(@FilterBy) > 0

	SELECT @filterName = SearchValue          
	FROM @tblFilterBy          
	WHERE ColumnName = 'Name'          
	AND SearchValue NOT IN ( '%', '')

	SELECT @csvStatus = SearchValue
	FROM  @tblFilterBy          
	WHERE ColumnName = 'Status'          
	AND   SearchValue NOT IN ( '%', '')

	IF (LEN(@csvStatus) > 0)          
	BEGIN          
		INSERT INTO @filterStatus (StatusTypeId)          
		SELECT CONVERT(int, value) FROM STRING_SPLIT(@csvStatus, ',');          
	END

	SELECT @filterStatusTypeId = COUNT(StatusTypeId)          
	FROM  @filterStatus          
	WHERE StatusTypeId > 0

	SELECT @SortValue =          
						CASE ColumnName WHEN N'firstName' THEN 100
										WHEN N'lastName' THEN 101
										WHEN N'loginName' THEN 102
										ELSE 100 END 
						* CASE SortDirection WHEN N'ASC' THEN 1 ELSE -1 END           
	FROM OPENJSON (JSON_QUERY(@SortBy, '$.sortBy'))          
	WITH (ColumnName nvarchar(max) '$.ColumnName', SortDirection nvarchar(max) '$.SortDirection')          
	WHERE ISJSON(@SortBy) > 0;
	;WITH CTE AS(
	SELECT p.FirstName
	,p.LastName
	,ul.LoginName
	,ul.UserId
	,COUNT(1) OVER () AS TotalRecords
	,CASE @sortValue 
		WHEN 100 THEN ROW_NUMBER() OVER (ORDER BY p.FirstName + ' ' + p.LastName ASC) 
		WHEN -100 THEN ROW_NUMBER() OVER (ORDER BY p.FirstName + ' ' + p.LastName DESC)

		WHEN 101 THEN ROW_NUMBER() OVER (ORDER BY p.LastName + ' ' + p.FirstName ASC) 
		WHEN -101 THEN ROW_NUMBER() OVER (ORDER BY p.LastName + ' ' + p.FirstName DESC)

		WHEN 102 THEN ROW_NUMBER() OVER (ORDER BY ul.LoginName ASC) 
		WHEN -102 THEN ROW_NUMBER() OVER (ORDER BY ul.LoginName DESC)

	END AS RowNumber
	FROM ident.userlogin ul
	INNER JOIN ident.UserLoginPersona ulp ON ulp.UserLoginId = ul.UserId
	INNER JOIN person.person p ON p.PartyId = ul.PersonPartyId
	LEFT OUTER JOIN @filterStatus fs ON (ulp.StatusTypeId = fs.StatusTypeId)
	WHERE ulp.OrganizationPartyId = @OrgPartyId
	AND ((@filterName IS NULL)
		OR ((CHARINDEX(@filterName, p.FirstName + ' ' + p.LastName, 1) > 0)            
		OR (CHARINDEX(@filterName, ul.LoginName, 1) > 0)))
	AND  ((@filterStatusTypeId = 0) OR (NOT fs.StatusTypeId IS NULL))    
	AND ulp.IsRPEmployee = 0
	AND 1 = (CASE WHEN ((SELECT count(1) FROM @filterStatus WHERE StatusTypeId = 2) = 0) 
			THEN (CASE WHEN ((ulp.StatusTypeId = 12) AND (ul.LastLoginDate IS NULL)) 
			THEN 0 ELSE 1 END) ELSE 1 END))
	SELECT CTE.FirstName,CTE.LastName,CTE.LoginName,CTE.UserId, CTE.TotalRecords
	FROM CTE
	ORDER BY RowNumber
	OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS          
	FETCH NEXT(@RowsPerPage) ROWS ONLY  
END
GO