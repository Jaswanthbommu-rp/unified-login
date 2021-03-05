-- EXEC  [Enterprise].[GetCompanyList] null,0,0,7,@RowsPerPage=3
-- EXEC  [Enterprise].[GetCompanyList] 'c',0,0,null,@FilterByProduct='26,45',@FilterByType='6',@FilterByDomain='1,2'
-- Procedure	: Enterprise.GetCompanyList
-- Purpose		: Select Company list
-- Date			  Author				Comment
-------------------------------------------------------------------------------------------------------------------------------------------
-- 10/20/2020	  RohithVundyala				Created
-- Copyright  : copyright (c) 2015.  RealPage Inc.
-- This module is the confidential & proprietary property of RealPage Inc.
-------------------------------------------------------------------------------------------------------------------------------------------
CREATE PROCEDURE [Enterprise].[GetCompanyList] 
(	
	@OrganizationName		VARCHAR(300) = NULL,
	@OrganizationID			INT = 0,
	@Domain					INT = 0,
	@BooksCustomerMasterId	VARCHAR(20) = NULL,
	@FilterByProduct		VARCHAR(MAX) = NULL,
	@FilterByDomain			VARCHAR(MAX) = NULL,
	@FilterByType			VARCHAR(MAX) = NULL,
	@SortColumn				VARCHAR(256) = 'OrganizationName',
	@SortDirection			VARCHAR(4) = 'Asc',
	@RowsPerPage			INT     = 0,
	@PageNumber				INT     = 1
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
	SELECT	CONVERT(int, value)
	FROM		STRING_SPLIT(@FilterByProduct, ',');

	--Domain Filter
	DECLARE @DomainFilter TABLE (
		DomainId int PRIMARY KEY
	)
	INSERT INTO @DomainFilter (
		DomainId
	)
	SELECT	CONVERT(int, value)
	FROM		STRING_SPLIT(@FilterByDomain, ',');

	--Type Filter
	DECLARE @TypeFilter TABLE (
		TypeId int PRIMARY KEY
	)
	INSERT INTO @TypeFilter (
		TypeId
	)
	SELECT	CONVERT(int, value)
	FROM		STRING_SPLIT(@FilterByType, ',');

	CREATE TABLE #tempOrganizations
	(
		OrganizationPartyId		bigint, 
		OrganizationName		NVARCHAR(300),
		RealPageId				UNIQUEIDENTIFIER,
		BooksMasterId			NVARCHAR(200),
		BooksCustomerMasterId	NVARCHAR(200),
		OrganizationTypeId		INT,
		OrganizationType		NVARCHAR(100),
		OrganizationDomainId	INT,
		Domain					NVARCHAR(40),
		Products				INT
	)	
	INSERT INTO #tempOrganizations(OrganizationPartyId,		
								   OrganizationName,		
								   RealPageId,				
								   BooksMasterId,			
								   BooksCustomerMasterId,	
								   OrganizationTypeId,	
								   OrganizationType,	
								   OrganizationDomainId,	
								   Domain,					
								   Products)
	SELECT O.PartyId as OrganizationPartyId,    
		   O.Name as OrganizationName,    
		   P.RealPageId,    
		   COALESCE(ISNULL(D.MasterId, 0),0) AS BooksMasterId,    
		   COALESCE(ISNULL(D.CompanyMasterId, 0), 0) AS BooksCustomerMasterId,    
		   o.OrganizationTypeId,
		   OT.Name AS OrganizationType,  
		   o.OrganizationDomainId,
		   OD.Name as Domain,
		   Products = (select count(distinct productid) 
						from Enterprise.OrganizationProduct op where o.PartyId= op.PartyId and ThruDate is null)
	FROM [Enterprise].Organization AS o    
		INNER JOIN [Enterprise].Party P ON P.PartyId = O.PartyId
		INNER JOIN Enterprise.OrganizationDomain OD ON OD.OrganizationDomainId = O.OrganizationDomainId
		INNER JOIN Enterprise.OrganizationType OT ON OT.OrganizationTypeId = O.OrganizationTypeId 
		INNER JOIN Enterprise.VW_DataImportMapping D ON(O.PartyId = D.PartyId) and d.CompanyMasterId > 1
    WHERE	(@OrganizationName IS NULL OR O.Name LIKE '%' + @OrganizationName + '%')
		AND	(@OrganizationID = 0 OR O.PartyId = @OrganizationID)
		AND	(@Domain = 0 OR O.OrganizationDomainId = @Domain)
		AND	(@BooksCustomerMasterId IS NULL OR D.CompanyMasterId LIKE '%' + @BooksCustomerMasterId + '%')
		AND (@FilterByDomain IS NULL OR O.OrganizationDomainId IN (SELECT DomainId FROM @DomainFilter))
		AND (@FilterByType IS NULL OR O.OrganizationTypeId IN (SELECT TypeId FROM @TypeFilter))
		AND (@FilterByProduct IS NULL or o.PartyId in (
			select distinct op.PartyId from 
			Enterprise.OrganizationProduct op inner join Enterprise.Organization o on o.PartyId = op.PartyId
			INNER JOIN Enterprise.VW_DataImportMapping D ON(O.PartyId = D.PartyId) and d.CompanyMasterId > 1
			where ProductId in(SELECT ProductId FROM @ProductFilter) and op.ThruDate is null
			)		
		)
	SELECT @sortValue =
		CASE @SortColumn
			WHEN N'OrganizationName' THEN 100
			WHEN N'Domain' THEN 101
			ELSE 102
		END * CASE UPPER(@SortDirection) WHEN N'ASC' THEN 1 WHEN N'DESC' THEN -1 END;


	WITH cteFilterOrganizations
		(
			OrganizationPartyId,		
			OrganizationName,		
			RealPageId,				
			BooksMasterId,			
			BooksCustomerMasterId,	
			OrganizationTypeId,	
			OrganizationType,	
			OrganizationDomainId,	
			Domain,					
			Products,
			TotalRecords, 
			RowNumber
		)
	AS
	(
		SELECT 
			OrganizationPartyId,		
			OrganizationName,		
			RealPageId,				
			BooksMasterId,			
			BooksCustomerMasterId,	
			OrganizationTypeId,	
			OrganizationType,	
			OrganizationDomainId,	
			Domain,					
			Products,
			COUNT(1) OVER () AS [TotalRecords],
			CASE @sortValue
				WHEN 100 THEN ROW_NUMBER() OVER (ORDER BY OrganizationName ASC)
				WHEN 101 THEN ROW_NUMBER() OVER (ORDER BY Domain ASC)
				WHEN -100 THEN ROW_NUMBER() OVER (ORDER BY OrganizationName DESC)
				WHEN -101 THEN ROW_NUMBER() OVER (ORDER BY Domain DESC)
			END AS [RowNumber]
			FROM #tempOrganizations
	)
	
	SELECT	
		OrganizationPartyId,		
		OrganizationName,		
		RealPageId,				
		BooksMasterId,			
		BooksCustomerMasterId,	
		OrganizationTypeId,	
		OrganizationType,	
		OrganizationDomainId,	
		Domain,					
		Products,
		TotalRecords
	FROM cteFilterOrganizations
	ORDER BY RowNumber
	OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS
    FETCH NEXT @RowsPerPage ROWS ONLY

	drop table #tempOrganizations
END
GO