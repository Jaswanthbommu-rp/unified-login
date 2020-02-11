CREATE PROCEDURE [Enterprise].[GetUserProductBatchJsonData] (
	@ProductId INT,
	@PersonaId INT
)
AS
BEGIN
	SELECT	TOP 1 InputJSON
	FROM		Batch.BatchProcessor
	WHERE	ISJSON(InputJSON) > 0
	AND			(JSON_VALUE(InputJSON, '$.IsAssigned') = 'true' OR (JSON_VALUE(InputJSON, '$.OneSite.IsAssigned') = 'true' AND JSON_VALUE(InputJSON, '$.Lead2Lease.IsAssigned') = 'true'))
	AND			SubjectUserPersonaId = @PersonaId
	AND			ProductId = @ProductId
	AND			StatusTypeId = 8
	AND			BatchProcessTypeId = 1
	ORDER BY BatchProcessorId DESC
END;