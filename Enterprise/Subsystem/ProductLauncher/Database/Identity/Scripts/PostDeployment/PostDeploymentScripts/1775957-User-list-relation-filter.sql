UPDATE EUR
SET EUR.ThirdPartyRelationshipId = ETR.ThirdPartyRelationshipId
FROM Enterprise.ThirdPartyRelationship ETR
INNER JOIN Enterprise.UserRelationShip EUR ON EUR.UserRelationshipName = ETR.ThirdPartyRelationship

UPDATE Enterprise.RoleTypeDependency SET SortOrder =10 WHERE ChildRoleTypeId = 403
UPDATE Enterprise.RoleTypeDependency SET SortOrder =20 WHERE ChildRoleTypeId = 401
UPDATE Enterprise.RoleTypeDependency SET SortOrder =30 WHERE ChildRoleTypeId = 402
UPDATE Enterprise.RoleTypeDependency SET SortOrder =40 WHERE ChildRoleTypeId = 405
UPDATE Enterprise.RoleTypeDependency SET SortOrder =50 WHERE ChildRoleTypeId = 404


UPDATE Enterprise.UserRelationShip SET SortIndex = 30 WHERE UserRelationshipName = 'Employee (Additional Company)' AND  PartyRoleTypeId=405
UPDATE Enterprise.UserRelationShip SET SortIndex = 50 WHERE UserRelationshipName = 'Operator (External User)' AND  PartyRoleTypeId=405
UPDATE Enterprise.UserRelationShip SET SortIndex = 60 WHERE UserRelationshipName = 'Other (External User)' AND  PartyRoleTypeId=405
UPDATE Enterprise.UserRelationShip SET SortIndex = 70 WHERE UserRelationshipName = 'Owner (External User)' AND  PartyRoleTypeId=405
UPDATE Enterprise.UserRelationShip SET SortIndex = 80 WHERE UserRelationshipName = 'Third Party Vendor (External User)' AND  PartyRoleTypeId=405