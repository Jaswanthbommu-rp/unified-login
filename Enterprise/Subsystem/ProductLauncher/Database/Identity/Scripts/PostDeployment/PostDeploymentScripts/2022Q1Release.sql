GO
IF NOT EXISTS ( SELECT TOP (1) (1) FROM Enterprise.ThirdPartyRelationship )
BEGIN
	INSERT INTO [Enterprise].[ThirdPartyRelationship] (ThirdPartyRelationshipId, ThirdPartyRelationship )
		VALUES (1, 'Operator'),(2,'Owner'),(3,'Third Party Vendor')

END
