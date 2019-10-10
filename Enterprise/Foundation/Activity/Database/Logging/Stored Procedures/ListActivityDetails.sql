CREATE PROCEDURE [Logging].[ListActivityDetails]
	 @activityId bigint 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

     SELECT  [ActivityDetailId] ,[ActivityId] ,[Key] ,[Value]
	  FROM  [Logging].[ActivityDetail]
	  WHERE [ActivityId]=@activityId
	 
END
