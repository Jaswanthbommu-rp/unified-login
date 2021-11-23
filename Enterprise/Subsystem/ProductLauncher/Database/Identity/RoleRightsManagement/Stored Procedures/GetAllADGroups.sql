CREATE PROCEDURE [Security].[GetAllADGroups]
AS
BEGIN
 SELECT 
		ADGroupId
		,DisplayName
		,ActiveDirectoryId
 FROM Security.ADGroup
 WHERE Security.ADGroup.IsActive = 1
END