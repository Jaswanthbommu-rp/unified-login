
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Deletes the record with the indicated ID from the ControlType table.
-- =============================================
CREATE PROCEDURE [UserManagement].DeleteControlType (
	 @ControlTypeId INT = NULL 
	,@Name NVARCHAR(100) = NULL 
	,@Description NVARCHAR(510) = NULL 
	,@CreatedBy BIGINT = NULL 
	,@CreatedDate DATETIME = NULL 
)

AS
BEGIN



	DELETE 
		[UserManagement].[ControlType]

	WHERE
		(@ControlTypeId IS NULL  OR  [ControlTypeId] = @ControlTypeId)
	AND
		(@Name IS NULL  OR  [Name] = @Name)
	AND
		(@Description IS NULL  OR  [Description] = @Description)

	RETURN 1 --for success

END