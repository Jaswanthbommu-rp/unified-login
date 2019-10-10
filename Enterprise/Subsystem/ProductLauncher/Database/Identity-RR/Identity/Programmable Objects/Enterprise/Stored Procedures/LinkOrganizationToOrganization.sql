IF OBJECT_ID('[Enterprise].[LinkOrganizationToOrganization]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[LinkOrganizationToOrganization];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[LinkOrganizationToOrganization]
    @OrganizationRealPageIdFrom UNIQUEIDENTIFIER ,
    @OrganizationRealPageIdTo UNIQUEIDENTIFIER ,
    @RoleTypeIdFrom INT ,
    @RoleTypeIdTo INT
AS
BEGIN
	BEGIN TRY

		DECLARE @PartyIdFrom BIGINT;
		DECLARE @PartyIdTo BIGINT;

		DECLARE @PartyRelationshipTypeId INT;

		SELECT	@PartyRelationshipTypeId = [RelationshipTypeId]
		FROM	Enterprise.[RelationshipType]
		WHERE	RoleTypeIdValidFrom = @RoleTypeIdFrom
		AND		RoleTypeIdValidTo = @RoleTypeIdTo;

		-- Get Party ID's
		SELECT	@PartyIdFrom = p.PartyId
		FROM	Enterprise.Party p
		WHERE	p.RealPageId = @OrganizationRealPageIdFrom

		SELECT	@PartyIdTo = p.PartyId
		FROM	Enterprise.Party p
		WHERE	p.RealPageId = @OrganizationRealPageIdTo

		IF @PartyRelationshipTypeId IS NULL
		BEGIN
			RAISERROR('The Relationship is invalid between Role Type %i and Role Type %i', 16, -1, @RoleTypeIdFrom, @RoleTypeIdTo);
		END;

        BEGIN TRANSACTION; 

		INSERT INTO Enterprise.PartyRelationship (
			PartyIdFrom,
			PartyIdTo,
			RoleTypeIdFrom,
			RoleTypeIdTo,
			PartyRelationshipTypeId,
			FromDate,
			ThruDate
		)
		OUTPUT	Inserted.PartyRelationshipId AS Id,
				'' AS ErrorMessage
		VALUES (
			@PartyIdFrom , -- PartyIdFrom - bigint
			@PartyIdTo , -- PartyIdTo - bigint
			@RoleTypeIdFrom , -- RoleTypeIdFrom - int
			@RoleTypeIdTo , -- RoleTypeIdTo - int
			@PartyRelationshipTypeId , -- PartyRelationshipTypeId - int
			GETUTCDATE() , -- FromDate - datetime
			NULL  -- ThruDate - datetime
		)

        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT  0 AS Id,
				ErrorMessage
        FROM    dbo.ErrorLog
        WHERE   ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
