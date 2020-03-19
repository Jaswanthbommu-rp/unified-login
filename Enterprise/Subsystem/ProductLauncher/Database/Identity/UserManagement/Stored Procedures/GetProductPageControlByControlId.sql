
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: Gets the record with the indicated ID from the ProductPageControl table.
-- =============================================
CREATE PROCEDURE [UserManagement].[GetProductPageControlByControlId] (
	 @ControlId INT) 

 AS 

	SELECT
		 [ProductPageControlId]
		,[ProductPageId]
		,[ControlId]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[ProductPageControl]
	WHERE
		[ControlId] = @ControlId