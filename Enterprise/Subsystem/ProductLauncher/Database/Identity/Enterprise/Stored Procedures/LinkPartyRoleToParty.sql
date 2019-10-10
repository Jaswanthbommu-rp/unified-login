CREATE PROCEDURE [Enterprise].[LinkPartyRoleToParty]
	@RealPageId UNIQUEIDENTIFIER,
	@PartyRoleId int
AS
BEGIN
    INSERT INTO Enterprise.PartyRole
            ( PartyId, RoleTypeId )
	OUTPUT Inserted.PartyRoleId AS Id, '' AS ErrorMessage
    SELECT p.PartyId, @PartyRoleId
	FROM Enterprise.Party p 
	WHERE p.RealPageId = @RealPageId
END
