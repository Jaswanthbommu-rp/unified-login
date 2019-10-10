IF OBJECT_ID('[Person].[UnlinkPersonToOrganization]') IS NOT NULL
	DROP PROCEDURE [Person].[UnlinkPersonToOrganization];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Person].[UnlinkPersonToOrganization] (
	@PersonRealPageId UNIQUEIDENTIFIER,
	@OrganizationRealPageId UNIQUEIDENTIFIER,
	@RoleTypeIdFrom INT,
	@RoleTypeIdTo INT
)
AS
BEGIN
	BEGIN TRY
        DECLARE @PartyIdFrom BIGINT;
        DECLARE @PartyIdTo BIGINT;
		DECLARE @PartyRelationshipTypeId INT;

		SELECT  @PartyRelationshipTypeId = [RelationshipTypeId]
		FROM    Enterprise.[RelationshipType]
		WHERE   RoleTypeIdValidFrom = @RoleTypeIdFrom
		AND		RoleTypeIdValidTo = @RoleTypeIdTo;

		-- Get Party ID's
        SELECT  @PartyIdFrom = p.PartyId
        FROM    Enterprise.Party p
        WHERE   p.RealPageId = @PersonRealPageId;
		print @PartyIdFrom
        SELECT  @PartyIdTo = p.PartyId
        FROM    Enterprise.Party p
        WHERE   p.RealPageId = @OrganizationRealPageId;
		print @PartyIdto
		IF @PartyRelationshipTypeId IS NULL
		BEGIN
			RAISERROR('The Relationship is invalid between Role Type %i and Role Type %i', 16, -1, @RoleTypeIdFrom, @RoleTypeIdTo);
		END;

        BEGIN TRANSACTION; 
		
		UPDATE Enterprise.PartyRelationship
		SET ThruDate = GETUTCDATE()
		OUTPUT	Inserted.PartyRelationshipId AS Id,
					'' AS ErrorMessage
		WHERE PartyIdFrom = @PartyIdFrom AND PartyIdTo = @PartyIdTo 
			  AND RoleTypeIdFrom = @RoleTypeIdFrom
			  AND RoleTypeIdTo = @RoleTypeIdTo
			  AND ThruDate IS NULL

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
