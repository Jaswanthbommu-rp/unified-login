CREATE PROCEDURE [UserManagement].[DeletePage] @productPageId INT  
AS  
BEGIN  
 IF NOT EXISTS(SELECT * FROM UserManagement.ProductPageControl WHERE ProductPageId = @productPageId )
 BEGIN
	DELETE FROM UserManagement.ProductPage  
	WHERE ProductPageId = @productPageId  
 END
END
