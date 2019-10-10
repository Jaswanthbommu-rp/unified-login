IF OBJECT_ID('[Person].[ListPersons]') IS NOT NULL
	DROP PROCEDURE [Person].[ListPersons];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE  [Person].[ListPersons]
(
	@RealPageId UNIQUEIDENTIFIER = NULL,
	@Name NVARCHAR(100) = NULL,
	@ProductId INT = NULL
)

AS

BEGIN

	DECLARE @NOW  DATETIME = GETUTCDATE();

	SELECT	DISTINCT 
			personParty.RealPageId,
			Person.PartyId,
			Person.FirstName ,
			Person.MiddleName ,
			Person.LastName ,
			Person.Title ,
			Person.Suffix ,
			UserLogin.UserId ,
			UserLogin.LoginName ,
			UserLogin.LastLoginDate AS LastLogin,
			UserLogin.FromDate,
			UserLogin.ThruDate,			
			ISNULL(Products.ProductCount,0) AS [Products],
			0 AS [Properties],
			ISNULL(rtf.Name, '') AS [UserType]
	FROM	Enterprise.PartyRelationship
			JOIN Person.Person ON PartyRelationship.PartyIdFrom = Person.PartyId
			JOIN Enterprise.Party personParty ON personParty.PartyId = Person.PartyId
			JOIN Ident.UserLogin ON UserLogin.PartyId = Person.PartyId			
			JOIN Enterprise.Organization ON PartyRelationship.PartyIdTo = Organization.PartyId
			JOIN Enterprise.Party OrgParty ON Organization.PartyId = OrgParty.PartyId
			LEFT JOIN (
				SELECT	COUNT(DISTINCT ProductId) AS ProductCount, PartyId
				FROM	Ident.SamlUserAttribute
						JOIN Person.ActivePersona ON ActivePersona.PersonaId = SamlUserAttribute.PersonaId
						WHERE ProductId = @ProductId OR @ProductId IS NULL
						GROUP BY PartyId ) 
				AS Products ON Products.PartyId = Person.PartyId

			JOIN Enterprise.RoleType rtf ON (PartyRelationship.RoleTypeIdFrom = rtf.PartyRoleTypeId)
			JOIN Enterprise.[RelationshipType] rt ON (PartyRelationship.PartyRelationshipTypeId = rt.RelationshipTypeId)			
			LEFT OUTER JOIN Enterprise.RoleType prt ON (rtf.ParentPartyRoleTypeId = prt.PartyRoleTypeId)

	WHERE  (OrgParty.RealPageId = @RealPageId OR @RealPageId IS NULL)
		   AND 
		   (
				(FirstName LIKE @Name +'%' OR @Name IS NULL)
				OR 
				(LastName LIKE @Name + '%' OR @Name IS NULL)    
				OR 
				(LoginName LIKE @Name + '%' OR @Name IS NULL)    
		   )

	AND ((@NOW BETWEEN PartyRelationship.FromDate AND PartyRelationship.ThruDate) OR (@NOW >= PartyRelationship.FromDate AND PartyRelationship.ThruDate IS NULL))
	
END;
GO
