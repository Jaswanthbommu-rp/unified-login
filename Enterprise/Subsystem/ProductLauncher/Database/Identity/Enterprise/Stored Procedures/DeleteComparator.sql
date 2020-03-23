
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the Comparator table.
-- =============================================
CREATE PROCEDURE [Enterprise].DeleteComparator (
	 @ComparatorId INT = NULL 
	,@Name NVARCHAR(100) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy BIGINT = NULL 
)

AS
BEGIN



	DELETE 
		[Enterprise].[Comparator]

	WHERE
		(@ComparatorId IS NULL  OR  [ComparatorId] = @ComparatorId)
	AND
		(@Name IS NULL  OR  [Name] = @Name)

	RETURN 1 --for success

END