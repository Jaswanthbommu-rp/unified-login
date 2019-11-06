
CREATE PROCEDURE [Logging].[ListLoggingMetadata] 	
AS
BEGIN
 
	SET NOCOUNT ON;
	-- Get Product, log type & log category list to display in drop-down box of search UI
	-- testing 3
	select ProductID, BooksProductCode from Logging.Product
	select LogCategoryTypeid, Name  from   Logging.LogCategoryType
	select LogTypeId,Name,LogcategoryTypeId from Logging.LogType
	
END