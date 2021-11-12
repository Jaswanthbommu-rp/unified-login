CREATE PROCEDURE [Security].[UpdateAdGroup]
	@AdGroupId int,
	@DisplayName nvarchar(255)
AS
BEGIN
	UPDATE [Security].[ADGroup]
	SET DisplayName = @DisplayName
	WHERE ADGroupId = @AdGroupId
	SELECT @ADGroupId AS Id, '' AS ErrorMessage
END;
