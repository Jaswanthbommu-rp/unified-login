CREATE PROCEDURE [Auth].[GetEnterpriseUserLockActivities]
	@enterpriseUserId	bigint
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

   SELECT [UserLockActivityId] ,[UserId] ,[AcivityId] ,[LockReason]
      ,[IsLockActive] ,[LockDateTime]
  FROM [Auth].[UserLockAcitvity] WHERE [UserId] = @enterpriseUserId and [IsLockActive] =1

END