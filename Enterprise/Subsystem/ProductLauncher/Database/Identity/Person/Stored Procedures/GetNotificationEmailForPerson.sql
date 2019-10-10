USE [Identity]
GO
/****** Object:  StoredProcedure [Person].[GetNotificationEmailForPerson]    Script Date: 9/21/2019 10:21:23 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
Create PROCEDURE [Person].[GetNotificationEmailForPerson] (
	@LoginName varchar(100),
	@OrgPartyId bigint
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
			JOIN Enterprise.PartyRelationship pr ON pr.PartyIdFrom = p.PartyId
			Join Person.Persona pe ON pe.PersonPartyId = p.PartyId
			Join Ident.UserLogin ul ON ul.UserId = pe.UserId
	WHERE	ul.LoginName = @LoginName
	AND     pr.RoleTypeIdFrom = 404
	AND     pe.OrganizationPartyId = @OrgPartyId
	AND		(pcm.ThruDate IS NULL OR pcm.ThruDate > GETUTCDATE())
END