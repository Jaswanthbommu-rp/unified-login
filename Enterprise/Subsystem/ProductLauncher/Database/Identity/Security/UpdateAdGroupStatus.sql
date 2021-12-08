CREATE PROCEDURE [Security].[UpdateAdGroupStatus](@adGroupId int, @status bit)
AS
BEGIN
	UPDATE SECURITY.ADGroup
	SET IsActive = @status
	WHERE ADGroupId = @adGroupId
END