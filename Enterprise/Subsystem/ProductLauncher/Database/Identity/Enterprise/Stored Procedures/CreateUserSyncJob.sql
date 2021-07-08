CREATE PROCEDURE [Enterprise].[CreateUserSyncJob]
(@PersonaId	BIGINT,
 @Sources	[Enterprise].[SourceList] READONLY)
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @UserSyncJobId BIGINT;
	DECLARE @PendingStatusId INT = 26;

	INSERT INTO Enterprise.UserSyncJob
	    (
	        PersonaId,
	        StatusTypeId
	    )
	VALUES
	    (
	        @PersonaId,     
	        @PendingStatusId
	    )
	SELECT @UserSyncJobId = SCOPE_IDENTITY()

	INSERT INTO Enterprise.UserSyncJobTask
	    (
	        UserSyncJobId,
	        StatusTypeId,
	        Source
	    )
		SELECT @UserSyncJobId, @PendingStatusId, Source
		FROM @Sources

	SELECT 
		UserSyncJobTaskId
	FROM 
		Enterprise.UserSyncJobTask
	WHERE	
		UserSyncJobId = @UserSyncJobId
END;
GO
