CREATE PROC Ident.RemoveSamlUserAttributeByProductId
    @PersonaId INT ,
    @ProductId INT
AS
    BEGIN
        DECLARE @ThruDate DATETIME = GETUTCDATE();
        UPDATE SamlUserAttribute
        SET    ThruDate = @ThruDate
        FROM   Ident.SamlUserAttribute
        WHERE  PersonaId = @PersonaId
               AND ProductId = @ProductId
			   AND ThruDate IS NULL;
    END;
