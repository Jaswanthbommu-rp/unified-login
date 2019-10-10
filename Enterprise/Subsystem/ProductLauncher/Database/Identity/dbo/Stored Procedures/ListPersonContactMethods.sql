CREATE PROC ListPersonContactMethods
@PersonId UNIQUEIDENTIFIER 
AS

SELECT  cm.ContactMechanismID ,
        ea.ElectronicAddressString AS AddressString ,
        'Email' AS AddressType ,
        cmt.Name AS UsageType
FROM    Enterprise.ContactMechanismUsage cmu
        JOIN Enterprise.ContactMechanismUsageType cmt ON cmt.ContactMechanismUsageTypeID = cmu.ContactMechanismUsageTypeID
        JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
        JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
        JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID
		JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
WHERE   p.RealPageId = @PersonId
UNION ALL
SELECT  cm.ContactMechanismID ,
        pa.StreetAddress1 AS AddressString ,
        'Street Address' AS AddressType ,
        cmt.Name AS UsageType
FROM    Enterprise.ContactMechanismUsage cmu
        JOIN Enterprise.ContactMechanismUsageType cmt ON cmt.ContactMechanismUsageTypeID = cmu.ContactMechanismUsageTypeID
        JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
        JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
        JOIN Enterprise.[StreetAddress] pa ON pa.ContactMechanismID = cm.ContactMechanismID
		JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
WHERE   p.RealPageId = @PersonId
UNION ALL
SELECT  cm.ContactMechanismID ,
        gb.Name AS AddressString ,
        gbt.Name AS AddressType ,
        cmt.Name AS UsageType
FROM    Enterprise.ContactMechanismUsage cmu
        JOIN Enterprise.ContactMechanismUsageType cmt ON cmt.ContactMechanismUsageTypeID = cmu.ContactMechanismUsageTypeID
        JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
        JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
        JOIN Enterprise.ContactMechanismBoundary cmb ON cmb.ContactMechanismId = cm.ContactMechanismID
        JOIN Enterprise.GeographicBoundary gb ON gb.GeographicBoundaryId = cmb.GeographicBoundaryId
        JOIN Enterprise.GeographicBoundaryType gbt ON gbt.GeographicBoundaryTypeId = gb.GeographicBoundaryTypeId
		JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
WHERE   p.RealPageId = @PersonId
UNION ALL
SELECT  cm.ContactMechanismID ,
        CONCAT(tm.CountryCode, tm.AreaCode, tm.PhoneNumber) AS AddressString ,
        'Telecommunications Number' AS AddressType ,
        cmt.Name AS UsageType
FROM    Enterprise.ContactMechanismUsage cmu
        JOIN Enterprise.ContactMechanismUsageType cmt ON cmt.ContactMechanismUsageTypeID = cmu.ContactMechanismUsageTypeID
        JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
        JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
        JOIN Enterprise.TelecommunicationsNumber tm ON tm.ContactMechanismID = cm.ContactMechanismID
		JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
WHERE   p.RealPageId = @PersonId;