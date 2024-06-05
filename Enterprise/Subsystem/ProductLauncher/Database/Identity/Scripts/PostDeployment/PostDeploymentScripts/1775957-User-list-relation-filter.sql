UPDATE EUR
SET EUR.ThirdPartyRelationshipId = ETR.ThirdPartyRelationshipId
FROM Enterprise.ThirdPartyRelationship ETR
INNER JOIN Enterprise.UserRelationShip EUR ON EUR.UserRelationshipName = ETR.ThirdPartyRelationship