IF OBJECT_ID('[Enterprise].[GetOrganizationByBlueBookId]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[GetOrganizationByBlueBookId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROC [Enterprise].[GetOrganizationByBlueBookId]
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
GO
