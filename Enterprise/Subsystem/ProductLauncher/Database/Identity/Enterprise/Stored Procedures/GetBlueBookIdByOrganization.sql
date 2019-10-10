CREATE PROC [Enterprise].[GetBlueBookIdByOrganization]
@RealPageId UNIQUEIDENTIFIER
AS
BEGIN

    SELECT SourceId AS BlueBookId
	FROM Enterprise.DataImportMapping
	JOIN Enterprise.Party ON Party.PartyId = DataImportMapping.PartyId
	WHERE RealPageId = @RealPageId

END