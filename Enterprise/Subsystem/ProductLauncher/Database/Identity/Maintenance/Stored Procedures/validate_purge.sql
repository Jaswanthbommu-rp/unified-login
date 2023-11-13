CREATE PROCEDURE [Maintenance].[Validate_Purge] @p_purgetype nvarchar(20)
AS
BEGIN
    DECLARE @v_minretentionhours int = 8
           ,@v_cnt int
           ,@v_errmsg nvarchar(100)
           ,@v_sqlerrm nvarchar(100)
           ,@v_table nvarchar(255)
           ,@pbv_jobstarttime datetime2(6);

    SET @pbv_jobstarttime = SYSUTCDATETIME();

    BEGIN
        DECLARE @rec$programname nvarchar(20)
               ,@rec$purgeid numeric(38,0)
               ,@rec$tablename nvarchar(50)
               ,@rec$columnname nvarchar(50)
               ,@rec$hst_tablename nvarchar(50)
               ,@rec$retentionhours numeric(38,0)
               ,@rec$commitpoint numeric(38,0)
               ,@rec$purgeflag nchar(1)
               ,@rec$backupflag nchar(1);

        DECLARE db_implicit_cursor_for_rec CURSOR LOCAL FAST_FORWARD
            FOR SELECT purgeconfigparams.programname
                      ,purgeconfigparams.purgeid
                      ,purgeconfigparams.tablename
                      ,purgeconfigparams.columnname
                      ,purgeconfigparams.hst_tablename
                      ,purgeconfigparams.retentiondays
                      ,purgeconfigparams.commitpoint
                      ,purgeconfigparams.purgeflag
                      ,purgeconfigparams.backupflag
                       FROM [Maintenance].purgeconfigparams
                       WHERE purgeconfigparams.programname = @p_purgetype
                         AND tablename NOT LIKE 'ERR$%' -- These tables do not exist on SQL Server


        OPEN db_implicit_cursor_for_rec;
        WHILE 1 = 1
            BEGIN
                FETCH db_implicit_cursor_for_rec INTO @rec$programname,@rec$purgeid,@rec$tablename,@rec$columnname,@rec$hst_tablename,@rec$retentionhours,@rec$commitpoint,@rec$purgeflag,@rec$backupflag;

                IF @@fetch_status = -1
                    BREAK;

                SET @v_table = @rec$tablename;

                IF [Maintenance].[ObjectExists](@rec$tablename,'TABLE') = 0
                    BEGIN
                        SET @v_errmsg = 'TABLE '+ @rec$tablename +' DOES NOT EXIST';
                        THROW 59999, @v_errmsg, 1;
                    END;

                IF UPPER(@rec$purgeflag) = 'Y'
                  AND (@rec$retentionhours IS NULL
                    OR ABS(@rec$retentionhours) < @v_minretentionhours)
                    BEGIN
                        SET @v_errmsg =
                               'PURGECONFIGPARAMS.retentionhours must be defined and greater than '
                             + ISNULL(CAST(@v_minretentionhours AS nvarchar(max)),'');
                        THROW 59999, @v_errmsg, 1;
                    END;

                IF @rec$purgeid IS NULL
                    BEGIN
                        SET @v_errmsg = 'PURGECONFIGPARAMS.PURGEID must be defined';
                        THROW 59999, @v_errmsg, 1;
                    END;
                ELSE
                    BEGIN
                        SELECT @v_cnt = COUNT(1)
                               FROM [Maintenance].purgeconfigparams
                               WHERE purgeconfigparams.programname
                                     = 
                                     @rec$programname
                                 AND purgeconfigparams.purgeid
                                     = 
                                     @rec$purgeid;

                        IF @v_cnt > 1
                            BEGIN
                                SET @v_errmsg = 'PURGECONFIGPARAMS.PURGEID must be unique for each programname';
                                THROW 59999, @v_errmsg, 1;
                            END;
                    END;

                IF @rec$backupflag IS NULL
                   OR UPPER(@rec$backupflag) <> 'Y'
                   AND UPPER(@rec$backupflag) <> 'N'
                    BEGIN
                        SET @v_errmsg = 'PURGECONFIGPARAMS.BACKUPFLAG must be set to Y or N';
                        THROW 59999, @v_errmsg, 1;
                    END;

                IF @rec$purgeflag IS NULL
                   OR UPPER(@rec$purgeflag) <> 'Y'
                   AND UPPER(@rec$purgeflag) <> 'N'
                    BEGIN
                        SET @v_errmsg = 'PURGECONFIGPARAMS.PURGEFLAG must be set to Y or N';
                        THROW 59999, @v_errmsg, 1;
                    END;

                IF UPPER(@rec$purgeflag) = 'Y'
                   AND (@rec$commitpoint IS NULL
                    OR @rec$commitpoint < 1)
                    BEGIN
                        SET @v_errmsg = 'PURGECONFIGPARAMS.COMMITPOINT must be defined and greater than 0';
                        THROW 59999, @v_errmsg, 1;
                    END;

                IF UPPER(@rec$backupflag) = 'Y'
                   AND @rec$hst_tablename IS NULL
                    BEGIN
                        SET @v_errmsg = 'HST_TABLENAME must be defined if BACKUPFLAG = Y';
                        THROW 59999, @v_errmsg, 1;
                    END;
            END;

        CLOSE db_implicit_cursor_for_rec;
        DEALLOCATE db_implicit_cursor_for_rec;

    END;
END;