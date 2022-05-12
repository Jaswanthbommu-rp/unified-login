CREATE PROCEDURE [Enterprise].[CreateUserSyncJob_V2]
(@PersonaId	BIGINT,
 @EditorPersonaId	BIGINT = NULL,
 @JobTypeId tinyint,
 @Sources	[Enterprise].[SourceList] READONLY,
 @GraceHours INT = 1
 )
AS
BEGIN
SET NOCOUNT ON;
	DECLARE @UserSyncJobId BIGINT = -1;
	DECLARE @PendingStatusId INT = 2;

	--filter source list so that only sources that are not already in pending status (within @GraceHours hours) get sync jobs created
	DECLARE @SourcesToSync [Enterprise].[SourceList];
	INSERT INTO @SourcesToSync
	SELECT Source FROM @Sources 
	EXCEPT
	SELECT DISTINCT sjt.Source
	FROM 
		Enterprise.UserSyncJobTask_V2 sjt JOIN
		Enterprise.UserSyncJob_V2 sj ON sjt.UserSyncJobId = sj.UserSyncJobId AND 
		sj.UserPersonaId = @PersonaId AND 
		sjt.StatusTypeId = @PendingStatusId AND 
		sj.UserSyncJobTypeId = @JobTypeId AND
		sjt.CreatedDate > DATEADD(hh,-ABS(@GraceHours),GETUTCDATE())

	IF (@@ROWCOUNT > 0)
	BEGIN
		INSERT INTO Enterprise.UserSyncJob_V2
			(
				UserPersonaId,
				EditorUserPersonaId,
				StatusTypeId,
				UserSyncJobTypeId,
				CreatedDate
			)
		VALUES
			(
				@PersonaId,
				@EditorPersonaId,
				@PendingStatusId,
				@JobTypeId,
				GETUTCDATE()
			)
		SELECT @UserSyncJobId = SCOPE_IDENTITY()

		INSERT INTO Enterprise.UserSyncJobTask_V2
			(
				UserSyncJobId,
				StatusTypeId,
				Source,
				CreatedDate
			)
		SELECT @UserSyncJobId, @PendingStatusId, Source, GETUTCDATE() FROM @SourcesToSync
	END

	SELECT 
		UserSyncJobTaskId
	FROM 
		Enterprise.UserSyncJobTask_V2
	WHERE	
		UserSyncJobId = @UserSyncJobId
END;
GO
