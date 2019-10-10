CREATE PROCEDURE [Enterprise].[GetOrganizationByBlueBookId]
@BlueBookId INT
AS
BEGIN
	DECLARE @RealPageId UNIQUEIDENTIFIER;

    SELECT @RealPageId = RealPageId
	FROM Enterprise.DataImportMapping
	JOIN Enterprise.Party ON Party.PartyId = DataImportMapping.PartyId
	WHERE SourceId = @BlueBookId

	EXEC Enterprise.GetOrganization @RealPageId

END

