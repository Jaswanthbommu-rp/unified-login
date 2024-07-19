UPDATE EUR
SET EUR.ThirdPartyRelationshipId = ETR.ThirdPartyRelationshipId
FROM Enterprise.ThirdPartyRelationship ETR
INNER JOIN Enterprise.UserRelationShip EUR ON EUR.UserRelationshipName = ETR.ThirdPartyRelationship

UPDATE Enterprise.RoleTypeDependency SET SortOrder =10 WHERE ChildRoleTypeId = 403
UPDATE Enterprise.RoleTypeDependency SET SortOrder =20 WHERE ChildRoleTypeId = 401
UPDATE Enterprise.RoleTypeDependency SET SortOrder =30 WHERE ChildRoleTypeId = 402
UPDATE Enterprise.RoleTypeDependency SET SortOrder =40 WHERE ChildRoleTypeId = 405
UPDATE Enterprise.RoleTypeDependency SET SortOrder =50 WHERE ChildRoleTypeId = 404



DECLARE @Id1 INT, @SortIndex1 INT, @Id2 INT, @SortIndex2 INT

SELECT @Id1 = Id, @SortIndex1 = SortIndex  FROM [Enterprise].[UserRelationShip] WHERE UserRelationshipName = 'Third Party Vendor (External User)'
SELECT @Id2 = Id, @SortIndex2 = SortIndex  FROM [Enterprise].[UserRelationShip] WHERE UserRelationshipName = 'Other (external user)'

UPDATE Enterprise.UserRelationShip SET SortIndex = @SortIndex2 WHERE Id=@Id1
UPDATE Enterprise.UserRelationShip SET SortIndex = @SortIndex1 WHERE Id=@Id2
