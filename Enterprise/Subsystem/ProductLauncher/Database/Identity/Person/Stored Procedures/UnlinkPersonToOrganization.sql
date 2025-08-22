CREATE PROCEDURE Person.UnlinkPersonToOrganization (
 @PersonRealPageId uniqueidentifier,
 @OrganizationRealPageId uniqueidentifier,
 @RoleTypeIdFrom int = NULL,
 @RoleTypeIdTo int = NULL
)
AS
BEGIN
	DECLARE @PartyIdFrom bigint,
		@PartyIdTo bigint,
		@PartyRelationshipTypeId int,
		@Now datetime = GETUTCDATE();

	IF ((@RoleTypeIdFrom IS NOT NULL) AND (@RoleTypeIdTo IS NOT NULL))
	BEGIN
		SELECT	@PartyRelationshipTypeId = [RelationshipTypeId]
		FROM		Enterprise.[RelationshipType]
		WHERE	RoleTypeIdValidFrom = @RoleTypeIdFrom
		AND			RoleTypeIdValidTo = @RoleTypeIdTo;
	END

	-- Get Party ID's
	SELECT	@PartyIdFrom = p.PartyId
	FROM		Enterprise.Party p
	WHERE	p.RealPageId = @PersonRealPageId;

	SELECT	@PartyIdTo = p.PartyId
	FROM		Enterprise.Party p
	WHERE	p.RealPageId = @OrganizationRealPageId;

	IF ((@PartyRelationshipTypeId IS NULL) AND (@RoleTypeIdFrom IS NOT NULL) AND (@RoleTypeIdTo IS NOT NULL))
	BEGIN
		RAISERROR('The Relationship is invalid between Role Type %i and Role Type %i', 16, -1, @RoleTypeIdFrom, @RoleTypeIdTo);
	END;

	BEGIN TRY
		BEGIN TRANSACTION;

		DELETE FROM	Enterprise.PartyRelationship
		WHERE	PartyIdFrom = @PartyIdFrom AND PartyIdTo = @PartyIdTo
		AND			((@RoleTypeIdFrom IS NULL) OR (RoleTypeIdFrom = @RoleTypeIdFrom))
		AND			((@RoleTypeIdTo IS NULL) OR (RoleTypeIdTo = @RoleTypeIdTo))
		AND			ThruDate IS NULL

		DELETE PR
		FROM Security.PersonaRole PR
			INNER JOIN Person.Persona P ON PR.PersonaId = P.PersonaId
			INNER JOIN Ident.UserLoginPersona ULP ON P.UserLoginPersonaId = ULP.UserLoginPersonaId
			INNER JOIN Ident.UserLogin UL ON ULP.UserLoginId = UL.UserId
			WHERE
				UL.PersonPartyId = @PartyIdFrom
				AND
				ULP.OrganizationPartyId = @PartyIdTo
		
		DELETE RTM
		FROM Security.RoleTemplateUserMapping RTM
		INNER JOIN Person.Persona P ON RTM.PersonaId = P.PersonaId
		INNER JOIN Ident.UserLoginPersona ULP ON P.UserLoginPersonaId = ULP.UserLoginPersonaId
		INNER JOIN Ident.UserLogin UL ON ULP.UserLoginId = UL.UserId
		WHERE
		UL.PersonPartyId = @PartyIdFrom
		AND
		ULP.OrganizationPartyId = @PartyIdTo

	   DELETE PM
	   FROM Enterprise.PropertyInstanceMapping PM
	   INNER JOIN Person.Persona P  ON PM.PersonaId = P.PersonaId
	   INNER JOIN Ident.UserLoginPersona ULP ON P.UserLoginPersonaId = ULP.UserLoginPersonaId  
	   INNER JOIN Ident.UserLogin UL ON ULP.UserLoginId = UL.UserId  
	   WHERE  
		UL.PersonPartyId = @PartyIdFrom  
		AND  
		ULP.OrganizationPartyId = @PartyIdTo 

		DELETE P
		FROM Person.Persona P
			INNER JOIN Ident.UserLoginPersona ULP ON P.UserLoginPersonaId = ULP.UserLoginPersonaId
			INNER JOIN Ident.UserLogin UL ON ULP.UserLoginId = UL.UserId
			WHERE
				UL.PersonPartyId = @PartyIdFrom
				AND
				ULP.OrganizationPartyId = @PartyIdTo
		
		DELETE UE
		FROM Enterprise.UserEmployeeId UE 
		   INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = UE.UserLoginPersonaId
		   INNER JOIN Ident.UserLogin UL ON ULP.UserLoginId = UL.UserId 
		   WHERE  
				UL.PersonPartyId = @PartyIdFrom
				AND
				ULP.OrganizationPartyId = @PartyIdTo

		DELETE ULP
		FROM Ident.UserLoginPersona ULP
			INNER JOIN Ident.UserLogin UL ON ULP.UserLoginId = UL.UserId
			WHERE
				UL.PersonPartyId = @PartyIdFrom
				AND
				ULP.OrganizationPartyId = @PartyIdTo
		
		SELECT @PartyIdFrom AS Id

		COMMIT;
	END TRY
	BEGIN CATCH
		ROLLBACK;
		DECLARE @ErrorLogID INT;

		EXEC dbo.LogError
			@ErrorLogID = @ErrorLogID OUTPUT;

		SELECT	0 AS Id,
						ErrorMessage
		FROM		dbo.ErrorLog
		WHERE	ErrorLogID = @ErrorLogID;
	END CATCH;
END;