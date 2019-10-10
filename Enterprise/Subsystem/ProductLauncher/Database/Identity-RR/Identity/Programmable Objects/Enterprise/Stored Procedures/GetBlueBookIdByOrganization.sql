IF OBJECT_ID('[Enterprise].[GetBlueBookIdByOrganization]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[GetBlueBookIdByOrganization];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROC [Enterprise].[GetBlueBookIdByOrganization]
@RealPageId UNIQUEIDENTIFIER
AS
BEGIN

    SELECT SourceId AS BlueBookId
	FROM Enterprise.DataImportMapping
	JOIN Enterprise.Party ON Party.PartyId = DataImportMapping.PartyId
	WHERE RealPageId = @RealPageId

END
GO
