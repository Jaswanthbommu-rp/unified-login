CREATE PROCEDURE [Person].[ListContactMechanismsForPerson] (
	@RealPageId uniqueidentifier
)
AS
BEGIN
	DECLARE @NOW  DATETIME = GETUTCDATE();

	SELECT	pcm.PartyContactMechanismId,
					pcm.ContactMechanismID,
					ea.ElectronicAddressString AS AddressString,
					'Email' AS AddressType,
					cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId
    FROM		Enterprise.ContactMechanismUsage cmu
					INNER JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
					INNER JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = pcm.ContactMechanismID
					INNER JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
	WHERE	p.RealPageId = @RealPageId
	AND			((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL))
	UNION ALL
	SELECT	pcm.PartyContactMechanismId,
					pcm.ContactMechanismID,
					pa.StreetAddress1 AS AddressString,
					'Street Address' AS AddressType,
					cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId
	FROM		Enterprise.ContactMechanismUsage cmu
					INNER JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
					INNER JOIN Enterprise.[StreetAddress] pa ON pa.ContactMechanismID = pcm.ContactMechanismID
					INNER JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
	WHERE	p.RealPageId = @RealPageId
	AND			((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL))
	UNION ALL
	SELECT	pcm.PartyContactMechanismId,
					pcm.ContactMechanismID,
					gb.Name AS AddressString,
					gbt.Name AS AddressType,
					cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId
	FROM		Enterprise.ContactMechanismUsage cmu
					INNER JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
					INNER JOIN Enterprise.ContactMechanismBoundary cmb ON cmb.ContactMechanismId = pcm.ContactMechanismID
					INNER JOIN Enterprise.GeographicBoundary gb ON gb.GeographicBoundaryId = cmb.GeographicBoundaryId
					INNER JOIN Enterprise.GeographicBoundaryType gbt ON gbt.GeographicBoundaryTypeId = gb.GeographicBoundaryTypeId
					INNER JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
	WHERE	p.RealPageId = @RealPageId
	AND			((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL))
	AND			((@NOW BETWEEN cmb.FromDate AND cmb.ThruDate) OR (@NOW >= cmb.FromDate AND cmb.ThruDate IS NULL))
	UNION ALL
	SELECT	pcm.PartyContactMechanismId,
					pcm.ContactMechanismID,
					CONCAT(tm.CountryCode, tm.AreaCode, tm.PhoneNumber) AS AddressString,
					'Telecommunications Number' AS AddressType,
					cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId
	FROM		Enterprise.ContactMechanismUsage cmu
					INNER JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
					INNER JOIN Enterprise.TelecommunicationsNumber tm ON tm.ContactMechanismID = pcm.ContactMechanismID
					INNER JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
	WHERE	p.RealPageId = @RealPageId
	AND			((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL));
END;