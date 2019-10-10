
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the Comperator table.
-- =============================================
CREATE PROCEDURE [CustomField].DeleteComperator (
	 @ComperatorId TINYINT = NULL 
	,@Name NVARCHAR(100) = NULL 
	,@CreatedDate DATETIME = NULL 
	,@CreatedBy NVARCHAR(650) = NULL 
)

AS
BEGIN


	UPDATE
		[CustomField].[Comperator]--Captures the current record for auditing purposes
		SET
		 CreatedBy = @CreatedBy
		,CreatedDate = @CreatedDate
	WHERE
		(@ComperatorId IS NULL  OR  [ComperatorId] = @ComperatorId)
	AND
		(@Name IS NULL  OR  [Name] = @Name)

	DELETE 
		[CustomField].[Comperator]

	WHERE
		(@ComperatorId IS NULL  OR  [ComperatorId] = @ComperatorId)
	AND
		(@Name IS NULL  OR  [Name] = @Name)

	RETURN 1 --for success

END

