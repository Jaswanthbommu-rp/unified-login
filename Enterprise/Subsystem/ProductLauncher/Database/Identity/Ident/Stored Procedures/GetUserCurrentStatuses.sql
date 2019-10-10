CREATE PROCEDURE [Ident].[GetUserCurrentStatuses] (
	@RealPageId UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
	SELECT	ul.UserId,
					st.StatusTypeId,
					st.Name,
					ULP.FromDate,
					ULP.ThruDate AS 'ThruDate'
	FROM	Ident.UserLogin AS ul
				INNER JOIN Ident.UserLoginPersona ULP ON ul.UserId = ULP.UserLoginId
				INNER JOIN Person.Persona PE ON pe.UserLoginPersonaId = ULP.UserLoginPersonaId
				INNER JOIN Enterprise.Party AS p ON p.PartyId = ul.PersonPartyId
				INNER JOIN Enterprise.StatusType AS st ON ULP.StatusTypeId = st.StatusTypeId
	WHERE	(p.RealPageId = @RealPageId OR @RealPageId IS NULL)
	AND			ULP.ThruDate IS NOT NULL;
END;
GO