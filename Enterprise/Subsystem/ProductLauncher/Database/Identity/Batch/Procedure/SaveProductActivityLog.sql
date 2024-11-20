CREATE PROCEDURE [Batch].[SaveProductActivityLog](
    @BatchProcessorGroupId bigint
	,@ProductId int
	,@jsonstring NVARCHAR(MAX)
)
AS
BEGIN
    -- Parse the JSON and insert into the table
    INSERT INTO Batch.ProductActivityLog (BatchProcessorGroupId,ProductId, [Key], [ActivityJSONMessage])
    SELECT 
		@BatchProcessorGroupId,
		@ProductId,
        JSON_VALUE(value, '$.Key') AS [Key],
        JSON_VALUE(value, '$.Value') AS [Value]
    FROM OPENJSON(@jsonstring);
END;