IF OBJECT_ID('[Person].[ListPostalAddressesForPerson]') IS NOT NULL
	DROP PROCEDURE [Person].[ListPostalAddressesForPerson];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Person].[ListPostalAddressesForPerson] (
	@RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	SELECT	unpvt.PartyContactMechanismId,
			unpvt.ContactMechanismID,
			unpvt.AddressString,
			unpvt.AddressType,
			unpvt.ContactMechanismUsageTypeID
	FROM    (
				SELECT	pcm.PartyContactMechanismId,
						cm.ContactMechanismID,
						pa.StreetAddress1,
						pa.StreetAddress2,
						pa.StreetAddress3,
						cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeID
				FROM	Enterprise.Party p
						JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyId = p.PartyId AND p.RealPageId = @RealPageId
						JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
						JOIN Enterprise.[StreetAddress] pa ON pa.ContactMechanismID = cm.ContactMechanismID
						JOIN Enterprise.ContactMechanismUsage cmu ON cmu.PartyContactMechanismID = pcm.PartyContactMechanismId
				WHERE	(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE())
			) AS pvt UNPIVOT
			(AddressString FOR AddressType IN (StreetAddress1, StreetAddress2, StreetAddress3)) AS unpvt
	UNION ALL
	SELECT	pcm.PartyContactMechanismId,
			cm.ContactMechanismID ,
			gb.Name AS AddressString ,
			gbt.Name AS AddressType ,
			cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeID
	FROM	Enterprise.ContactMechanismUsage cmu
			JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
			JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
			JOIN Enterprise.ContactMechanismBoundary cmb ON cmb.ContactMechanismId = cm.ContactMechanismID
			JOIN Enterprise.GeographicBoundary gb ON gb.GeographicBoundaryId = cmb.GeographicBoundaryId
			JOIN Enterprise.GeographicBoundaryType gbt ON gbt.GeographicBoundaryTypeId = gb.GeographicBoundaryTypeId
			JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
	WHERE	p.RealPageId = @RealPageId
	AND		(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE());
END;
GO
