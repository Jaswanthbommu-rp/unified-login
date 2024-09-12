GO

DECLARE @jobId BINARY(16)
DECLARE @ArchiveJobName VARCHAR(256) = 'UL_Identity_DataCleanup'
	
--if job exists delete and re run the below script
SELECT @jobId = job_id FROM msdb.dbo.sysjobs WHERE ([name] = @ArchiveJobName)

IF (@jobId IS NOT NULL)
BEGIN 
	EXEC msdb.dbo.sp_delete_job @jobId 
END 
GO

BEGIN TRANSACTION
	DECLARE @ReturnCode INT
	DECLARE @DBName VARCHAR(20) = 'UPSandbox';
	Declare @ServerName SYSNAME = @@SERVERNAME
	DECLARE @jobId BINARY(16)
	IF @ServerName IN ('RCDUSODBSQL001')  --DEV
	BEGIN
		SET @DBName = 'UPDEV';
	END
	IF @ServerName IN ('rctusodbsql001') --QA
	BEGIN
		SET @DBName = 'UPQA';
	END
	IF @ServerName IN ('rcausodbsql001') --SAT
	BEGIN
		SET @DBName = 'UPSAT';
	END
	IF @ServerName IN ('RCDUSODBSQL001A')  --UPLOAD
	BEGIN
		SET @DBName = 'UPLOAD';
	END
	IF @ServerName IN ('RCTUSODBSQL001A','RCTUSODBSQL001B') --UAT
	BEGIN
		SET @DBName = 'UPUAT';
	END
	IF @ServerName IN ('RCIUSODBSQL002') --PREPROD
	BEGIN
		SET @DBName = 'UPPREPROD';
	END
	IF @ServerName IN ('RCVGBKDBSQL001') --DEMO
	BEGIN
		SET @DBName = 'UPDEMO';
	END
	IF @ServerName IN ('RCTUSODBTUL001') --TRAINING
	BEGIN
		SET @DBName = 'UPTRAINING';
	END
	IF @ServerName IN ('RCPGBKDBSQL005A', 'RCPGBKDBSQL005B') --PROD
	BEGIN
		SET @DBName = 'IDENTITY';
	END
	IF @ServerName IN ('reagbkdbsql001') --EUSAT
	BEGIN
		SET @DBName = 'UPEUSAT';
	END
	IF @ServerName IN ('repgbkdbsql001a','repgbkdbsql001b', 'gnpgbkdbsql001a', 'gnpgbkdbsql001b') --EUPROD
	BEGIN
		SET @DBName = 'UPEUPROD';
	END
	IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = @DBName)
	BEGIN 
		SET @DBName = 'UPSandbox';
	END
	SELECT @ReturnCode = 0

	/****** Object:  JobCategory [[Uncategorized (Local)]]    Script Date: 10/27/2023 6:59:45 PM ******/
	IF NOT EXISTS (SELECT name FROM msdb.dbo.syscategories WHERE name=N'[Uncategorized (Local)]' AND category_class=1)
	BEGIN
		EXEC @ReturnCode = msdb.dbo.sp_add_category @class=N'JOB', @type=N'LOCAL', @name=N'[Uncategorized (Local)]'
		IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
	END

	EXEC @ReturnCode =  msdb.dbo.sp_add_job @job_name=N'UL_Identity_DataCleanup', 
			@enabled=1, 
			@notify_level_eventlog=0, 
			@notify_level_email=0, 
			@notify_level_netsend=0, 
			@notify_level_page=0, 
			@delete_level=0, 
			@description=N'No description available.', 
			@category_name=N'[Uncategorized (Local)]', 
			@owner_login_name=N'sa', @job_id = @jobId OUTPUT
	IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
		/****** Object:  Step [Cleanup]    Script Date: 10/27/2023 6:59:48 PM ******/
		EXEC @ReturnCode = msdb.dbo.sp_add_jobstep @job_id=@jobId, @step_name=N'Cleanup', 
				@step_id=1, 
				@cmdexec_success_code=0, 
				@on_success_action=1, 
				@on_success_step_id=0, 
				@on_fail_action=2, 
				@on_fail_step_id=0, 
				@retry_attempts=0, 
				@retry_interval=0, 
				@os_run_priority=0, @subsystem=N'TSQL', 
				@command=N'EXECUTE [Maintenance].[usp_Ident_DataPurge] ''PUR_Activity'',0
GO
EXECUTE [Maintenance].[usp_Ident_DataPurge] ''PUR_LOGS'',0
GO
EXECUTE [Maintenance].[usp_Ident_DataPurge] ''PUR_BATCH'',0
GO', 
				@database_name=@DBName, 
				@flags=0
	IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
		EXEC @ReturnCode = msdb.dbo.sp_update_job @job_id = @jobId, @start_step_id = 1
	IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
		EXEC @ReturnCode = msdb.dbo.sp_add_jobschedule @job_id=@jobId, @name=N'Cleanup_schedule', 
				@enabled=1, 
				@freq_type=4, 
				@freq_interval=1, 
				@freq_subday_type=1, 
				@freq_subday_interval=0, 
				@freq_relative_interval=0, 
				@freq_recurrence_factor=0, 
				@active_start_date=20231027, 
				@active_end_date=99991231, 
				@active_start_time=0, 
				@active_end_time=235959, 
				@schedule_uid=N'cb735952-5d4f-4029-b261-cd31bfcfa13c'
	IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
		EXEC @ReturnCode = msdb.dbo.sp_add_jobserver @job_id = @jobId, @server_name = N'(local)'
IF (@@ERROR <> 0 OR @ReturnCode <> 0) GOTO QuitWithRollback
COMMIT TRANSACTION
GOTO EndSave
QuitWithRollback:
    IF (@@TRANCOUNT > 0) ROLLBACK TRANSACTION
EndSave:

GO