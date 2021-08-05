CREATE PROCEDURE [Enterprise].[UpdateOrganizationIdentityProvider](
	 @PartyId int
	 ,@NewIdentityProviderTypeId INT
)
AS

BEGIN
	SET NOCOUNT ON;
	IF NOT EXISTS ( SELECT TOP (1) 1 FROM Ident.IdentityProviderType WHERE IdentityProviderTypeId = @NewIdentityProviderTypeId )
	BEGIN
		SELECT @PartyId
		RETURN 0
	END

	DECLARE @NOW datetime = GETUTCDATE(),
		@CurrentIDP INT,
		@NewIDPIsPassword TINYINT = 0,
		@NewContactMechanismId INT

	SELECT @NewIDPIsPassword = CASE WHEN @NewIdentityProviderTypeId = IdentityProviderTypeId THEN 1 ELSE 0 END FROM Ident.IdentityProviderType WHERE Name = 'ID3'
	SELECT @NewContactMechanismId = ContactMechanismId  FROM Ident.IdentityProviderType WHERE IdentityProviderTypeId = @NewIdentityProviderTypeId

	SELECT @CurrentIDP = IdentityProviderTypeId FROM Enterprise.Organization WHERE PartyId = @PartyId

	IF @CurrentIDP = @NewIdentityProviderTypeId
	BEGIN
		SELECT @PartyId
		RETURN 0
	END

	IF @NewIDPIsPassword = 1 AND 2 = ( SELECT COUNT(1) FROM Enterprise.PartyContactMechanism PC 
				INNER JOIN Ident.IdentityProviderType IPT ON IPT.ContactMechanismId = PC.ContactMechanismId 
				WHERE PC.PartyId = @PartyId AND PC.ThruDate IS NULL )
	BEGIN
		-- NEED TO REMOVE THE SECONDARY IDP SO PASSWORD IS THE ONLY ACTIVE IDP
		UPDATE PCM SET PCM.ThruDate = @NOW
		FROM Enterprise.PartyContactMechanism  PCM
			INNER JOIN Ident.IdentityProviderType IPT ON IPT.ContactMechanismId = PCM.ContactMechanismId 
				WHERE PCM.PartyId = @PartyId AND PCM.ThruDate IS NULL AND IPT.Name <> 'ID3'

		UPDATE Enterprise.Organization SET IdentityProviderTypeId = @NewIdentityProviderTypeId WHERE PartyId = @PartyId
		SELECT @PartyId
		RETURN 0
	END

	IF @NewIDPIsPassword <> 1 AND 2 = ( SELECT COUNT(1) FROM Enterprise.PartyContactMechanism PC 
				INNER JOIN Ident.IdentityProviderType IPT ON IPT.ContactMechanismId = PC.ContactMechanismId 
				WHERE PC.PartyId = @PartyId AND PC.ThruDate IS NULL )
	BEGIN
		IF @NewContactMechanismId <> ( SELECT PC.ContactMechanismId FROM Enterprise.PartyContactMechanism PC 
				INNER JOIN Ident.IdentityProviderType IPT ON IPT.ContactMechanismId = PC.ContactMechanismId 
				WHERE PC.PartyId = @PartyId AND PC.ThruDate IS NULL AND IPT.Name <> 'ID3' )
		BEGIN
			UPDATE PCM SET PCM.ThruDate = @NOW
				FROM Enterprise.PartyContactMechanism  PCM
				INNER JOIN Ident.IdentityProviderType IPT ON IPT.ContactMechanismId = PCM.ContactMechanismId 
					WHERE PCM.PartyId = @PartyId AND PCM.ThruDate IS NULL AND IPT.Name <> 'ID3'
			INSERT INTO Enterprise.PartyContactMechanism ( PartyId, ContactMechanismId, FromDate, ThruDate )
				VALUES ( @PartyId, @NewContactMechanismId, @NOW, NULL )
				
			UPDATE Enterprise.Organization SET IdentityProviderTypeId = @NewIdentityProviderTypeId WHERE PartyId = @PartyId
			SELECT @PartyId
			RETURN 0
		END
	END
	IF @NewIDPIsPassword <> 1 AND 1 = ( SELECT COUNT(1) FROM Enterprise.PartyContactMechanism PC 
				INNER JOIN Ident.IdentityProviderType IPT ON IPT.ContactMechanismId = PC.ContactMechanismId 
				WHERE PC.PartyId = @PartyId AND PC.ThruDate IS NULL )
	BEGIN
		INSERT INTO Enterprise.PartyContactMechanism ( PartyId, ContactMechanismId, FromDate, ThruDate )
			VALUES ( @PartyId, @NewContactMechanismId, @NOW, NULL )
				
		UPDATE Enterprise.Organization SET IdentityProviderTypeId = @NewIdentityProviderTypeId WHERE PartyId = @PartyId
		SELECT @PartyId
		RETURN 0
	END

END;
