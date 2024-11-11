CREATE PROCEDURE Person.GetUsersAndProductsByCompany  
    @FilterBy NVARCHAR(MAX),  
    @OrgPartyId BIGINT,  
    @SortBy NVARCHAR(MAX),  
    @RowsPerPage INT = 10,  
    @PageNumber INT = 1  
AS  
BEGIN  
    /*        
 EXEC Person.GetUsersAndProductsByCompany @FilterBy='{"filterBy":[{"ColumnName":"Productcodes","SearchValue":"RCLMS,OS"},{"ColumnName":"name","SearchValue":"a"}]}'        
 ,@OrgPartyId=350      
 ,@SortBy='{"sortBy":[{"ColumnName":"lastName","SortDirection":"ASC"}]}'        
 ,@RowsPerPage = 10      
 ,@PageNumber=1        
   
 EXEC Person.GetUsersAndProductsByCompany @FilterBy='{"filterBy":[{"ColumnName":"Productcodes","SearchValue":"RCLMS,OS"}]}'        
 ,@OrgPartyId=350      
 ,@SortBy='{"sortBy":[{"ColumnName":"lastName","SortDirection":"ASC"}]}'        
 ,@RowsPerPage = 10       
 ,@PageNumber=1     
   
 EXEC Person.GetUsersAndProductsByCompany @FilterBy=''        
 ,@OrgPartyId=350      
 ,@SortBy='{"sortBy":[{"ColumnName":"lastName","SortDirection":"ASC"}]}'        
 ,@RowsPerPage = 10       
 ,@PageNumber=1     
 */  
    DECLARE @tblFilterBy TABLE  
    (  
        ColumnName NVARCHAR(MAX),  
        SearchValue NVARCHAR(MAX)  
    )  
    DECLARE @filterStatus TABLE (StatusTypeId INT PRIMARY KEY)  
    DECLARE @tblProductList TABLE  
    (  
        ProductId INT,  
        ProductCode NVARCHAR(MAX)  
    )  
    DECLARE @filterName NVARCHAR(MAX),  
            @csvStatus NVARCHAR(MAX),  
            @filterStatusTypeId INT,  
            @sortValue INT = 100,  
            @filterProductCode NVARCHAR(MAX)  
    IF (ISJSON(@FilterBy) = 0)  
    BEGIN  
        SET @FilterBy = NULL  
    END  
    IF (ISJSON(@SortBy) = 0)  
    BEGIN  
        SET @SortBy = NULL  
    END  
    INSERT INTO @tblFilterBy  
    (  
        ColumnName,  
        SearchValue  
    )  
    SELECT ColumnName,  
           SearchValue  
    FROM  
        OPENJSON(JSON_QUERY(@FilterBy, '$.filterBy'))  
        WITH  
        (  
            ColumnName NVARCHAR(MAX) '$.ColumnName',  
            SearchValue NVARCHAR(MAX) '$.SearchValue'  
        )  
    WHERE ISJSON(@FilterBy) > 0  
  
    SELECT @filterName = SearchValue  
    FROM @tblFilterBy  
    WHERE ColumnName = 'Name'  
          AND SearchValue NOT IN ( '%', '' )  
  
    SELECT @csvStatus = SearchValue  
    FROM @tblFilterBy  
    WHERE ColumnName = 'Status'  
          AND SearchValue NOT IN ( '%', '' )  
      
    IF (LEN(@csvStatus) > 0)  
    BEGIN  
        INSERT INTO @filterStatus  
        (  
            StatusTypeId  
        )  
        SELECT CONVERT(INT, value)  
        FROM STRING_SPLIT(@csvStatus, ',');  
  
    END  
  
    SELECT @filterProductCode = SearchValue  
    FROM @tblFilterBy  
    WHERE ColumnName = 'productcodes'  
          AND SearchValue NOT IN ( '%', '' )  
  
    IF (LEN(@filterProductCode) > 0)  
    BEGIN  
        INSERT INTO @tblProductList  
        (  
            ProductCode  
        )  
        SELECT [value]  
        FROM STRING_SPLIT(@filterProductCode, ',');  
  
        UPDATE PL  
        SET PL.ProductId = P.ProductId  
        FROM @tblProductList PL  
            JOIN Enterprise.Product P  
                ON PL.ProductCode = P.BooksProductCode  
    END  
  
    SELECT @filterStatusTypeId = COUNT(StatusTypeId)  
    FROM @filterStatus  
    WHERE StatusTypeId > 0  
  
    SELECT @SortValue = CASE ColumnName  
                            WHEN N'firstName' THEN  
                                100  
                            WHEN N'lastName' THEN  
                                101  
                            WHEN N'loginName' THEN  
                                102  
                            ELSE  
                                100  
                        END * CASE SortDirection  
                                  WHEN N'ASC' THEN  
                                      1  
                                  ELSE  
                                      -1  
                              END  
    FROM  
        OPENJSON(JSON_QUERY(@SortBy, '$.sortBy'))  
        WITH  
        (  
            ColumnName NVARCHAR(MAX) '$.ColumnName',  
            SortDirection NVARCHAR(MAX) '$.SortDirection'  
        )  
    WHERE ISJSON(@SortBy) > 0;  
  
    ;WITH UserList  
    AS (SELECT p.FirstName,  
               p.LastName,  
               ul.LoginName,  
               PP.PersonaId,  
               ul.UserId,  
               EST.Name AS UserStatus,  
               COUNT(1) OVER () AS TotalRecords,  
               CASE @sortValue  
                   WHEN 100 THEN  
                       ROW_NUMBER() OVER (ORDER BY p.FirstName + ' ' + p.LastName ASC)  
                   WHEN-100 THEN  
                       ROW_NUMBER() OVER (ORDER BY p.FirstName + ' ' + p.LastName DESC)  
                   WHEN 101 THEN  
                       ROW_NUMBER() OVER (ORDER BY p.LastName + ' ' + p.FirstName ASC)  
                   WHEN-101 THEN  
                       ROW_NUMBER() OVER (ORDER BY p.LastName + ' ' + p.FirstName DESC)  
                   WHEN 102 THEN  
                       ROW_NUMBER() OVER (ORDER BY ul.LoginName ASC)  
                   WHEN-102 THEN  
                       ROW_NUMBER() OVER (ORDER BY ul.LoginName DESC)  
               END AS RowNumber  
        FROM ident.userlogin ul  
            INNER JOIN ident.UserLoginPersona ulp  
                ON ulp.UserLoginId = ul.UserId  
            INNER JOIN person.persona PP  
                ON PP.UserLoginPersonaId = ulp.UserLoginPersonaId  
            INNER JOIN person.person p  
                ON p.PartyId = ul.PersonPartyId  
            LEFT OUTER JOIN @filterStatus fs  
                ON (ulp.StatusTypeId = fs.StatusTypeId)  
            JOIN Enterprise.StatusType EST  
                ON EST.StatusTypeId = ulp.StatusTypeId  
            JOIN Enterprise.PersonaConfiguration EPC  
                ON EPC.PersonaId = PP.PersonaId  
                   AND EPC.StatusTypeId = 8  
                   AND EPC.ProductId IN (  
                                            SELECT ProductId FROM @tblProductList  
                                        )  
                   AND EPC.ThruDate IS NULL  
        WHERE ulp.OrganizationPartyId = @OrgPartyId  
              AND (  
                      (@filterName IS NULL)  
                      OR (  
                             (CHARINDEX(@filterName, p.FirstName + ' ' + p.LastName, 1) > 0)  
                             OR (CHARINDEX(@filterName, ul.LoginName, 1) > 0)  
                         )  
                  )  
              AND (  
                      (@filterStatusTypeId = 0)  
                      OR (NOT fs.StatusTypeId IS NULL)  
                  )  
              AND ulp.IsRPEmployee = 0  
              AND 1 = (CASE  
                           WHEN (  
                                (  
                                    SELECT COUNT(1) FROM @filterStatus WHERE StatusTypeId = 2  
                                ) = 0  
                                ) THEN  
            (CASE  
                 WHEN  
                 (  
                     (ulp.StatusTypeId = 12)  
                     AND (ul.LastLoginDate IS NULL)  
                 ) THEN  
                     0  
                 ELSE  
                     1  
             END  
            )  
                           ELSE  
                               1  
                       END  
                      )  
        GROUP BY p.FirstName,  
                 p.LastName,  
                 ul.LoginName,  
                 PP.PersonaId,  
                 ul.UserId,  
                 EST.Name  
       )  
  
    SELECT UL.FirstName,  
           UL.LastName,  
           UL.LoginName,  
           UL.PersonaId,  
           UL.UserId,  
           UL.UserStatus,  
           UL.TotalRecords  
    INTO #PaginatedUsers  
    FROM UserList UL  
    ORDER BY RowNumber OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS FETCH NEXT (@RowsPerPage) ROWS ONLY  
  
    SELECT DISTINCT  
        PU.PersonaId,  
        PU.UserId,  
        PU.FirstName,  
        PU.LastName,  
        PU.LoginName,  
        PU.UserStatus,  
        PU.TotalRecords,  
        EP.ProductId,  
        EP.Name AS ProductName,  
        SA.Name,  
        SUA.Value  
    FROM #PaginatedUsers PU  
        JOIN Enterprise.PersonaConfiguration EPC  
            ON EPC.PersonaId = PU.PersonaId  
               AND EPC.StatusTypeId = 8  
               AND EPC.ProductId IN (  
                                        SELECT ProductId FROM @tblProductList  
                                    )  
               AND EPC.ThruDate IS NULL  
        JOIN Enterprise.Product EP  
            ON EPC.ProductId = EP.ProductId  
        JOIN ident.SamlUserAttribute SUA  
            ON SUA.PersonaId = EPC.PersonaId  
               AND EPC.ProductId = SUA.ProductId  
        JOIN Ident.SamlAttribute SA  
            ON SA.SamlAttributeId = SUA.SamlAttributeId  
    WHERE EPC.ThruDate IS NULL  
          AND SUA.ThruDate IS NULL  
    ORDER BY pu.PersonaId  
END