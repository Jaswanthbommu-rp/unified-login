CREATE PROCEDURE [Security].[AddAdGroup]
	@DisplayName nvarchar(255),
	@ActiveDirectoryId uniqueidentifier,
	@CreatedBy nvarchar(25)
AS
BEGIN
	DECLARE @CreatedDate datetime = GETUTCDATE(),
			@ADGroupId int
	INSERT INTO [Security].[ADGroup]
		(DisplayName,
		ActiveDirectoryId,
		CreatedBy,
		CreatedDate
		)
	VALUES
		(@DisplayName,
		@ActiveDirectoryId,
		@CreatedBy,
		@CreatedDate
		);
	SELECT @ADGroupId = SCOPE_IDENTITY();
	SELECT @ADGroupId AS Id, '' AS ErrorMessage
END;
