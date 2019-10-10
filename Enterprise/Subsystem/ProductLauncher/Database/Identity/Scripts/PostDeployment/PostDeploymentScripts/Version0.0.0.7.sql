--PRINT 'Backfill PersonaIdentityUserLogin';

--SELECT ul.UserId ,
--       ps.PersonaId ,
--       46
--FROM   Ident.UserLogin ul
--       INNER JOIN Enterprise.Party p ON ul.PartyId = p.PartyId
--       INNER JOIN Person.Persona ps ON p.PartyId = ps.PersonPartyId
--WHERE  ps.PersonaId NOT IN (   SELECT PersonaId
--                               FROM   Enterprise.PersonaIdentityUserLogin
--                           )
--       AND ul.FromDate <= GETUTCDATE()
--       AND (   ul.ThruDate IS NULL
--               OR ul.ThruDate >= GETUTCDATE()
--           );

--UPDATE Enterprise.PersonaIdentityUserLogin
--SET    ContactMechanismID = 46
--WHERE  ContactMechanismID IS NULL;

--The above statement is not required as new schema chnages supports all the functionality.

EXEC sys.sp_updateextendedproperty @name = N'Build' , @value = '8';