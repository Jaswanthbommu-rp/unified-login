IF OBJECT_ID('[Person].[GetPersona]') IS NOT NULL
	DROP PROCEDURE [Person].[GetPersona];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Person].[GetPersona] (
	@PersonaId bigint
)
AS
BEGIN

	DECLARE @NOW  DATETIME = GETUTCDATE();

	SELECT	pe.PersonaId,
			pe.PersonPartyId,
			p.RealPageId,
			pe.OrganizationPartyId,
			pe.PersonaTypeId,
			pe.PersonaEnvironmentTypeId,
			pt.Name,
			pe.FromDate,
			pe.ThruDate,
			pe.IsDefault
	FROM	Person.Persona pe
			INNER JOIN Enterprise.Party p ON (pe.PersonPartyId = p.PartyId)
			INNER JOIN Person.PersonaType pt ON (pe.PersonaTypeId = pt.PersonaTypeId)
	WHERE	pe.PersonaId = @PersonaId
	AND ((@NOW BETWEEN pe.FromDate AND pe.ThruDate) OR (@NOW >= pe.FromDate AND pe.ThruDate IS NULL))
END
GO
