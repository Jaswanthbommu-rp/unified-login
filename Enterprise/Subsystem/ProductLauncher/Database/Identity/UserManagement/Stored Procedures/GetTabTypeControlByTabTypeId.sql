
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the TabTypeControl table.
-- =============================================
CREATE PROCEDURE [UserManagement].GetTabTypeControlByTabTypeId (
	 @TabTypeId INT) 

 AS 

	SELECT
		 [TabTypeControlId]
		,[TabTypeId]
		,[ProductPageId]
		,[ControlId]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[TabTypeControl]
	WHERE
		[TabTypeId] = @TabTypeId