
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Comparator table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].SearchComparator (
	 @ComparatorId INT = NULL 
	,@Name NVARCHAR(100) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy BIGINT = NULL 
)

AS

BEGIN

	SELECT
		 [ComparatorId]
		,[Name]
		,[CreatedDate]
		,[CreatedBy]
	FROM
		[UserManagement].[Comparator]
	WHERE 
		(@ComparatorId IS NULL  OR  [ComparatorId] = @ComparatorId)
	AND
		(@Name IS NULL OR [Name] = @Name OR CHARINDEX(@Name,[Name]) > 0)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)
OPTION(RECOMPILE);

END