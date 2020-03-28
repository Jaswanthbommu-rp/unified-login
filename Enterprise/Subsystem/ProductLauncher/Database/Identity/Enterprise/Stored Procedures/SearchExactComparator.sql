
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Comparator table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [Enterprise].SearchExactComparator (
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
		[Enterprise].[Comparator]
	WHERE 
		(@ComparatorId IS NULL  OR  [ComparatorId] = @ComparatorId)
	AND
		(@Name IS NULL  OR  [Name] = @Name)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)
OPTION(RECOMPILE);

END