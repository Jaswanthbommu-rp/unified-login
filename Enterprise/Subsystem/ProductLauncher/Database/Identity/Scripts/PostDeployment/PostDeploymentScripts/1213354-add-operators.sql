IF NOT EXISTS(SELECT * FROM Enterprise.ThirdPartyRelationship WHERE ThirdPartyRelationship = 'Employee')
BEGIN
    INSERT INTO Enterprise.ThirdPartyRelationship(ThirdPartyRelationshipId,ThirdPartyRelationship)
    VALUES(4,'Employee')
END
IF NOT EXISTS(SELECT * FROM Enterprise.ThirdPartyRelationship WHERE ThirdPartyRelationship = 'Other')
BEGIN
    INSERT INTO Enterprise.ThirdPartyRelationship(ThirdPartyRelationshipId,ThirdPartyRelationship)
    VALUES(5,'Other')
END