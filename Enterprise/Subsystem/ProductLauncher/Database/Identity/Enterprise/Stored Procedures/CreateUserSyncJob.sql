CREATE PROCEDURE [Enterprise].[CreateUserSyncJob]
(@PersonaId	BIGINT,
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
		Enterprise.UserSyncJobTask sjt JOIN
		Enterprise.UserSyncJob sj ON sjt.UserSyncJobId = sj.UserSyncJobId AND 
		sj.PersonaId = @PersonaId AND 
		sjt.StatusTypeId = @PendingStatusId AND 
		sjt.CreatedDate > DATEADD(hh,-ABS(@GraceHours),GETUTCDATE())

	IF (@@ROWCOUNT > 0)
	BEGIN
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
		SELECT @UserSyncJobId, @PendingStatusId, Source FROM @SourcesToSync
	END

	SELECT 
		UserSyncJobTaskId
	FROM 
		Enterprise.UserSyncJobTask
	WHERE	
		UserSyncJobId = @UserSyncJobId
END;
GO
