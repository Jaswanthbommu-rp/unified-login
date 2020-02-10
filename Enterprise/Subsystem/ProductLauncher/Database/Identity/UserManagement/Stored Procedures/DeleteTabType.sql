
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the TabType table.
-- =============================================
CREATE PROCEDURE [UserManagement].DeleteTabType (
	 @TabTypeId INT = NULL 
	,@UIId NVARCHAR(510) = NULL 
	,@DisplayName NVARCHAR(510) = NULL 
	,@Sequence TINYINT = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS
BEGIN



	DELETE 
		[UserManagement].[TabType]

	WHERE
		(@TabTypeId IS NULL  OR  [TabTypeId] = @TabTypeId)
	AND
		(@UIId IS NULL  OR  [UIId] = @UIId)
	AND
		(@DisplayName IS NULL  OR  [DisplayName] = @DisplayName)
	AND
		(@Sequence IS NULL  OR  [Sequence] = @Sequence)

	RETURN 1 --for success

END