
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Control table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].[SearchExactControl] (
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

	SELECT
		 [ControlId]
		,[ParentControlId]
		,[ControlTypeId]
		,[UIId]
		,[DisplayName]
		,[DataSource]
		,[Sequence]
		,[CreatedBy]
		,[CreatedDate]
	FROM
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
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
OPTION(RECOMPILE);

END