
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the TabType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetTabTypeJoinTabTypeControlByControlId (
@ControlId INT
)
AS

BEGIN
	SELECT
		 [UserManagement].[TabType].[TabTypeId]
		,[UserManagement].[TabType].[UIId]
		,[UserManagement].[TabType].[DisplayName]
		,[UserManagement].[TabType].[Sequence]
		,[UserManagement].[TabType].[CreatedBy]
		,[UserManagement].[TabType].[CreatedDate]
	FROM
		[UserManagement].[TabType]
	INNER Join
		[UserManagement].[TabTypeControl] A
	On
		[A].[TabTypeId] = [UserManagement].[TabType].[TabTypeId]
	INNER Join
		[UserManagement].[Control] B
	On
		[A].[ControlId] = [B].[ControlId]
	WHERE
		[B].[ControlId] = @ControlId

END