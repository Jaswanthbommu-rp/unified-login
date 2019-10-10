IF OBJECT_ID('[Person].[GetDefaultPersona]') IS NOT NULL
	DROP PROCEDURE [Person].[GetDefaultPersona];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Person].[GetDefaultPersona]
		@RealPageId UNIQUEIDENTIFIER
AS
BEGIN

	DECLARE @NOW  DATETIME = GETUTCDATE();

	SELECT	per.PersonaId, 
			per.PersonaTypeId as PersonaType, 
			ppt.Name 
	FROM	Person.Persona per
			JOIN Enterprise.Party epar ON epar.PartyId = per.PersonPartyId
			JOIN Person.PersonaType ppt ON ppt.PersonaTypeId = per.PersonaTypeId	
	WHERE	epar.RealPageId  = @RealPageId and per.IsDefault = 1
	AND ((@NOW BETWEEN per.FromDate AND per.ThruDate) OR (@NOW >= per.FromDate AND per.ThruDate IS NULL))
END
GO
