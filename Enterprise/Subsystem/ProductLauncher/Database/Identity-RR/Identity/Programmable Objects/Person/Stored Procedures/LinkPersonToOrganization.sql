IF OBJECT_ID('[Person].[LinkPersonToOrganization]') IS NOT NULL
	DROP PROCEDURE [Person].[LinkPersonToOrganization];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Person].[LinkPersonToOrganization] (
	@PersonRealPageId UNIQUEIDENTIFIER,
	@OrganizationRealPageId UNIQUEIDENTIFIER,
	@RoleTypeIdFrom INT,
	@RoleTypeIdTo INT,
	@FromDate datetime = null,
	@ThruDate dateTime = null
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

        SELECT  @PartyIdTo = p.PartyId
        FROM    Enterprise.Party p
        WHERE   p.RealPageId = @OrganizationRealPageId;

		IF @FromDate IS NULL 
			SET @FromDate = GETUTCDATE()  

		IF @PartyRelationshipTypeId IS NULL
		BEGIN
			RAISERROR('The Relationship is invalid between Role Type %i and Role Type %i', 16, -1, @RoleTypeIdFrom, @RoleTypeIdTo);
		END;

        BEGIN TRANSACTION; 

		INSERT INTO Enterprise.PartyRelationship (
			PartyIdFrom ,
			PartyIdTo ,
			RoleTypeIdFrom ,
			RoleTypeIdTo ,
			PartyRelationshipTypeId ,
			FromDate ,
			ThruDate
		)
		OUTPUT	Inserted.PartyRelationshipId AS Id,
				'' AS ErrorMessage
		VALUES (
			@PartyIdFrom, -- PartyIdFrom - bigint
			@PartyIdTo, -- PartyIdTo - bigint
			@RoleTypeIdFrom, -- RoleTypeIdFrom - int
			@RoleTypeIdTo, -- RoleTypeIdTo - int
			@PartyRelationshipTypeId , -- PartyRelationshipTypeId - int
			@FromDate, -- FromDate - datetime
			@ThruDate  -- ThruDate - datetime
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
