IF OBJECT_ID('[Person].[UpdatePersona]') IS NOT NULL
	DROP PROCEDURE [Person].[UpdatePersona];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROC [Person].[UpdatePersona](
	 @PersonaId bigint
	,@PersonaTypeId INT = NULL
	,@PersonaEnvironmentTypeId INT = 1
    ,@FromDate DATETIME = NULL
	,@ThruDate DATETIME = NULL
)
AS
    BEGIN

		BEGIN TRY
			BEGIN TRANSACTION;

			UPDATE Person.Persona 
			SET PersonaTypeId = ISNULL(@PersonaTypeId, PersonaTypeId),
			    PersonaEnvironmentTypeId = ISNULL(@PersonaEnvironmentTypeId, PersonaEnvironmentTypeId),
				FromDate = ISNULL(@FromDate, FromDate),
				ThruDate = ISNULL(@ThruDate, ThruDate)
			OUTPUT Inserted.PersonaId AS Id , '' AS ErrorMessage
			WHERE PersonaID = @PersonaId

			COMMIT;
        END TRY
        BEGIN CATCH
            ROLLBACK;

            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

            SELECT 0 AS Id,
                   ErrorMessage
            FROM   dbo.ErrorLog
            WHERE  ErrorLogID = @ErrorLogID;
        END CATCH;

    END;
GO
