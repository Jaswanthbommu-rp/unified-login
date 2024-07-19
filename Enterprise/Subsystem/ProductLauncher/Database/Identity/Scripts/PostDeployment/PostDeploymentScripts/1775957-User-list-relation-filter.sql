UPDATE EUR
SET EUR.ThirdPartyRelationshipId = ETR.ThirdPartyRelationshipId
FROM Enterprise.ThirdPartyRelationship ETR
INNER JOIN Enterprise.UserRelationShip EUR ON EUR.UserRelationshipName = ETR.ThirdPartyRelationship

UPDATE Enterprise.RoleTypeDependency SET SortOrder =10 WHERE ChildRoleTypeId = 403
UPDATE Enterprise.RoleTypeDependency SET SortOrder =20 WHERE ChildRoleTypeId = 401
UPDATE Enterprise.RoleTypeDependency SET SortOrder =30 WHERE ChildRoleTypeId = 402
UPDATE Enterprise.RoleTypeDependency SET SortOrder =40 WHERE ChildRoleTypeId = 405
UPDATE Enterprise.RoleTypeDependency SET SortOrder =50 WHERE ChildRoleTypeId = 404
