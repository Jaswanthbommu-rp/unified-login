
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the Control table.
-- =============================================
CREATE PROCEDURE [UserManagement].GetControlByParentControlId (
	 @ParentControlId INT) 

 AS 

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
		[ParentControlId] = @ParentControlId