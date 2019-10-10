IF OBJECT_ID('[Enterprise].[MapBlueBookIdtoPartyId]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[MapBlueBookIdtoPartyId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[MapBlueBookIdtoPartyId]
    @SourceId NVARCHAR(50) ,
    @PartyId INT
AS
    BEGIN
        INSERT INTO Enterprise.DataImportMapping (   DataImportApplicationId ,
                                                     SourceId ,
                                                     PartyId ,
                                                     DateCreated
                                                 )
		OUTPUT Inserted.DataImportMappingId AS Id, '' AS ErrorMessage
        VALUES ( 1, @SourceId, @PartyId, GETUTCDATE());
    END;
GO
