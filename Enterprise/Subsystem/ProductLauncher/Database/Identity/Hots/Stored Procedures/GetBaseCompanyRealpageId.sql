Create Procedure [Hots].[GetBaseCompanyRealpageId] (@RealPageId uniqueidentifier)
AS
BEGIN
	Select BP.RealPageId 
	From [Hots].[CompanyRelationship] CR
	Join Enterprise.Party BP ON
		CR.BaseLineCompanyPartyId = BP.PartyId
	Join Enterprise.Party CP ON
		CR.CloneCompanyPartyId = CP.PartyId
	Where CP.RealPageId = @RealPageId
END
