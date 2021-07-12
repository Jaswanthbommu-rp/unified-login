CREATE PROCEDURE [Logging].[ListActivityDetails]
	 @ActivityId BIGINT 
AS
BEGIN
SET NOCOUNT ON;

	SELECT  
		[ActivityDetailId],
		[ActivityId],
		[Key],
		[Value]
	FROM  
		[Logging].[ActivityDetail]
	WHERE 
		[ActivityId] = @ActivityId
	 
END
