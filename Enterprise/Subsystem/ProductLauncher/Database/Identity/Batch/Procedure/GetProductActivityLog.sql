CREATE PROCEDURE [Batch].[GetProductActivityLog](
@BatchProcessorGroupId bigint
)
AS
BEGIN

	select [Key]
	,[ActivityJSONMessage] as [Value]
	from batch.ProductActivityLog
	where BatchProcessorGroupId = @BatchProcessorGroupId
END