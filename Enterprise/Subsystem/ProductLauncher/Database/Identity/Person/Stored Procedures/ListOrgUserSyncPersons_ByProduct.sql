CREATE PROCEDURE [Person].[ListOrgUserSyncPersons_ByProduct]
(     
 @RealPageId uniqueidentifier ,      
 @ProductId int,      
 @FilterBy nvarchar(max),      
 @SortBy nvarchar(max),      
 @RowsPerPage int = 0,      
 @PageNumber int = 1      
) 
AS
BEGIN
	DECLARE @PartyId bigint,      
	  @NOW datetime= GETUTCDATE(),      
	  @sortOrder nvarchar(128),      
	  @sortDirection nvarchar(4),      
	  @sortValue int = 100,      
	  @filterName nvarchar(512),      
	  @filterProductId int = NULL,      
	  @filterStatusTypeId int = 0,
	  @filterPartyRoleTypeId int = NULL,
	  @minSequence smallint, 
	  @csvStatus varchar(max),
	  @ProductSettingTypeId int,      
	  @OffsetMinutes smallint,
	  @filterOperatorCount int = NULL,
	  @ProductSource varchar(10);
	        
	 DECLARE @filterStatus TABLE (      
	  StatusTypeId int PRIMARY KEY      
	 )      
         
       
	 DECLARE @ValidPersona TABLE (      
	  PersonaId bigint  ,
	  UserLoginPersonaId BIGINT
	 )      
      
 
	 DECLARE @tblFilterBy TABLE (      
	  ColumnName varchar(128),      
	  SearchValue varchar(max)      
	 )     
	 
	 SELECT @RowsPerPage = CASE      
	  WHEN @RowsPerPage <= 0 THEN 2147483647      
	  ELSE @RowsPerPage      
	 END;       
      
	 IF (ISJSON(@SortBy) = 0)      
	 BEGIN      
	  SET @SortBy = NULL      
	 END      
      
	 IF (ISJSON(@FilterBy) = 0)      
	 BEGIN      
	  SET @FilterBy = NULL      
	 END      
       
	 SELECT @ProductSettingTypeId = ProductSettingTypeId      
	 FROM  Enterprise.ProductSettingType      
	 WHERE Name = 'ProductStatus' 
      
	 SELECT @SortValue =      
	   CASE ColumnName      
		WHEN N'InitialSort' THEN 100      
		WHEN N'Name' THEN 100      
		WHEN N'LoginName' THEN 101      
		WHEN N'StatusName' THEN 102 
		WHEN N'LastRefreshed' THEN 103  
		WHEN N'LastSynced' THEN 104
		ELSE 100      
	   END * CASE SortDirection WHEN N'ASC' THEN 1 ELSE -1 END       
	 FROM OPENJSON (JSON_QUERY(@SortBy, '$.sortBy'))      
	 WITH (      
		 ColumnName nvarchar(max) '$.ColumnName',      
		 SortDirection nvarchar(max) '$.SortDirection'      
		)      
	 WHERE ISJSON(@SortBy) > 0;  

	 INSERT INTO @tblFilterBy (      
	  ColumnName,      
	  SearchValue      
	 )      
	 SELECT ColumnName,      
		 SearchValue      
	 FROM OPENJSON (JSON_QUERY(@FilterBy, '$.filterBy'))      
	 WITH (      
		 ColumnName nvarchar(max) '$.ColumnName',      
		 SearchValue nvarchar(max) '$.SearchValue'      
		)      
	 WHERE ISJSON(@FilterBy) > 0      
      
	 SELECT @OffsetMinutes = SearchValue      
	 FROM  @tblFilterBy      
	 WHERE ColumnName = 'OffsetMinutes'      
      
	 SET @OffsetMinutes = ISNULL(@OffsetMinutes, 0)      
      
	 SELECT @filterName = SearchValue      
	 FROM  @tblFilterBy      
	 WHERE ColumnName = 'Name'      
	 AND   SearchValue NOT IN ( '%', '')      
     
	 SELECT @filterPartyRoleTypeId = CONVERT(int, SearchValue)      
	 FROM  @tblFilterBy      
	 WHERE ColumnName = 'UserType'      
	 AND   ISNUMERIC(SearchValue) = 1      
      
	 SELECT @csvStatus = SearchValue      
	 FROM  @tblFilterBy      
	 WHERE ColumnName = 'Status'      
	 AND   SearchValue NOT IN ( '%', '')   
	 
	 IF (LEN(@csvStatus) > 0)      
	 BEGIN      
	  INSERT INTO @filterStatus (      
	   StatusTypeId      
	  )      
	  SELECT CONVERT(int, value)      
	  FROM STRING_SPLIT(@csvStatus, ',');      
	 END      
      
	 SELECT @ProductSource = BooksProductCode FROM Enterprise.Product Where ProductId = @ProductId

	 SELECT @filterStatusTypeId = StatusTypeId      
	 FROM  @filterStatus      
	 WHERE StatusTypeId > 0      
      
	 SELECT @PartyId = PartyId      
	 FROM  Enterprise.Party      
	 WHERE RealPageId = @RealPageId   
 
	 DROP TABLE IF EXISTS #UserSync  
	 CREATE TABLE #UserSync      
	 (      
		  PersonaId BIGINT ,        
		  UserLoginPersonaId BIGINT NULL,
		  UserId BIGINT NULL,
		  FirstName VARCHAR(100) ,      
		  MiddleName VARCHAR(50) NULL,      
		  LastName VARCHAR(100) ,
		  LoginName VARCHAR(255) NULL,
		  StatusId INT NULL,        
		  StatusName VARCHAR(50) NULL,
		  AssignedCount INT NULL,
		  MatchedCount INT NULL,
		  LastRefreshed DATETIME NULL,
		  LastSynced DATETIME NULL
	 )  
	 
	 IF (@filterPartyRoleTypeId IS NULL)
	 BEGIN
		  INSERT INTO @ValidPersona (      
		   PersonaId   ,UserLoginPersonaId   
		  )      
		  SELECT P.PersonaId  , ulp.UserLoginPersonaId  
		  FROM Person.Persona p      
			 INNER JOIN Ident.UserLoginPersona ulp ON p.UserLoginPersonaId = ulp.UserLoginPersonaId      
			 INNER JOIN Ident.UserLogin ul ON ulp.UserLoginId = ul.UserId      
			 INNER JOIN Enterprise.PartyRelationship pr ON pr.PartyIdFrom = ul.PersonPartyId and pr.PartyIdTo = ulp.OrganizationPartyId and pr.ThruDate is null      
			 INNER JOIN Enterprise.RoleType rt ON rt.PartyRoleTYpeId = pr.RoleTypeIdFrom  
			 INNER JOIN Enterprise.PersonaConfiguration pec ON p.PersonaId = pec.PersonaId  
			 INNER JOIN Enterprise.StatusType est ON ulp.StatusTypeId = est.StatusTypeId  And est.StatusTypeId NOT IN (12,23,24)
		  WHERE rt.PartyRoleTYpeId IN (401,404,405) --user,User (No Email),external    
		  AND  ulp.OrganizationPartyId = @PartyId 
		  AND pec.StatusTypeId = 8    
		  AND  pec.ProductId = @ProductId
		  AND  ((@NOW >= p.FromDate AND p.ThruDate IS NULL) OR (@NOW BETWEEN p.FromDate AND p.ThruDate))      
		  AND  ((@NOW >= pec.FromDate AND pec.ThruDate IS NULL) OR (@NOW BETWEEN pec.FromDate AND pec.ThruDate))    
	 END
	 ELSE
	 BEGIN
		  INSERT INTO @ValidPersona (      
		   PersonaId,UserLoginPersonaId      
		  )      
		  SELECT P.PersonaId  , ulp.UserLoginPersonaId    
		  FROM Person.Persona p      
			 INNER JOIN Ident.UserLoginPersona ulp ON p.UserLoginPersonaId = ulp.UserLoginPersonaId      
			 INNER JOIN Ident.UserLogin ul ON ulp.UserLoginId = ul.UserId      
			 INNER JOIN Enterprise.PartyRelationship pr ON pr.PartyIdFrom = ul.PersonPartyId and pr.PartyIdTo = ulp.OrganizationPartyId and pr.ThruDate is null      
			 INNER JOIN Enterprise.RoleType rt ON rt.PartyRoleTYpeId = pr.RoleTypeIdFrom
			 INNER JOIN Enterprise.PersonaConfiguration pec ON p.PersonaId = pec.PersonaId 
			 INNER JOIN Enterprise.StatusType est ON ulp.StatusTypeId = est.StatusTypeId  And est.StatusTypeId NOT IN (12,23,24)
		  WHERE rt.PartyRoleTYpeId = @filterPartyRoleTypeId   
		  AND  ulp.OrganizationPartyId = @PartyId
		  AND pec.StatusTypeId = 8    
		  AND  pec.ProductId = @ProductId
		  AND  ((@NOW >= p.FromDate AND p.ThruDate IS NULL) OR (@NOW BETWEEN p.FromDate AND p.ThruDate))      
		  AND  ((@NOW >= pec.FromDate AND pec.ThruDate IS NULL) OR (@NOW BETWEEN pec.FromDate AND pec.ThruDate))    
	 END;

	  WITH CTE_PersonaAssignedPrimaryProperties(PersonaId,AssignedCount) 
	   AS (
			Select pl.PersonaId, COUNT(PM.PropertyInstanceId)
			FROM	@ValidPersona pl
			INNER JOIN Enterprise.PropertyInstanceMapping PM ON
				PM.PersonaId = pl.PersonaId And PM.ProductId = @ProductId
			GROUP BY pl.PersonaId
		  ),
	   CTE_PersonaMatchedPrimaryProperties(PersonaId,MatchedCount) 
	   AS (
			Select pl.PersonaId, COUNT(PM.PropertyInstanceId)
			FROM	@ValidPersona pl
			INNER JOIN Enterprise.UserSyncProductPrimaryPropertiesStaging PM ON
				PM.PersonaId = pl.PersonaId And PM.ProductId = @ProductId
			GROUP BY pl.PersonaId
		  ),
	   CTE_UserSyncJobStatus(PersonaId,StatusId,LastRefreshDate )
	   AS (
			SELECT TOP 1 F.PersonaId,ISNULL(USJ.StatusTypeId ,1),USJ.CreatedDate
			FROM @ValidPersona F
			LEFT JOIN [Enterprise].[UserSyncJob_V2] USJ ON
				F.PersonaId = USJ.UserPersonaId AND USJ.UserSyncJobTypeId = 2
			LEFT JOIN Enterprise.UserSyncJobTask_V2 USJT ON
				USJ.UserSyncJobId = USJT.UserSyncJobId AND USJT.Source = @ProductSource
			ORDER BY USJ.UserSyncJobId desc
		  ),
	   CTE_UserPropertiesSynced(PersonaId,LastSyncDate )
	   AS (
			SELECT P.PersonaId,PMH.ProductPropertiesSyncDate
			FROM @ValidPersona P
			INNER JOIN Enterprise.PersonaProductPropertiesSyncHistory PMH ON
				PMH.PersonaId = P.PersonaId And PMH.ProductId = @ProductId
		  )

	  INSERT INTO #UserSync
	  Select  pe.PersonaId,       
			  iulp.UserLoginPersonaId,
			  ul.UserId, 
			  p.FirstName,
			  p.MiddleName,
			  p.LastName,
			  ul.LoginName ,
			  ISNULL(CUSJ.StatusId,1),
			  CASE        
				  WHEN (StatusId = 1) THEN 'New'        
				  WHEN (StatusId = 6) THEN 'In Progress'
				   WHEN (StatusId = 8) THEN 'Completed'
			  ELSE est.Name        
			  END AS 'StatusName'  ,
			  ISNULL(AssignedCount,0),
			  ISNULL(MatchedCount,0),		  
			  ISNULL(LastRefreshDate, Null)  AS 'LastRefreshDate',
			  ISNULL(LastSyncDate, Null)  AS 'LastSyncDate'
	  FROM @ValidPersona pe        
	  INNER JOIN Ident.UserLoginPersona iulp ON 
		pe.UserLoginPersonaId = iulp.UserLoginPersonaId	   
	  INNER JOIN Ident.UserLogin ul ON 
		iulp.UserLoginId = ul.UserId      
	  INNER JOIN Person.Person p ON p.PartyId = ul.PersonPartyId
	  LEFT JOIN CTE_PersonaAssignedPrimaryProperties CPAP ON
		pe.PersonaId = CPAP.PersonaId
	  LEFT JOIN CTE_PersonaMatchedPrimaryProperties CPMP ON
		pe.PersonaId = CPMP.PersonaId
	  LEFT JOIN CTE_UserSyncJobStatus CUSJ ON
		pe.PersonaId = CUSJ.PersonaId
	  LEFT JOIN CTE_UserPropertiesSynced CUPS ON
		pe.PersonaId = CUPS.PersonaId
	  LEFT JOIN Enterprise.StatusType est ON CUSJ.StatusId = est.StatusTypeId  
	  LEFT OUTER JOIN @filterStatus fs ON (est.StatusTypeId = fs.StatusTypeId)      
	   WHERE ((@filterStatusTypeId = 0) OR (NOT fs.StatusTypeId IS NULL) OR (ISNULL(CUSJ.StatusId,1) = @filterStatusTypeId))  

	  ;WITH cteUsersFinal      
	  (     
		PersonaId,  
		FirstName,      
		MiddleName,      
		LastName,      
		LoginName,
		StatusId,      
		StatusName,
		LastRefreshDate,
		LastSyncDate,
		AssignedCount,
		MatchedCount,
		TotalRecords,      
		RowNumber
		)
		AS
	   (SELECT PersonaId,
		   FirstName,
		   MiddleName,
		   LastName,
		   LoginName,
		   ISNULL(StatusId,1),      
		   ISNULL(StatusName,'New'),
		   LastRefreshed,
		   LastSynced,
		   AssignedCount,
		   MatchedCount,
		   COUNT(1) OVER () AS TotalRecords,      
		   CASE @sortValue        
			  WHEN 100 THEN ROW_NUMBER() OVER (ORDER BY FirstName + ' ' + LastName ASC)        
			  WHEN 101 THEN ROW_NUMBER() OVER (ORDER BY LoginName ASC, FirstName + ' ' + LastName ASC)        
			  WHEN 102 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(StatusName,'New') ASC, FirstName + ' ' + LastName ASC)        
			  WHEN 103 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(LastRefreshed,'') ASC, FirstName + ' ' + LastName ASC) 
			  WHEN 104 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(LastSynced,'') ASC, FirstName + ' ' + LastName ASC)     
			  WHEN -101 THEN ROW_NUMBER() OVER (ORDER BY LoginName DESC, FirstName + ' ' + LastName DESC)        
			  WHEN -102 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(StatusName,'New') DESC, FirstName + ' ' + LastName DESC)        
			  WHEN -103 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(LastRefreshed,'') DESC, FirstName + ' ' + LastName DESC)  
			  WHEN -104 THEN ROW_NUMBER() OVER (ORDER BY ISNULL(LastSynced,'') DESC, FirstName + ' ' + LastName DESC) 
			 END AS RowNumber
		FROM #UserSync UY
		 WHERE  (        
				(@filterName IS NULL)        
				OR (CHARINDEX(@filterName, FirstName + ' ' + LastName, 1) > 0)        
				OR (CHARINDEX(@filterName, LoginName, 1) > 0)    
	 ))

	 SELECT PersonaId,  
		FirstName,      
		MiddleName,      
		LastName,      
		LoginName,
		StatusId,      
		StatusName,
		LastRefreshDate,
		LastSyncDate,
		AssignedCount,
		MatchedCount,
		TotalRecords
	 FROM cteUsersFinal
	 ORDER BY RowNumber      
	 OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS      
	 FETCH NEXT(@RowsPerPage) ROWS ONLY      
	 OPTION (RECOMPILE)  
END