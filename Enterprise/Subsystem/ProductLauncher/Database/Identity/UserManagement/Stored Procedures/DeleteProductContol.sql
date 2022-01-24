CREATE PROCEDURE [UserManagement].[DeleteProductContol] @controlId INT  
AS  
BEGIN  

IF NOT EXISTS( SELECT * FROM UserManagement.Control WHERE ParentControlId = @controlId)
BEGIN
	DELETE FROM UserManagement.ControlAttribute  
	WHERE ControlId = @controlId  
  
	DELETE FROM UserManagement.ProductPageControl
	WHERE ControlId = @controlId

	DELETE FROM UserManagement.Control  
	WHERE ControlId = @controlId  
 END
END  
