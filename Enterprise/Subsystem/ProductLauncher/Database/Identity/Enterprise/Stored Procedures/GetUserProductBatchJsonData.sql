CREATE PROCEDURE [Enterprise].[GetUserProductBatchJsonData]
(@ProductId INT,
 @PersonaId INT
)
AS
     BEGIN
         WITH CTE AS(
		 SELECT  BatchProcessorId, InputJSON, CreatedDateTime
			 FROM Batch.BatchProcessor
			 WHERE ISJSON(InputJSON) > 0
			 AND ( JSON_VALUE(InputJSON, '$.IsAssigned') = 'true' OR (JSON_VALUE(InputJSON, '$.OneSite.IsAssigned') = 'true' AND JSON_VALUE(InputJSON, '$.Lead2Lease.IsAssigned') = 'true') )
			 AND SubjectUserPersonaId = @PersonaId
			 AND ProductId = @ProductId
			 AND StatusTypeId = 8
			
         UNION
         SELECT ProductBatchId, InputJson, CreatedDate
			 FROM [Enterprise].[ProductBatch]
			 WHERE ISJSON(InputJson) > 0
			 AND ( JSON_VALUE(InputJSON, '$.IsAssigned') = 'true' OR (JSON_VALUE(InputJSON, '$.OneSite.IsAssigned') = 'true' AND JSON_VALUE(InputJSON, '$.Lead2Lease.IsAssigned') = 'true') )
			 AND AssignUserPersonaId = @PersonaId
			 AND ProductId = @ProductId
			 AND StatusTypeId = 8
        )

		 SELECT TOP 1 InputJson FROM CTE
		 ORDER By BatchProcessorId DESC, CreatedDateTime

     END;




