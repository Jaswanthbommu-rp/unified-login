
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the TabType table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [UserManagement].GetTabTypeByTabTypeControlOnTabTypeControlId (
@TabTypeControlId INT
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
		[UserManagement].[TabTypeControl]
	On
		[UserManagement].[TabTypeControl].[TabTypeControlId] = [UserManagement].[TabType].[TabTypeId]
	WHERE
		[UserManagement].[TabTypeControl].[TabTypeControlId] = @TabTypeControlId

END