
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the TabType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].SearchTabType (
	 @TabTypeId INT = NULL 
	,@UIId NVARCHAR(510) = NULL 
	,@DisplayName NVARCHAR(510) = NULL 
	,@Sequence TINYINT = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS

BEGIN

	SELECT
		 [TabTypeId]
		,[UIId]
		,[DisplayName]
		,[Sequence]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[TabType]
	WHERE 
		(@TabTypeId IS NULL  OR  [TabTypeId] = @TabTypeId)
	AND
		(@UIId IS NULL OR [UIId] = @UIId OR CHARINDEX(@UIId,[UIId]) > 0)
	AND
		(@DisplayName IS NULL OR [DisplayName] = @DisplayName OR CHARINDEX(@DisplayName,[DisplayName]) > 0)
	AND
		(@Sequence IS NULL  OR  [Sequence] = @Sequence)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
OPTION(RECOMPILE);

END