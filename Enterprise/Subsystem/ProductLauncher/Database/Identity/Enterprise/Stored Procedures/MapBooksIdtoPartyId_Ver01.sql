CREATE PROCEDURE [Enterprise].[MapBooksIdtoPartyId_Ver01]
(@ApplicationId INT, --1 Black, 2 blue
 @SourceId      NVARCHAR(50),
 @PartyId       INT
)
AS
     BEGIN
         INSERT INTO Enterprise.DataImportMapping
			(DataImportApplicationId,
			 SourceId,
			 PartyId,
			 DateCreated
			)
         OUTPUT Inserted.DataImportMappingId AS Id,
                '' AS ErrorMessage
         VALUES
			(@ApplicationId,
			 @SourceId,
			 @PartyId,
			 GETUTCDATE()
			);
     END;

