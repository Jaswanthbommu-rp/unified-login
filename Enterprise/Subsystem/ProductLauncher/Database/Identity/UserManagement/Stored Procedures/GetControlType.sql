
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the ControlType table.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetControlType] (
	 @ControlTypeId INT) 

 AS 

	SELECT
		 [ControlTypeId]
		,[Name]
		,[Description]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[ControlType]
	WHERE
		[ControlTypeId] = @ControlTypeId