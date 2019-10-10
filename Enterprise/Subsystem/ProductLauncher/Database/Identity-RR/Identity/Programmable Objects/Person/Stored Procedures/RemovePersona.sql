IF OBJECT_ID('[Person].[RemovePersona]') IS NOT NULL
	DROP PROCEDURE [Person].[RemovePersona];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Person].[RemovePersona] (
    @PersonaId bigint 
)
AS
BEGIN
    BEGIN TRY
		BEGIN TRANSACTION
        UPDATE	Persona
		SET		ThruDate = GETUTCDATE()
		OUTPUT	Inserted.PersonaId AS Id,
				'' AS ErrorMessage
		WHERE	PersonaId = @PersonaId
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT	0 AS Id,
                ErrorMessage
        FROM	dbo.ErrorLog
        WHERE	ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
