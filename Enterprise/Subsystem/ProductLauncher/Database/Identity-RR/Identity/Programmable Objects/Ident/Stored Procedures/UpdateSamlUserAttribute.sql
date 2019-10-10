IF OBJECT_ID('[Ident].[UpdateSamlUserAttribute]') IS NOT NULL
	DROP PROCEDURE [Ident].[UpdateSamlUserAttribute];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[UpdateSamlUserAttribute] (
	@SamlUserAttributeId int,
	@Value nvarchar(500)
)
AS
BEGIN
	BEGIN TRY
        BEGIN TRANSACTION;
		UPDATE	Ident.SamlUserAttribute
		SET		Value = @Value
		OUTPUT	Inserted.SamlUserAttributeId AS Id,
				'' AS ErrorMessage
		WHERE	SamlUserAttributeId = @SamlUserAttributeId
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
