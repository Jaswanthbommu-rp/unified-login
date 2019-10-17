CREATE PROCEDURE [Person].[GetNotificationEmailForPerson] (
	@LoginName varchar(255),
	@OrgPartyId bigint
)
AS
BEGIN
	SELECT  pcm.PartyContactMechanismId,
			cm.ContactMechanismID,
			ea.ElectronicAddressString AS AddressString,
			ea.ElectronicAddressType AS AddressType,
			cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeID
	FROM    
		Ident.UserLogin ul 
		JOIN Ident.UserLoginPersona ulp on ul.UserId = ulp.UserLoginId
		JOIN Enterprise.PartyRelationship pr ON pr.PartyIdFrom = ul.PersonPartyId and pr.PartyIdTo = ulp.OrganizationPartyId
		JOIN Enterprise.PartyContactMechanism pcm on pcm.PartyId = ul.PersonPartyId
		JOIN Enterprise.ContactMechanismUsage cmu ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
		JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
		JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID
	WHERE	ul.LoginName = @LoginName
	AND     pr.RoleTypeIdFrom = 404
	AND     ulp.OrganizationPartyId = @OrgPartyId
	AND		(pcm.ThruDate IS NULL OR pcm.ThruDate > GETUTCDATE())
END