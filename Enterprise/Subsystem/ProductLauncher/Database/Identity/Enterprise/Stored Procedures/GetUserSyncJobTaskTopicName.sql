Create PROCEDURE [Enterprise].[GetUserSyncJobTaskTopicName]
(@UserSyncJobTaskId BIGINT)
AS
BEGIN
	Select JT.KafkaTopicName 
	FROM  Enterprise.UserSyncJobTask_V2 sjt
	INNER JOIN Enterprise.UserSyncJob_V2 sj ON
		sjt.UserSyncJobId = sj.UserSyncJobId
	INNER JOIN [Enterprise].[UserSyncJobType] JT ON
		sj.[UserSyncJobTypeId] = JT.[UserSyncJobTypeId]
	 WHERE UserSyncJobTaskId = @UserSyncJobTaskId
END
