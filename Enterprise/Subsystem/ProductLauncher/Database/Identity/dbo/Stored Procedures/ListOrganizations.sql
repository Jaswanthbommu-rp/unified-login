CREATE PROCEDURE [dbo].[ListOrganizations]
	@RealPageId UNIQUEIDENTIFIER = NULL
AS
BEGIN
	SELECT O.PartyId, 
		O.Name,
		P.RealPageId,
		D.SourceId AS 'BooksMasterId'
	FROM Enterprise.Organization O
	INNER JOIN Enterprise.Party P ON O.PartyId = P.PartyId
	INNER JOIN Enterprise.DataImportMapping D ON O.PartyId = D.PartyId
	WHERE P.RealPageId = @RealPageId OR @RealPageId IS NULL;
END
RETURN 0