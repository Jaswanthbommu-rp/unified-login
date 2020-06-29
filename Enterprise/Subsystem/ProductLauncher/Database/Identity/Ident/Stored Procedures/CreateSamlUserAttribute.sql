CREATE PROCEDURE Ident.CreateSamlUserAttribute (
    @PersonaId BIGINT,
    @ProductId INT,
    @SamlAttributeId INT,
    @Value NVARCHAR(500)
)
AS
BEGIN
    BEGIN TRY
        IF EXISTS ( SELECT TOP 1 1 FROM Ident.SamlUserAttribute WHERE PersonaId = @PersonaId AND ProductId = @ProductId AND SamlAttributeId = @SamlAttributeId AND Value = @Value )
        BEGIN
            SELECT 
                SamlUserAttributeId AS Id
                ,'' AS ErrorMessage
            FROM Ident.SamlUserAttribute 
                WHERE PersonaId = @PersonaId AND ProductId = @ProductId AND SamlAttributeId = @SamlAttributeId AND Value = @Value
            RETURN
        END

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