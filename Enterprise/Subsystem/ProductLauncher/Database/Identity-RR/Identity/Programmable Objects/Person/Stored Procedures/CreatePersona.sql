IF OBJECT_ID('[Person].[CreatePersona]') IS NOT NULL
	DROP PROCEDURE [Person].[CreatePersona];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Person].[CreatePersona]
    @PersonRealPageId UNIQUEIDENTIFIER,
    @OrganizationRealPageId UNIQUEIDENTIFIER,
    @PersonaTypeId INT,
    @PersonaEnvironmentTypeId INT,
    @FromDate DATETIME,
    @ThruDate DATETIME = NULL,
    @PersonaId BIGINT = NULL OUTPUT
AS
    BEGIN
		IF @FromDate IS NULL
			SELECT @FromDate = GETUTCDATE();

        BEGIN TRY
            DECLARE @PersonPartyId BIGINT,
                    @OrganizationPartyId BIGINT;

            -- Get the Party ID for a Person
            SELECT @PersonPartyId = PartyId
            FROM   Enterprise.Party
            WHERE  RealPageId = @PersonRealPageId;

            -- Get the Party ID for an Organization
            SELECT @OrganizationPartyId = PartyId
            FROM   Enterprise.Party
            WHERE  RealPageId = @OrganizationRealPageId;

            BEGIN TRANSACTION;

			-- Check if it exists
			SELECT	@PersonaId = PersonaId 
			FROM	Person.Persona
			WHERE	Persona.PersonPartyId = @PersonPartyId
			AND		Persona.OrganizationPartyId = @OrganizationPartyId
			AND		Persona.PersonaTypeId = @PersonaTypeId

			IF  @PersonaId IS NULL
				BEGIN
					INSERT INTO Person.Persona (
						PersonPartyId,
						OrganizationPartyId,
						PersonaTypeId,
						PersonaEnvironmentTypeId,
						FromDate,
						ThruDate
					)
					VALUES (
						@PersonPartyId,
						@OrganizationPartyId,
						@PersonaTypeId,
						@PersonaEnvironmentTypeId,
						@FromDate,
						@ThruDate
					);

					SET @PersonaId = SCOPE_IDENTITY()					
				END;

            SELECT	@PersonaId AS Id ,
					'' AS ErrorMessage
            COMMIT;
        END TRY
        BEGIN CATCH
            ROLLBACK;

            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id,
                   '' AS RealPageId,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;
        END CATCH;
    END;
GO
