USE [msdb]
GO


DECLARE @jobId BINARY(16)
DECLARE @ArchiveJobName VARCHAR(256) = 'AuditDBV2_DataArchiving_Job'
DECLARE @schedule_id INT
DECLARE @ServerName VARCHAR(128)

SELECT @ServerName = CONVERT(VARCHAR(128) ,  SERVERPROPERTY('servername') )

--if job exists delete and re run the below script
SELECT @jobId = job_id FROM msdb.dbo.sysjobs WHERE ([name] = @ArchiveJobName)

IF (@jobId IS NOT NULL)
BEGIN 

	EXEC msdb.dbo.sp_delete_job @jobId 

END 


EXEC  msdb.dbo.sp_add_job @job_name=@ArchiveJobName, 
		@enabled=1, 
		@notify_level_eventlog=0, 
		@notify_level_email=2, 
		@notify_level_page=2, 
		@delete_level=0, 
		@category_name=N'Database Maintenance'  
			   

EXEC msdb.dbo.sp_add_jobstep @job_name=@ArchiveJobName, @step_name=N'DataArchiving_Proc', 
		@step_id=1, 
		@cmdexec_success_code=0, 
		@on_success_action=1, 
		@on_fail_action=2, 
		@retry_attempts=0, 
		@retry_interval=0, 
		@os_run_priority=0, @subsystem=N'TSQL', 
		@command=N'EXEC Logging.Activity_Archive', 
		@database_name=N'AuditDBV2', 
		@flags=0


EXEC msdb.dbo.sp_add_jobschedule @job_name=@ArchiveJobName, @name=@ArchiveJobName, 
		@enabled=1, 
		@freq_type=4, 
		@freq_interval=1, 
		@freq_subday_type=1, 
		@freq_subday_interval=0, 
		@freq_relative_interval=0, 
		@freq_recurrence_factor=1, 
		@active_start_date=20210714, 
		@active_end_date=99991231, 
		@active_start_time=3000, 
		@active_end_time=235959

EXEC msdb.dbo.sp_add_jobserver @job_name = @ArchiveJobName,  @server_name = @ServerName

