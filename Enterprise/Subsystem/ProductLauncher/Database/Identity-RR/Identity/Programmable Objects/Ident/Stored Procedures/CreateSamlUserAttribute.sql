IF OBJECT_ID('[Ident].[CreateSamlUserAttribute]') IS NOT NULL
	DROP PROCEDURE [Ident].[CreateSamlUserAttribute];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[CreateSamlUserAttribute] (
    @PersonaId BIGINT,
    @ProductId INT,
    @SamlAttributeId INT,
    @Value NVARCHAR(500)
)
AS
BEGIN
    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO Ident.SamlUserAttribute
		(
			PersonaId,
			ProductId,
			SamlAttributeId,
			Value
        )
        OUTPUT	Inserted.SamlUserAttributeId AS Id,
                '' AS ErrorMessage
        VALUES
		(
			@PersonaId,
			@ProductId,
			@SamlAttributeId,
			@Value
		);
        COMMIT;
    END TRY
    BEGIN CATCH
        ROLLBACK;

        DECLARE @ErrorLogID INT;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT	0 AS Id ,
                ErrorMessage
        FROM	dbo.ErrorLog
        WHERE	ErrorLogID = @ErrorLogID;
    END CATCH;
END;
GO
