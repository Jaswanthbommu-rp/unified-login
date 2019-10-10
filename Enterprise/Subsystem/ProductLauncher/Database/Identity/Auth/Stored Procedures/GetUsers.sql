CREATE PROCEDURE [Auth].[GetUsers]
       @skipRows int = 0,
       @takeRows int = 0,
       @whereCondition nvarchar(max) = null,
	   @whereProductSubCondition nvarchar(max) = null,
       @sortColumnList nvarchar(max) = null	   
AS
BEGIN

SELECT 'No. Do not do this.' AS Information

--declare @sql nvarchar(max) = ''
--declare @joinProduct nvarchar(max) = ''
--declare @condition nvarchar(max) = ''


--SET NOCOUNT ON;

--if (@whereProductSubCondition is not null and @whereProductSubCondition <> '')
--begin
--	set @joinProduct += '		
--		INNER JOIN [Auth].[PortfolioProductUser] WITH(NOLOCK) PPU ON PPU.UserId = U.UserId 
--		LEFT JOIN Product WITH(NOLOCK) ON PPU.ProductId = Product.ProductId '
--end

--if (@whereCondition is not null and @whereCondition <> '')
--begin
--	set @condition += ' WHERE '
--end

--if (@whereCondition is not null and @whereCondition <> '')
--begin
--	set @condition += ' (' + cast(@whereCondition as nvarchar(max)) + ') '
--end

--if (@whereProductSubCondition is not null and @whereProductSubCondition <> '')
--begin
--	if (@whereCondition is not null and @whereCondition <> '')
--		set @condition += ' AND '
--	set @condition += ' ' + cast(@whereProductSubCondition as nvarchar(max))	
--end

--set @sql += 'SELECT COUNT(DISTINCT U.UserId) as [COUNT] from [Auth].[Users] U WITH(NOLOCK) '
--set @sql += @joinProduct 
--set @sql += @condition 

--set @sql += '         
--	SELECT * INTO #tempUsers
--		FROM (SELECT DISTINCT U.* FROM [Auth].[Users] U WITH(NOLOCK) '

--set @sql += @joinProduct 
--set @sql += @condition 

--if (@sortColumnList is not null and @sortColumnList <> '')
--begin
--	set @sql += ' ORDER BY ' + cast(@sortColumnList as nvarchar(max))
--end
--else
--begin
--    set @sql += ' ORDER BY IsActive ASC, FirstName ASC '
--end

--set @sql += ' OFFSET @_skipRows ROWS '

--if (@takeRows > 0)
--begin
--	set @sql += ' FETCH NEXT @_takeRows ROWS ONLY '
--end

--set @sql += ') AS U '
	
--Set @sql += '

--	SELECT * FROM #tempUsers WITH(NOLOCK) 
	
--	SELECT PPU.*, Product.* 
--		FROM #tempUsers U WITH(NOLOCK)
--		INNER JOIN
--		(
--			SELECT ProductId, UserId 
--			FROM [Auth].[PortfolioProductUser] WITH(NOLOCK)
--			GROUP BY ProductId, UserId 
--		) PPU 
--		ON PPU.UserId = U.UserId 
--		INNER JOIN Product WITH(NOLOCK) ON PPU.ProductId = Product.ProductId AND Product.ProductName NOT IN (''landing'', ''clientportal'') '
		
--set @sql += @condition 

--Set @sql += ' 	
--	DROP TABLE IF EXISTS #tempUsers '

----print @sql

--exec sp_executesql @sql, N'@_skipRows int, @_takeRows int', @skipRows, @takeRows

END