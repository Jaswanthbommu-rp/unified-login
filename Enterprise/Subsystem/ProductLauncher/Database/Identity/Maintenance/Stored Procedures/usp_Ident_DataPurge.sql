CREATE PROCEDURE [Maintenance].[usp_Ident_DataPurge] @v_programname  NVARCHAR(20),@p_debug TINYINT = 0
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE --@v_programname nvarchar(20) = 'PUR_batch'
			@v_schemaname nvarchar(20)
           ,@v_tablename nvarchar(50)
           ,@v_deletedrecords int = 0
           ,@v_backeduprecords int = 0 --Keep track of rows backed up.
           ,@v_statrecords int = 0
           ,@v_action nvarchar(100)    --Current ACTION being performed, used to provide more info for a WHEN OTHERS Exception Error Message
           ,@v_errmsg nvarchar(100)    --Specific error messages
           ,@v_sqlerrm nvarchar(100)   --Hold a portion of the message from SQLERRM function
           ,@v_sql nvarchar(max)
           ,@v_list nvarchar(max)
           ,@v_count int
           ,@v_row int
           ,@v_status nvarchar(2000)
           ,@pbv_jobstarttime datetime2(6)
           ,@v_msgtime datetime2(6)
           ,@v_continue tinyint = 1;

    DECLARE @jobhist TABLE (jobhistid int identity(1,1) primary key
                           ,jobprogramname nvarchar(20)
						   ,schemaname NVARCHAR(20)
                           ,tablename nvarchar(50)
                           ,deletedrecords int
                           ,jobstatus nvarchar(2000)
                           ,msgtime datetime2(6) default SYSDATETIME());
    
    DECLARE c_purge CURSOR LOCAL FAST_FORWARD
        FOR SELECT schemaname
				  ,tablename
                  ,columnname
                  ,hst_tablename
                  ,programname
                  ,backupflag
                  ,purgeflag
                  ,commitpoint
                  ,DATEADD(dd,-retentiondays,SYSUTCDATETIME()) as purgeenddate
              FROM [Maintenance].purgeconfigparams
             WHERE programname = @v_programname
             ORDER BY purgeid ASC;

    SET @pbv_jobstarttime = GETDATE(); --Capture the start of the job (this is also the start of a process (i.e. Backup or Purge))
    INSERT INTO @jobhist (jobprogramname,tablename,deletedrecords,jobstatus) VALUES (@v_programname,' ',0,'Started');
    BEGIN TRY
      EXECUTE [Maintenance].validate_purge @v_programname; 
    END TRY
    BEGIN CATCH
      INSERT INTO @jobhist (jobprogramname,tablename,deletedrecords,jobstatus) VALUES (@v_programname,'',@v_backeduprecords,ERROR_MESSAGE());
      SET @v_continue = 0;
    END CATCH;

    DECLARE @rec$schemaname nvarchar(20)
		   ,@rec$tablename nvarchar(50)
           ,@rec$columnname nvarchar(30)
           ,@rec$hst_tablename nvarchar(50)
           ,@rec$programname nvarchar(100)
           ,@rec$backupflag nchar(1)
           ,@rec$purgeflag nchar(1)
           ,@rec$commitpoint int
           ,@rec$purgeenddate datetime2(6);

    OPEN c_purge;
    WHILE 1 = 1 AND @v_continue = 1
      BEGIN
        FETCH c_purge INTO @rec$schemaname
						  ,@rec$tablename
                          ,@rec$columnname
                          ,@rec$hst_tablename
                          ,@rec$programname
                          ,@rec$backupflag
                          ,@rec$purgeflag
                          ,@rec$commitpoint
                          ,@rec$purgeenddate;

        IF @@fetch_status = -1
          BEGIN
            BREAK;
          END;

        SET @v_backeduprecords = 0;
        
        IF @rec$backupflag = 'Y' AND @v_continue = 1
          BEGIN
              SET @pbv_jobstarttime = GETDATE(); --Capture the start of the job (this is also the start of a process (i.e. Backup or Purge))

              BEGIN TRY
			    IF [Maintenance].objectexists(@rec$hst_tablename,'TABLE') != 0
					 BEGIN
						PRINT 'BACKUP Table'+ @rec$hst_tablename +'ALREADY EXISTS'
					 END;
					 ELSE
					 BEGIN
						EXECUTE [Maintenance].CreateHistTable @rec$tablename, @rec$hst_tablename, 'primary', 1
					END;
                --EXECUTE dbo.validate_hist @rec$tablename,@rec$hst_tablename;
              END TRY
              BEGIN CATCH
                INSERT INTO @jobhist (jobprogramname,tablename,deletedrecords,jobstatus) VALUES (@rec$programname,@rec$tablename,@v_backeduprecords,ERROR_MESSAGE());
                SET @v_continue = 0;
              END CATCH;

              IF @v_continue = 0
                BEGIN
                  BREAK;
                END;
                
              SELECT @v_list =
                     COALESCE(@v_list + ',','')
                   + c.column_name
                     FROM information_schema.columns c
                     WHERE c.table_name
                           = 
                           @rec$tablename;

              SET @v_sql =
                     'INSERT INTO '+@rec$schemaname+'.'
                   + @rec$hst_tablename
                   + '('
                   + @v_list
                   + ') (SELECT '
                   + @v_list
                   + ' FROM dbo.'
                   + @rec$tablename
                   + ' WHERE ' + @rec$columnname + ' < '''
                   + CONVERT(nvarchar(10),@rec$purgeenddate)
                   + ''')';
              EXECUTE sp_executesql @v_sql;
              SET @v_backeduprecords =
                     @v_backeduprecords
                   + @@rowcount;
              INSERT INTO @jobhist (jobprogramname,tablename,deletedrecords,jobstatus) VALUES (@rec$programname,@rec$tablename,@v_backeduprecords,'Backup completed');
          END;

        IF @rec$purgeflag = 'Y' AND @v_continue = 1
          BEGIN
              SET @pbv_jobstarttime = GETDATE(); --Capture the start of the job (this is also the start of a process (i.e. Backup or Purge))
              SET @v_deletedrecords = 0;
              BEGIN
                SET @v_sql =
                       'DELETE TOP ('
                     + CONVERT(nvarchar,@rec$commitpoint)
                     + ') FROM '+@rec$schemaname+'.'
                     + @rec$tablename
                     + ' WHERE ' + @rec$columnname + ' < '''
                     + CONVERT(nvarchar(10),@rec$purgeenddate)
                     + '''';
                WHILE 1 = 1 and @v_continue = 1
                  BEGIN 
                    BEGIN TRY
                      BEGIN TRANSACTION;
					  --PRINT @v_sql;
                      EXECUTE sp_executesql @v_sql;
                      SET @v_count = @@rowcount;
                      SET @v_deletedrecords = @v_deletedrecords + @v_count;
                      COMMIT;
                      IF @v_count = 0
                        BREAK; --EXITS THE PURGE LOOP
                    END TRY
                    BEGIN CATCH
                      INSERT INTO @jobhist (jobprogramname,tablename,deletedrecords,jobstatus) VALUES (@rec$programname,@rec$tablename,@v_deletedrecords,ERROR_MESSAGE());
                      SET @v_continue = 0;
                    END CATCH
                  END;
            END;
  
            INSERT INTO @jobhist (jobprogramname,tablename,deletedrecords,jobstatus) VALUES (@v_programname,@rec$tablename,@v_deletedrecords,'Purge completed');
          END;
      END;
    CLOSE c_purge;
    DEALLOCATE c_purge;
    
    IF @v_continue = 1
      INSERT INTO @jobhist (jobprogramname,tablename,deletedrecords,jobstatus) VALUES (@v_programname,' ',0,'Completed');
    
    -- Now save the purgejobhist rows
    SELECT @v_row = MIN(jobhistid) FROM @jobhist;
    WHILE (@v_row IS NOT NULL)
      BEGIN
        SELECT @v_tablename = tablename, @v_deletedrecords = deletedrecords, @v_status = jobstatus, @v_msgtime = msgtime FROM @jobhist WHERE jobhistid = @v_row;
        IF @@ROWCOUNT = 1
          BEGIN
            EXEC [Maintenance].[InsertPurgeJobHist] @v_programname, @v_tablename, @v_deletedrecords, @v_status, @pbv_jobstarttime, @v_msgtime;
            DELETE @jobhist WHERE jobhistid = @v_row;
          END;
        SELECT @v_row = MIN(jobhistid) FROM @jobhist;
      END;       

    IF @v_continue = 0
      THROW 59999, 'Unexpected error in purge.  See PurgeJobHist for details', 1;
END;

GO
