
-- =============================================
-- Author:		Monte Jennings
-- Create date: 
-- Description: List of Active Product Access Pages.
-- =============================================
CREATE PROCEDURE [UserManagement].[ListActiveProductPage]
AS
BEGIN
	SELECT
		 [ProductPageId]
		,[ProductId]
		,[DisplayName]
		,[IsActive]
		,[CreatedBy]
		,[CreatedDate]
	FROM
		[UserManagement].[ProductPage]
	WHERE
		[IsActive] = 1
END
