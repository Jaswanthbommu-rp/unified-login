CREATE PROCEDURE Person.UpdatePersonToOrganization (
	@PersonRealPageId UNIQUEIDENTIFIER,
	@OrganizationRealPageId UNIQUEIDENTIFIER,
	@UnlinkRoleTypeIdFrom INT,
	@LinkRoleTypeIdFrom INT,
	@RoleTypeIdTo INT
)
AS
BEGIN
	BEGIN TRY
		DECLARE @PartyIdFrom BIGINT;
		DECLARE @PartyIdTo BIGINT;
		DECLARE @PartyRelationshipTypeId INT;
		DECLARE @Now datetime = GETUTCDATE();


		SELECT	@PartyRelationshipTypeId = [RelationshipTypeId]
		FROM		Enterprise.[RelationshipType]
		WHERE	RoleTypeIdValidFrom = @UnlinkRoleTypeIdFrom
		AND			RoleTypeIdValidTo = @RoleTypeIdTo;

		-- Get Party ID's
		SELECT	@PartyIdFrom = p.PartyId
		FROM		Enterprise.Party p
		WHERE	p.RealPageId = @PersonRealPageId;

		SELECT	@PartyIdTo = p.PartyId
		FROM		Enterprise.Party p
		WHERE	p.RealPageId = @OrganizationRealPageId;

		BEGIN TRANSACTION;
			IF @PartyRelationshipTypeId IS NOT NULL
			BEGIN
				IF ((@UnlinkRoleTypeIdFrom IS NOT NULL) AND (@RoleTypeIdTo IS NOT NULL))
				BEGIN
					SELECT	@PartyRelationshipTypeId = [RelationshipTypeId]
					FROM		Enterprise.[RelationshipType]
					WHERE	RoleTypeIdValidFrom = @UnlinkRoleTypeIdFrom
					AND			RoleTypeIdValidTo = @RoleTypeIdTo;
				END

				IF ((@PartyRelationshipTypeId IS NULL) AND (@UnlinkRoleTypeIdFrom IS NOT NULL) AND (@RoleTypeIdTo IS NOT NULL))
				BEGIN
					RAISERROR('The Relationship is invalid between Role Type %i and Role Type %i', 16, -1, @UnlinkRoleTypeIdFrom, @RoleTypeIdTo);
				END;

				UPDATE	Enterprise.PartyRelationship
				SET			ThruDate = @Now
				OUTPUT Inserted.PartyRelationshipId AS Id,
								'' AS ErrorMessage
				WHERE	PartyIdFrom = @PartyIdFrom AND PartyIdTo = @PartyIdTo
				AND			((@UnlinkRoleTypeIdFrom IS NULL) OR (RoleTypeIdFrom = @UnlinkRoleTypeIdFrom))
				AND			((@RoleTypeIdTo IS NULL) OR (RoleTypeIdTo = @RoleTypeIdTo))
				AND			ThruDate IS NULL
			END;

			EXEC Person.LinkPersonToOrganization
				@PersonRealPageId,
				@OrganizationRealPageId,
				@LinkRoleTypeIdFrom,
				@RoleTypeIdTo
		COMMIT;
	END TRY
	BEGIN CATCH
		ROLLBACK;
		DECLARE @ErrorLogID INT;

		EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

		SELECT	0 AS Id,
						ErrorMessage
		FROM		dbo.ErrorLog
		WHERE	ErrorLogID = @ErrorLogID;
	END CATCH;
END;  