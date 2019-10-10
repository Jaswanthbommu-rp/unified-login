CREATE PROCEDURE [Enterprise].[GetOrganization] (
	@RealPageId uniqueidentifier = NULL,
	@BlueBookId INT = NULL
)
AS
BEGIN
	PRINT 'This Procedure is repaces with other.'
	--SELECT	o.PartyId,
	--		o.Name,
	--		RealPageId,
	--		CreateDate,
	--		edim.SourceId AS 'BooksMasterId'
	--FROM	[Enterprise].Organization AS o  
	--		INNER JOIN [Enterprise].Party ON Party.PartyId = o.PartyId
	--		LEFT OUTER JOIN Enterprise.DataImportMapping edim ON (o.PartyId = edim.PartyId)
	--WHERE	((@RealPageId IS NULL) OR (RealPageId = @RealPageId))
	--AND		((@BlueBookId IS NULL) OR (@BlueBookId = edim.SourceId))
END;
