CREATE PROCEDURE [Enterprise].[SetupSGTRoles](@PartyId BIGINT)  
AS 
BEGIN
	DECLARE @UserId bigint = 0, 	@ProductId int = 0

	SELECT @UserId = UserId FROM Ident.UserLogin WHERE LoginName LIKE 'realpagead@%'  

	SELECT @ProductId = ProductId FROM Enterprise.Product WHERE Name = 'Self-Guided Tour'

	IF (@ProductId > 0 AND @UserId > 0 AND @PartyId > 0)
	BEGIN
		IF NOT EXISTS ( Select top 1 1 from Security.Role where ProductId = @ProductId and OrgPartyID = @PartyId and RoleName = 'Property Manager')
		BEGIN 
			INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
			VALUES ('Property Manager','Property Manager',	'Property Manager',	3,	@PartyId,	@ProductId,	@UserId, GETDATE())
		END

		IF NOT EXISTS ( Select top 1 1 from Security.Role where ProductId = @ProductId and OrgPartyID = @PartyId and RoleName = 'Regional Manager')
		BEGIN 
			INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
			VALUES ('Regional Manager','Regional Manager',	'Regional Manager',	3,	@PartyId,	@ProductId,	@UserId, GETDATE())
		END

		IF NOT EXISTS ( Select top 1 1 from Security.Role where ProductId = @ProductId and OrgPartyID = @PartyId and RoleName = 'Corporate Manager')
		BEGIN 
			INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
			VALUES ('Corporate Manager','Corporate Manager','Corporate Manager',3,	@PartyId,	@ProductId,	@UserId,	GETDATE())
		END
	END
END