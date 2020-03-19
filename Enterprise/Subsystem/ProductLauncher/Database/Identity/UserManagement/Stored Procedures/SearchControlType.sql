
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ControlType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].[SearchControlType] (
	 @ControlTypeId INT = NULL 
	,@Name NVARCHAR(100) = NULL 
	,@Description NVARCHAR(510) = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS

BEGIN

	SELECT
		 [ControlTypeId]
		,[Name]
		,[Description]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[ControlType]
	WHERE 
		(@ControlTypeId IS NULL  OR  [ControlTypeId] = @ControlTypeId)
	AND
		(@Name IS NULL OR [Name] = @Name OR CHARINDEX(@Name,[Name]) > 0)
	AND
		(@Description IS NULL OR [Description] = @Description OR CHARINDEX(@Description,[Description]) > 0)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
OPTION(RECOMPILE);

END