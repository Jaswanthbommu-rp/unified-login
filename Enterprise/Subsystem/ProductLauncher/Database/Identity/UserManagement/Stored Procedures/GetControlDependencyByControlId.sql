-- =============================================
-- Author:		Monte Jennings
-- Create date:
-- Description: Gets the record with the indicated ID from the ControlDependency table.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetControlDependencyByControlId] (
	 @ControlId INT
)
AS
BEGIN
	SELECT		umcd.[ControlDependencyId]
					,umcd.[MasterControlId]
					,umcm.[UIId] AS MasterControlUIId
					,umcd.[SlaveControlId]
					,umcs.[UIId] AS SlaveControlUIId
					,umcs.[DisplayName]
					,umcd.[MasterControlValue]
					,ec.[ComparatorId]
					,ec.[Name]
					,umcd.[CreatedBy]
					,umcd.[CreatedDate]
	FROM		[UserManagement].[ControlDependency] umcd
					INNER JOIN [Enterprise].[Comparator] ec ON umcd.ComparatorId = ec.ComparatorId
					INNER JOIN [UserManagement].[Control] umcs ON umcs.[ControlId] = umcd.[SlaveControlId]
					INNER JOIN [UserManagement].[Control] umcm ON umcm.[ControlId] = umcd.[MasterControlId]
	WHERE		umcd.[MasterControlId] = @ControlId
END