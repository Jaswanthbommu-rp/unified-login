CREATE PROCEDURE [Person].[ListTelecommunicationNumbersForPerson] (
	@RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	SELECT  pcm.PartyContactMechanismId,
			cm.ContactMechanismID,
			tm.CountryCode,
			tm.AreaCode,
			tm.PhoneNumber,
			'Telecommunications Number' AS AddressType,
			cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeID,
			Ispreferred = case when cmp.ContactMechanismID is not null then 1 else 0 end,
			tm.ISOCode,
			tm.[Default] as IsDefault
	FROM    Enterprise.ContactMechanismUsage cmu
			JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
			JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
			JOIN Enterprise.TelecommunicationsNumber tm ON tm.ContactMechanismID = cm.ContactMechanismID
			left outer join Enterprise.ContactMechanismPreference cmp on cmp.ContactMechanismID = tm.ContactMechanismID
			JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
	WHERE   p.RealPageId = @RealPageId
	AND		(pcm.ThruDate IS NULL OR pcm.ThruDate > GETUTCDATE());
END