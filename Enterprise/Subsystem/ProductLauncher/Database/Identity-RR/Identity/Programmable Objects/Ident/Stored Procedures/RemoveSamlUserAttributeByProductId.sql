IF OBJECT_ID('[Ident].[RemoveSamlUserAttributeByProductId]') IS NOT NULL
	DROP PROCEDURE [Ident].[RemoveSamlUserAttributeByProductId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROC [Ident].[RemoveSamlUserAttributeByProductId]
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
GO
