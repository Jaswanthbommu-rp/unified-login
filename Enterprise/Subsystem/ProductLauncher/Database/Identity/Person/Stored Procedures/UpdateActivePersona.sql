CREATE PROCEDURE Person.UpdateActivePersona
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

        UPDATE ULP
		SET ULP.LastLoginDate = GETUTCDATE()
		FROM Person.Persona P
		inner join Ident.UserLoginPersona ULP on ULP.UserLoginPersonaID = P.UserLoginPersonaID
		inner join Ident.UserLogin UL on UL.UserId = ULP.UserLoginId
		where P.PersonaId = @PersonaId;

		IF @@ROWCOUNT = 0
		BEGIN
			-- PERSONA IS MISSING SO ADD A NEW ONE
			INSERT INTO Person.ActivePersona ( PartyId, PersonaId )
			SELECT PartyId, @PersonaId
				FROM Enterprise.Party P
			WHERE  P.RealPageId = @RealPageID;
		END

    END;