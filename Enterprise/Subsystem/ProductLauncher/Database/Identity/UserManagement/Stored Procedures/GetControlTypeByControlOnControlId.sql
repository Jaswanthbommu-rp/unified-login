
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ControlType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetControlTypeByControlOnControlId] (
@ControlId INT
)
AS

BEGIN
	SELECT
		 [UserManagement].[ControlType].[ControlTypeId]
		,[UserManagement].[ControlType].[Name]
		,[UserManagement].[ControlType].[Description]
		,[UserManagement].[ControlType].[CreatedBy]
		,[UserManagement].[ControlType].[CreatedDate]
	FROM
		[UserManagement].[ControlType]
	INNER Join
		[UserManagement].[Control]
	On
		[UserManagement].[Control].[ControlId] = [UserManagement].[ControlType].[ControlTypeId]
	WHERE
		[UserManagement].[Control].[ControlId] = @ControlId

END