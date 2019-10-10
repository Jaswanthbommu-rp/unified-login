CREATE PROCEDURE Enterprise.MapBlueBookIdtoPartyId
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