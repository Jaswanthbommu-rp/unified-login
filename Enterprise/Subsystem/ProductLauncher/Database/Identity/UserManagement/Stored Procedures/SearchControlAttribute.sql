
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ControlAttribute table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].[SearchControlAttribute] (
	 @ControlAttributeId INT = NULL 
	,@ControlId INT = NULL 
	,@Key NVARCHAR(100) = NULL 
	,@Value NVARCHAR(100) = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS

BEGIN

	SELECT
		 [ControlAttributeId]
		,[ControlId]
		,[Key]
		,[Value]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[ControlAttribute]
	WHERE 
		(@ControlAttributeId IS NULL  OR  [ControlAttributeId] = @ControlAttributeId)
	AND
		(@ControlId IS NULL  OR  [ControlId] = @ControlId)
	AND
		(@Key IS NULL OR [Key] = @Key OR CHARINDEX(@Key,[Key]) > 0)
	AND
		(@Value IS NULL OR [Value] = @Value OR CHARINDEX(@Value,[Value]) > 0)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
OPTION(RECOMPILE);

END