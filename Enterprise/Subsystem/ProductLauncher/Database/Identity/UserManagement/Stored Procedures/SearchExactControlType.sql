
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ControlType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].SearchExactControlType (
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
		(@Name IS NULL  OR  [Name] = @Name)
	AND
		(@Description IS NULL  OR  [Description] = @Description)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
OPTION(RECOMPILE);

END