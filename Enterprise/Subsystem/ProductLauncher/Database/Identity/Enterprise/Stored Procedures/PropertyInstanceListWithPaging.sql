/*
declare @p1 Enterprise.PropertyInstanceType
insert into @p1 values('003B0509-1189-49DC-BBE6-01C5B6277A83')
insert into @p1 values('00A853D7-72C2-4A40-80DD-0C12ED9AA761')
insert into @p1 values('01F94ECA-0F6F-4170-B1B7-D9921A744EE8')
exec Enterprise.GetPropertyInstanceListByIdWithPaging @InstanceList=@p1,@Name=NULL,
@Domain='UAT',@SortColumn=N'Name',@SortDirection=N'Asc',@RowsPerPage=100,@PageNumber=1  

*/
-- Procedure : Enterprise.GetPropertyInstanceListByIdWithPaging  
-- Purpose  : Select Property list   for a company
-- Date     Author    Comment  
-------------------------------------------------------------------------------------------------------------------------------------------  
-- 12/03/2020   RohithVundyala    Created  
-- Copyright  : copyright (c) 2015.  RealPage Inc.  
-- This module is the confidential & proprietary property of RealPage Inc.  
-------------------------------------------------------------------------------------------------------------------------------------------  
CREATE PROCEDURE [Enterprise].[GetPropertyInstanceListByIdWithPaging]   
(   
 @InstanceList [Enterprise].[PropertyInstanceType] READONLY,  
 @Name  VARCHAR(MAX) = NULL,  
 @Domain		VARCHAR(MAX) = NULL, 
 @SortColumn    VARCHAR(256) = 'Name',  
 @SortDirection   VARCHAR(4) = 'Asc',  
 @RowsPerPage   INT     = 0,  
 @PageNumber    INT     = 1  
)  
AS  
BEGIN  
 DECLARE @sortValue INT  
 SELECT @RowsPerPage = CASE WHEN @RowsPerPage <= 0 THEN 2147483647 ELSE @RowsPerPage END  


 CREATE TABLE #tempPropertyInstance  
 (  
  PropertyInstanceId	BIGINT,   
  Name					NVARCHAR(300),  
  Address				NVARCHAR(400),  
  City					NVARCHAR(120),  
  State					NVARCHAR(40),  
  PostalCode			NVARCHAR(50),  
  Country				NVARCHAR(50),  
  County				NVARCHAR(120), 
  InstanceId			UNIQUEIDENTIFIER,
  CustomerPropertyId	BIGINT,
  Domain				NVARCHAR(100)
 )   
 INSERT INTO #tempPropertyInstance(
			PropertyInstanceId    
			,[Name]  
			,[Address]  
			,[City]  
			,[State]  
			,[PostalCode]  
			,[Country]  
			,[County]		 
			,PI1.[InstanceId]  
			,PI1.CustomerPropertyId   
           ,Domain)  
 SELECT  
		  PI1.[PropertyInstanceId]  
		  ,[Name]  
		  ,[Address]  
		  ,[City]  
		  ,[State]  
		  ,[PostalCode]  
		  ,[Country]  
		  ,[County]
		  ,PI1.[InstanceId]  
		  ,PI1.CustomerPropertyId
		  ,Domain
  
 FROM   
  [Enterprise].[PropertyInstance] pi1  
  INNER JOIN @InstanceList IL  
   ON IL.InstanceId = PI1.InstanceId
    WHERE (@Name IS NULL OR pi1.Name LIKE '%' + @Name + '%') 
  AND (@Domain IS NULL OR pi1.Domain like '%' + @Domain + '%')
  
 SELECT @sortValue =  
  CASE @SortColumn  
   WHEN N'Name' THEN 100  
   WHEN N'Domain' THEN 101  
   ELSE 102  
  END * CASE UPPER(@SortDirection) WHEN N'ASC' THEN 1 WHEN N'DESC' THEN -1 END;  
  
  
 WITH cteFilterProperties  
  (  
   PropertyInstanceId    
	,[Name]  
	,[Address]  
	,[City]  
	,[State]  
	,[PostalCode]  
	,[Country]  
	,[County]		 
	,[InstanceId]  
	,CustomerPropertyId   
    ,Domain
	,TotalRecords
	,RowNumber
  )  
 AS  
 (  
  SELECT   
	PropertyInstanceId    
	,[Name]  
	,[Address]  
	,[City]  
	,[State]  
	,[PostalCode]  
	,[Country]  
	,[County]		 
	,[InstanceId]  
	,CustomerPropertyId   
    ,Domain,  
   COUNT(1) OVER () AS [TotalRecords],  
   CASE @sortValue  
    WHEN 100 THEN ROW_NUMBER() OVER (ORDER BY Name ASC)  
    WHEN 101 THEN ROW_NUMBER() OVER (ORDER BY Domain ASC)  
    WHEN -100 THEN ROW_NUMBER() OVER (ORDER BY Name DESC)  
    WHEN -101 THEN ROW_NUMBER() OVER (ORDER BY Domain DESC)  
   END AS [RowNumber]  
   FROM #tempPropertyInstance  
 )  
   
 SELECT   
	PropertyInstanceId    
	,[Name]  
	,[Address]  
	,[City]  
	,[State]  
	,[PostalCode]  
	,[Country]  
	,[County]		 
	,[InstanceId]  
	,CustomerPropertyId   
    ,Domain,  
	TotalRecords  
 FROM cteFilterProperties  
 ORDER BY RowNumber  
 OFFSET ((@PageNumber - 1) * @RowsPerPage) ROWS  
    FETCH NEXT @RowsPerPage ROWS ONLY  
  
 drop table #tempPropertyInstance  
END  