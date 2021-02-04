
CREATE PROCEDURE [UserManagement].[DeletePage] @productPageId INT
AS
BEGIN
	DELETE FROM UserManagement.ProductPageControl
	WHERE ProductPageId = @productPageId

	DELETE FROM UserManagement.ProductPage
	WHERE ProductPageId = @productPageId
END