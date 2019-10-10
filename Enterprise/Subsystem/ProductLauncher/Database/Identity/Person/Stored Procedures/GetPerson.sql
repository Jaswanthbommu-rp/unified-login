CREATE PROCEDURE [Person].[GetPerson](@RealPageId UNIQUEIDENTIFIER)
AS
         BEGIN
             SELECT Party.RealPageId,
                    Person.PartyId,
                    Person.Title,
                    Person.FirstName,
                    Person.MiddleName,
                    Person.LastName,
                    Person.Suffix,
                    Person.PreferredContactMethodId
             FROM Person.Person
				INNER JOIN Enterprise.Party ON Party.PartyId = Person.PartyId
             WHERE Party.RealPageId = @RealPageId;
         END;

