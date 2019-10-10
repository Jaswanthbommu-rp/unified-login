IF OBJECT_ID('[Person].[CreatePersonaType]') IS NOT NULL
	DROP PROCEDURE [Person].[CreatePersonaType];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROC [Person].[CreatePersonaType] (
    @PersonaName VARCHAR(50) ,
    @PersonaTypeId INT = NULL OUTPUT
)
AS
BEGIN
	BEGIN TRY
		BEGIN TRANSACTION;
		-- Check if it exists
		SELECT @PersonaTypeId = PersonaTypeId
		FROM   Person.PersonaType
		WHERE  Name = @PersonaName;

		IF @PersonaTypeId IS NULL
			BEGIN
				INSERT INTO Person.PersonaType ( Name )
				VALUES ( @PersonaName );

				SELECT @PersonaTypeId = SCOPE_IDENTITY();
			END;

		SELECT @PersonaTypeId AS Id, '' AS ErrorMessage;

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
