CREATE PROCEDURE Person.ListEmailsForPerson (
	@RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	SELECT  pcm.PartyContactMechanismId,
			cm.ContactMechanismID,
			ea.ElectronicAddressString AS AddressString,
			ea.ElectronicAddressType AS AddressType,
			cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeID
	FROM    Enterprise.ContactMechanismUsage cmu
			JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
			JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
			JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID
			JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
	WHERE	p.RealPageId = @RealPageId
	AND		(pcm.ThruDate IS NULL OR pcm.ThruDate > GETUTCDATE());
END