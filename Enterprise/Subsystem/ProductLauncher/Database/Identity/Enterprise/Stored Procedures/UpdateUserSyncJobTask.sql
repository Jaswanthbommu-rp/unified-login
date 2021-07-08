CREATE PROCEDURE [Enterprise].[UpdateUserSyncJobTask]
(@UserSyncJobTaskId	BIGINT,
 @StatusTypeId INT)
AS
BEGIN
SET NOCOUNT ON;

	DECLARE @PendingStatusTypeId INT = 26;
	DECLARE @SuccessStatusTypeId INT = 27;
	DECLARE @FailureStatusTypeId INT = 28;
	DECLARE @UserSyncJobId BIGINT; 
	
	SELECT @UserSyncJobId = UserSyncJobId FROM Enterprise.UserSyncJobTask WHERE UserSyncJobTaskId = @UserSyncJobTaskId

	UPDATE 
		Enterprise.UserSyncJobTask
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
			Enterprise.UserSyncJob sj JOIN 
			Enterprise.UserSyncJobTask sjt ON sj.UserSyncJobId  = sjt.UserSyncJobId
		WHERE sjt.UserSyncJobId = @UserSyncJobId
		GROUP BY (sj.UserSyncJobId)
	)

	UPDATE sj
	SET sj.StatusTypeId = 
			CASE
				WHEN cte.FailureCount > 0 THEN 28
				WHEN cte.SuccessCount = cte.TotalCount THEN 27
				ELSE sj.StatusTypeId
			END,
		sj.ModifiedDate = GETUTCDATE()
	FROM 
		Enterprise.UserSyncJob sj JOIN 
		UserSyncJobSummaryCte cte ON sj.UserSyncJobId = cte.UserSyncJobId
END;
