CREATE PROCEDURE [Enterprise].[GetUserSyncJobTopics]	
AS
BEGIN
	Select UserSyncJobTypeId,
	[Name],
    KafkaTopicName 
    From Enterprise.UserSyncJobType
END
