CREATE PROCEDURE [Ident].[UpdateUsersIDP]
(
	 @OrganizationPartyId BIGINT
	,@UserIds [Enterprise].[BigIntListType] READONLY
	,@IsEnabled BIT
)
AS
BEGIN
	BEGIN TRY    
		BEGIN TRAN;  
			DECLARE @IDPValue INT;

			IF(@IsEnabled = 1)
			BEGIN
				--Find Company IDP in Enterprise.Organization table 
				SELECT @IDPValue = O.IdentityProviderTypeId
				FROM Enterprise.Organization O 
				WHERE O.PartyId = @OrganizationPartyId
			END
			ELSE
			BEGIN
				DECLARE @OldIDPValue INT;
				SELECT @OldIDPValue = O.IdentityProviderTypeId
				FROM Enterprise.Organization O 
				WHERE O.PartyId = @OrganizationPartyId

				SELECT @IDPValue = ipt.IdentityProviderTypeId FROM enterprise.PartyContactMechanism pcm 
				INNER JOIN ident.IdentityProviderType ipt ON ipt.ContactMechanismId = pcm.ContactMechanismId 
				WHERE pcm.PartyId = @OrganizationPartyId AND ipt.IdentityProviderTypeId <> @OldIDPValue
			END

				UPDATE UL SET UL.IdentityProviderTypeId = @IDPValue
				FROM IDENT.USERLOGIN UL
				JOIN IDENT.USERLOGINPERSONA ULP ON UL.UserId = ULP.UserLoginId 
				JOIN PERSON.PERSONA P ON P.UserLoginPersonaId = ULP.UserLoginPersonaId
				JOIN Enterprise.PartyRelationship  PR ON PR.PartyIdFrom = UL.PersonPartyId AND PR.PartyIdTo = ULP.OrganizationPartyId
				JOIN @UserIds UIDS ON UIDS.Id = UL.UserId
				WHERE ULP.OrganizationPartyId = @OrganizationPartyId 
				AND ULP.PrimaryOrganization = 1 
				AND UL.IdentityProviderTypeId <> @IDPValue
				AND PR.RoleTypeIdFrom <> 405 
				AND PR.RoleTypeIdTo = 205 
				AND PR.ThruDate IS NULL

			SELECT 1 AS BIT;  
		COMMIT;     
	END TRY    
	BEGIN CATCH    
		DECLARE @ErrorLogID INT;    
		EXEC dbo.LogError    
		@ErrorLogID = @ErrorLogID OUTPUT;    
		ROLLBACK;    
	END CATCH; 

END
GO