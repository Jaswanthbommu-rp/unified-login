CREATE PROCEDURE [Security].[UpdateAdGroup]
	@AdGroupId int,
	@DisplayName nvarchar(255) = NULL,
	@IsActive bit = NULL
AS
BEGIN
	UPDATE [Security].[ADGroup]
	SET DisplayName = ISNULL(@DisplayName, DisplayName),
		IsActive = ISNULL(@IsActive, IsActive)
	WHERE ADGroupId = @AdGroupId
	SELECT @ADGroupId AS Id, '' AS ErrorMessage
END;