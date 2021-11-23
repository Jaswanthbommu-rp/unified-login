CREATE PROCEDURE [Security].[DeactivateAdGroup] (  
 @ActiveDirectoryId INT
) 
AS
BEGIN
	UPDATE [Security].[ADGroup]
	SET IsActive = 0
	WHERE ADGroupId = @ActiveDirectoryId

	SELECT ADGroupId
	FROM [Security].[ADGroup] ADG
	WHERE ADG.ADGroupId = @ActiveDirectoryId
END
