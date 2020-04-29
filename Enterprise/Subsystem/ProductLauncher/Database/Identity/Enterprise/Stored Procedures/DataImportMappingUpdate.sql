CREATE PROCEDURE [Enterprise].[DataImportMappingUpdate] (
	@ApplicationId INT, --1 Black, 2 blue
	@PartyId       INT,
	@Original_SourceId NVARCHAR(50),
	@SourceId      NVARCHAR(50)
)
AS
BEGIN
	SET NOCOUNT OFF;
	UPDATE [Enterprise].[DataImportMapping] 
	SET 
		[SourceId] = @SourceId
		, [DateCreated] = GETUTCDATE()
	WHERE 
		PartyId = @PartyId
		AND DataImportApplicationId = @ApplicationId
		AND SourceId = @Original_SourceId

	SELECT
		DataImportMappingId as Id,
		ErrorMessage = ''
	FROM
		[Enterprise].[DataImportMapping]
	WHERE
		PartyId = @PartyId
		AND DataImportApplicationId = @ApplicationId
		AND SourceId = @SourceId
END
