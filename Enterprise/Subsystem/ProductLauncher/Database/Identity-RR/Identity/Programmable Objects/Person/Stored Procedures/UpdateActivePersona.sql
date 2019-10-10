IF OBJECT_ID('[Person].[UpdateActivePersona]') IS NOT NULL
	DROP PROCEDURE [Person].[UpdateActivePersona];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Person].[UpdateActivePersona]
    @RealPageID UNIQUEIDENTIFIER ,
    @PersonaId BIGINT
AS
    BEGIN

        UPDATE ap
        SET    PersonaId = @PersonaId
        FROM   Person.ActivePersona ap
               JOIN Person.Person per ON per.PartyId = ap.PartyId
               JOIN Enterprise.Party p ON p.PartyId = per.PartyId
        WHERE  p.RealPageId = @RealPageID;


    END;
GO
