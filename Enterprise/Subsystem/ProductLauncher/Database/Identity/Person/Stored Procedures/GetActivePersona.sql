CREATE PROCEDURE Person.GetActivePersona
    @RealPageId UNIQUEIDENTIFIER
AS
    BEGIN

        SELECT ap.PersonaId
        FROM   Person.ActivePersona ap
               JOIN Person.Person per ON per.PartyId = ap.PartyId
               JOIN Enterprise.Party p ON p.PartyId = per.PartyId
        WHERE  p.RealPageId = @RealPageId;

    END;