Create PROCEDURE [Person].[ListEmployeePersonas] (
	@UserId bigint,
	@OrgPartyId bigint
)
AS
BEGIN
	DECLARE @NOW datetime = GETUTCDATE()

	SELECT	pe.PersonaId,					
			pe.PersonaName AS 'Name'
	FROM Person.Persona PE
		 INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
		 INNER JOIN Ident.UserLogin UL ON UL.UserId = ULP.UserLoginId
	WHERE	UL.UserId = @UserId
	AND     ULP.OrganizationPartyId = @OrgPartyId
	AND		ULP.IsRPEmployee = 1
	AND		ULP.StatusTypeId IN (1, 2, 12)
	AND		((@NOW BETWEEN pe.FromDate AND pe.ThruDate) OR (@NOW >= pe.FromDate AND pe.ThruDate IS NULL))
	AND		((@NOW BETWEEN ulp.FromDate AND ulp.ThruDate) OR (@NOW >= ulp.FromDate AND ulp.ThruDate IS NULL))
END
