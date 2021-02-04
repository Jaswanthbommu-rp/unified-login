CREATE PROCEDURE [UserManagement].[DeleteProductContol] @controlId INT
AS
BEGIN
	DELETE FROM UserManagement.ControlAttribute
	WHERE ControlId = @controlId

	DELETE FROM UserManagement.Control
	where ParentControlId = @controlId

	DELETE FROM UserManagement.Control
	WHERE ControlId = @controlId
END
