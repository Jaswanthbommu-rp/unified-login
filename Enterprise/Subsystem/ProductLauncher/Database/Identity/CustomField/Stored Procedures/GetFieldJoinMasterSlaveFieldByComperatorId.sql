
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Searches the Field table for the record with the indicated criteria.
-- =============================================
CREATE PROCEDURE [CustomField].GetFieldJoinMasterSlaveFieldByComperatorId (
@ComperatorId TINYINT
)
AS

BEGIN
	SELECT
		 [CustomField].[Field].[FieldId]
		,[CustomField].[Field].[OrganizationId]
		,[CustomField].[Field].[Enabled]
		,[CustomField].[Field].[Name]
		,[CustomField].[Field].[Description]
		,[CustomField].[Field].[FieldTypeId]
		,[CustomField].[Field].[Required]
		,[CustomField].[Field].[ReadOnly]
		,[CustomField].[Field].[DefaultValue]
		,[CustomField].[Field].[SyncField]
		,[CustomField].[Field].[Sequence]
		,[CustomField].[Field].[HelpText]
		,[CustomField].[Field].[CreatedDate]
		,[CustomField].[Field].[CreatedBy]
	FROM
		[CustomField].[Field]
	INNER Join
		[CustomField].[MasterSlaveField] A
	On
		[A].[SlaveFieldId] = [CustomField].[Field].[FieldId]
	INNER Join
		[CustomField].[Comperator] B
	On
		[A].[ComperatorId] = [B].[ComperatorId]
	WHERE
		[B].[ComperatorId] = @ComperatorId

END