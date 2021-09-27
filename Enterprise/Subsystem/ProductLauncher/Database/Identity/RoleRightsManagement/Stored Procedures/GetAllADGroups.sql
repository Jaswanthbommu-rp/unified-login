CREATE PROCEDURE [Security].[GetAllADGroups]
AS
BEGIN
 SELECT 
		ADGroupId
		,DisplayName
		,ActiveDirectoryId
 FROM Security.ADGroup
END