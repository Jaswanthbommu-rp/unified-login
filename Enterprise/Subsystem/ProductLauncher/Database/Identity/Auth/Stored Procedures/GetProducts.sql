CREATE PROCEDURE [Auth].[GetProducts]
(
    @skipRows int = 0,
	@takeRows int = 0,
    @whereCondition nvarchar(max) = null,	   
    @sortColumnList nvarchar(max) = null	   
)
AS
BEGIN
       
	SELECT 'No. This is bad.' AS Information

--declare @sql nvarchar(max) = ''
--declare @condition nvarchar(max) = ' WHERE TitleId IS NOT NULL '


--SET NOCOUNT ON;

--if (@whereCondition is not null and @whereCondition <> '')
--begin
--	set @condition += ' AND '
--end

--if (@whereCondition is not null and @whereCondition <> '')
--begin
--	set @condition += ' (' + cast(@whereCondition as nvarchar(max)) + ') '
--end

--set @sql += '
--	SELECT COUNT(1) as [COUNT] from [Auth].[Product] WITH(NOLOCK) '
--set @sql += @condition 

--set @sql += '
--	SELECT ProductId, ProductName, Description, ClientId, ClassName, SettingsUrl, ProductUrl, SubDescription, TitleId, 
--		IsNewTab FROM [Auth].[Product] WITH(NOLOCK) '
--set @sql += @condition 

--if (@sortColumnList is not null and @sortColumnList <> '')
--begin
--	set @sql += ' ORDER BY ' + cast(@sortColumnList as nvarchar(max))
--end
--else
--begin
--    set @sql += ' ORDER BY TitleId ASC '
--end

--set @sql += ' OFFSET @_skipRows ROWS '

--if (@takeRows > 0)
--begin
--	set @sql += ' FETCH NEXT @_takeRows ROWS ONLY '
--end

--print @sql

--exec sp_executesql @sql, N'@_skipRows int, @_takeRows int', @skipRows, @takeRows

END
