
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the ControlAttribute table.
-- =============================================
CREATE PROCEDURE [UserManagement].GetControlAttributeByControlId (
	 @ControlId INT) 

 AS 

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
		[ControlId] = @ControlId