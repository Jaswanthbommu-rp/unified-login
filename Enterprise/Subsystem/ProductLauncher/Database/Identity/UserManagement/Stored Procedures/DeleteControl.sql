
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the Control table.
-- =============================================
CREATE PROCEDURE [UserManagement].[DeleteControl] (
	 @ControlId INT = NULL 
	,@ParentControlId INT = NULL 
	,@ControlTypeId INT = NULL 
	,@UIId NVARCHAR(510) = NULL 
	,@DisplayName NVARCHAR(510) = NULL 
	,@DataSource NVARCHAR(MAX) = NULL 
	,@Sequence TINYINT = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS
BEGIN



	DELETE 
		[UserManagement].[Control]

	WHERE
		(@ControlId IS NULL  OR  [ControlId] = @ControlId)
	AND
		(@ParentControlId IS NULL  OR  [ParentControlId] = @ParentControlId)
	AND
		(@ControlTypeId IS NULL  OR  [ControlTypeId] = @ControlTypeId)
	AND
		(@UIId IS NULL  OR  [UIId] = @UIId)
	AND
		(@DisplayName IS NULL  OR  [DisplayName] = @DisplayName)
	AND
		(@DataSource IS NULL  OR  [DataSource] = @DataSource)
	AND
		(@Sequence IS NULL  OR  [Sequence] = @Sequence)

	RETURN 1 --for success

END