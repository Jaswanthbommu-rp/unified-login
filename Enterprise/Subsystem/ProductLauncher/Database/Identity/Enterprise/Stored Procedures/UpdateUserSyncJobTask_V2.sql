CREATE PROCEDURE [Enterprise].[UpdateUserSyncJobTask_V2]
(@UserSyncJobTaskId	BIGINT,
 @StatusTypeId INT)
AS
BEGIN
SET NOCOUNT ON;

	DECLARE @PendingStatusTypeId INT = 2;
	DECLARE @SuccessStatusTypeId INT = 8;
	DECLARE @FailureStatusTypeId INT = 7;
	DECLARE @UserSyncJobId BIGINT; 
	
	SELECT @UserSyncJobId = UserSyncJobId FROM Enterprise.UserSyncJobTask_V2 WHERE UserSyncJobTaskId = @UserSyncJobTaskId

	UPDATE 
		Enterprise.UserSyncJobTask_V2
	SET	
		StatusTypeId = @StatusTypeId,
		ModifiedDate = GETUTCDATE()
	WHERE 
		UserSyncJobTaskId = @UserSyncJobTaskId

		
	;WITH UserSyncJobSummaryCte AS (
		SELECT 
			TotalCount = COUNT(*),
			PendingCount = SUM(CASE WHEN sjt.StatusTypeId = @PendingStatusTypeId THEN 1 ELSE 0 END),
			SuccessCount = SUM(CASE WHEN sjt.StatusTypeId = @SuccessStatusTypeId THEN 1 ELSE 0 END),
			FailureCount = SUM(CASE WHEN sjt.StatusTypeId = @FailureStatusTypeId THEN 1 ELSE 0 END),
			sj.UserSyncJobId
		FROM 
			Enterprise.UserSyncJob_V2 sj JOIN 
			Enterprise.UserSyncJobTask_V2 sjt ON sj.UserSyncJobId  = sjt.UserSyncJobId
		WHERE sjt.UserSyncJobId = @UserSyncJobId
		GROUP BY (sj.UserSyncJobId)
	)

	UPDATE sj
	SET sj.StatusTypeId = 
			CASE
				WHEN cte.FailureCount > 0 THEN @FailureStatusTypeId
				WHEN cte.SuccessCount = cte.TotalCount THEN @SuccessStatusTypeId
				ELSE sj.StatusTypeId
			END,
		sj.ModifiedDate = GETUTCDATE()
	FROM 
		Enterprise.UserSyncJob_V2 sj JOIN 
		UserSyncJobSummaryCte cte ON sj.UserSyncJobId = cte.UserSyncJobId
END;
