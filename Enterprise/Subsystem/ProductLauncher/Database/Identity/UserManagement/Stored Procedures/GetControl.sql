
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the Control table.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetControl] (
	 @ControlId INT) 

 AS 

	SELECT
		 C.[ControlId] AS ControlId
		,C.[ParentControlId] AS ParentControlId
		,C.[ControlTypeId] AS ControlTypeId
		,C.[UIId] AS UIId
		,C.[DisplayName] AS DisplayName
		,C.[DataSource] AS DataSource
		,C.[Sequence] AS Sequence
		,C.[CreatedBy] AS CreatedBy
		,C.[CreatedDate] AS CreatedDate
		,CT.[Name] AS ControlType
	FROM
		[UserManagement].[Control] C
		INNER JOIN [UserManagement].[ControlType] CT ON CT.ControlTypeId = C.ControlTypeId
	WHERE
		[ControlId] = @ControlId