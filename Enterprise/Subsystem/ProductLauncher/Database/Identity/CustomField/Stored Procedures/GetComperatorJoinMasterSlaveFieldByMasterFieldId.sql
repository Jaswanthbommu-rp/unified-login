
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Comperator table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].GetComperatorJoinMasterSlaveFieldByMasterFieldId (
@FieldId BIGINT
)
AS

BEGIN
	SELECT
		 [CustomField].[Comperator].[ComperatorId]
		,[CustomField].[Comperator].[Name]
		,[CustomField].[Comperator].[CreatedDate]
		,[CustomField].[Comperator].[CreatedBy]
	FROM
		[CustomField].[Comperator]
	INNER Join
		[CustomField].[MasterSlaveField] A
	On
		[A].[ComperatorId] = [CustomField].[Comperator].[ComperatorId]
	INNER Join
		[CustomField].[Field] B
	On
		[A].[MasterFieldId] = [B].[FieldId]
	WHERE
		[B].[FieldId] = @FieldId

END