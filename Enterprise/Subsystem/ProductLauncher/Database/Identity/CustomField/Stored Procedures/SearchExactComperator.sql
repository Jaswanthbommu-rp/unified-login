
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Comperator table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].SearchExactComperator (
	 @ComperatorId TINYINT = NULL 
	,@Name NVARCHAR(100) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy NVARCHAR(650) = NULL 
)

AS

BEGIN

	SELECT
		 [ComperatorId]
		,[Name]
		,[CreatedDate]
		,[CreatedBy]
	FROM
		[CustomField].[Comperator]
	WHERE 
		(@ComperatorId IS NULL  OR  [ComperatorId] = @ComperatorId)
	AND
		(@Name IS NULL  OR  [Name] = @Name)
	AND
		(@CreatedDate IS NULL  OR  [CreatedDate] = @CreatedDate)
	AND
		(@CreatedBy IS NULL  OR  [CreatedBy] = @CreatedBy)

END