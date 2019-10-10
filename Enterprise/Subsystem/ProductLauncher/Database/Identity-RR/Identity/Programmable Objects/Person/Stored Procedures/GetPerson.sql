IF OBJECT_ID('[Person].[GetPerson]') IS NOT NULL
	DROP PROCEDURE [Person].[GetPerson];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Person].[GetPerson] (
    @RealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
    SELECT  Party.RealPageId,
			Person.PartyId,
            Person.Title,
            Person.FirstName,
            Person.MiddleName,
            Person.LastName,
            Person.Suffix,
			Person.PreferredContactMethodId
    FROM    Person.Person
			JOIN Enterprise.Party ON Party.PartyId = Person.PartyId
    WHERE   Party.RealPageId = @RealPageId;
END
GO
