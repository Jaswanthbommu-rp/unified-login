
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the ControlType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetControlTypeJoinControlByParentControlId (
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
		[UserManagement].[Control] A
	On
		[A].[ControlTypeId] = [UserManagement].[ControlType].[ControlTypeId]
	INNER Join
		[UserManagement].[Control] B
	On
		[A].[ParentControlId] = [B].[ControlId]
	WHERE
		[B].[ControlId] = @ControlId

END