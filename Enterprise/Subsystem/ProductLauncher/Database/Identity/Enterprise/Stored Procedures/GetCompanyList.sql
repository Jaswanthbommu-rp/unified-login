-- EXEC  [Enterprise].[GetCompanyList] 'real','pri',3,@RowsPerPage=3
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
	@Domain					varchar(20) = NULL,
	@BooksCustomerMasterId	VARCHAR(20) = NULL,
	@SortColumn				VARCHAR(256) = 'OrganizationName',
	@SortDirection			VARCHAR(4) = 'Asc',
	@RowsPerPage			INT     = 0,
	@PageNumber				INT     = 1
)
AS
BEGIN
	DECLARE @sortValue INT
	SELECT @RowsPerPage = CASE WHEN @RowsPerPage <= 0 THEN 2147483647 ELSE @RowsPerPage END
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
		AND	(@Domain IS NULL OR OD.Name LIKE '%' + @Domain + '%')
		AND	(@BooksCustomerMasterId IS NULL OR D.CompanyMasterId LIKE '%' + @BooksCustomerMasterId + '%')

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