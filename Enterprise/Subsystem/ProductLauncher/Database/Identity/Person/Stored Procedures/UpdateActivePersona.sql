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
		IF @@ROWCOUNT = 0
		BEGIN
			-- PERSONA IS MISSING SO ADD A NEW ONE
			INSERT INTO Person.ActivePersona ( PartyId, PersonaId )
			SELECT PartyId, @PersonaId
				FROM Enterprise.Party P
			WHERE  P.RealPageId = @RealPageID;
		END
        
          UPDATE ULP  
          SET ULP.LastLoginDate = GETUTCDATE()  
          FROM Person.Persona P  
          inner join Ident.UserLoginPersona ULP on ULP.UserLoginPersonaID = P.UserLoginPersonaID  
          where P.PersonaId = @PersonaId and ULP.StatusTypeId = 1;  
 
        UPDATE UL
         SET UL.LastLoginDate = GETUTCDATE()
         FROM Ident.UserLogin UL 
         inner join Enterprise.Party P on UL.PersonPartyId = P.PartyId
         inner join Ident.UserLoginPersona ULP on ULP.UserLoginId = UL.UserId
         inner join Person.Persona PEA on PEA.UserLoginPersonaId = ULP.UserLoginPersonaId
         inner join Person.Person PER on PER.PartyId = P.PartyId
         where P.RealpageId = @RealPageID and ULP.StatusTypeId = 1 and PEA.PersonaId = @PersonaId;
    END;