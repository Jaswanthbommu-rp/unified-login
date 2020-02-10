
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the TabType table.
-- =============================================
CREATE PROCEDURE [UserManagement].GetTabType (
	 @TabTypeId INT) 

 AS 

	SELECT
		 [TabTypeId]
		,[UIId]
		,[DisplayName]
		,[Sequence]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[TabType]
	WHERE
		[TabTypeId] = @TabTypeId