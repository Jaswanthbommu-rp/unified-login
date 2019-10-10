IF OBJECT_ID('[Person].[ListContactMechanismsForPerson]') IS NOT NULL
	DROP PROCEDURE [Person].[ListContactMechanismsForPerson];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Person].[ListContactMechanismsForPerson] (
    @RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	DECLARE @NOW  DATETIME = GETUTCDATE();

    SELECT  pcm.PartyContactMechanismId,
			cm.ContactMechanismID,
            ea.ElectronicAddressString AS AddressString,
            'Email' AS AddressType,
			cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId
    FROM    Enterprise.ContactMechanismUsage cmu
            JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
            JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
            JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID
            JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
    WHERE   p.RealPageId = @RealPageId
	AND ((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL))
    UNION ALL
    SELECT  pcm.PartyContactMechanismId,
			cm.ContactMechanismID,
            pa.StreetAddress1 AS AddressString,
            'Street Address' AS AddressType,
			cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId
    FROM    Enterprise.ContactMechanismUsage cmu
            JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
            JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
            JOIN Enterprise.[StreetAddress] pa ON pa.ContactMechanismID = cm.ContactMechanismID
            JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
    WHERE   p.RealPageId = @RealPageId
	AND ((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL))
    UNION ALL
    SELECT  pcm.PartyContactMechanismId,
			cm.ContactMechanismID,
            gb.Name AS AddressString,
            gbt.Name AS AddressType,
			cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId
    FROM    Enterprise.ContactMechanismUsage cmu
            JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
            JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
            JOIN Enterprise.ContactMechanismBoundary cmb ON cmb.ContactMechanismId = cm.ContactMechanismID
            JOIN Enterprise.GeographicBoundary gb ON gb.GeographicBoundaryId = cmb.GeographicBoundaryId
            JOIN Enterprise.GeographicBoundaryType gbt ON gbt.GeographicBoundaryTypeId = gb.GeographicBoundaryTypeId
            JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
    WHERE   p.RealPageId = @RealPageId
	AND ((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL))
	AND ((@NOW BETWEEN cmb.FromDate AND cmb.ThruDate) OR (@NOW >= cmb.FromDate AND cmb.ThruDate IS NULL))
    UNION ALL
    SELECT  pcm.PartyContactMechanismId,
			cm.ContactMechanismID,
            CONCAT(tm.CountryCode, tm.AreaCode, tm.PhoneNumber) AS AddressString,
            'Telecommunications Number' AS AddressType,
			cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId
    FROM    Enterprise.ContactMechanismUsage cmu
            JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
            JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
            JOIN Enterprise.TelecommunicationsNumber tm ON tm.ContactMechanismID = cm.ContactMechanismID
            JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
    WHERE   p.RealPageId = @RealPageId
	AND ((@NOW BETWEEN pcm.FromDate AND pcm.ThruDate) OR (@NOW >= pcm.FromDate AND pcm.ThruDate IS NULL));
END;
GO
