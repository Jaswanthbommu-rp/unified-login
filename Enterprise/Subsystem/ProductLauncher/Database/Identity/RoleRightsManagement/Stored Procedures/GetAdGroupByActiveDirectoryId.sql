CREATE PROCEDURE [Security].[GetAdGroupByActiveDirectoryId] (  
 @ActiveDirectoryId uniqueidentifier
) 
AS
BEGIN
	SELECT  ADGroupId,
			DisplayName,
			ActiveDirectoryId,
			CreatedBy,
			CreatedDate
	FROM [Security].[ADGroup]
	WHERE ActiveDirectoryId = @ActiveDirectoryId
END
