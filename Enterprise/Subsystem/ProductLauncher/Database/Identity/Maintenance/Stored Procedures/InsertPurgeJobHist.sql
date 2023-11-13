CREATE PROCEDURE [Maintenance].[InsertPurgeJobHist] @p_jobprogramname nvarchar(20)
                                              ,@p_tablename nvarchar(50)
                                              ,@p_deletedrecords int
                                              ,@p_jobstatus nvarchar(2000)
                                              ,@pbv_jobstarttime datetime2(6)
                                              ,@p_msgtime datetime2(6)
AS
BEGIN
    DECLARE @v_jobruntime int
           ,@v_tabname nvarchar(50);
    BEGIN
        SET NOCOUNT ON;

        SET @v_jobruntime = ABS(DATEDIFF(s,@p_msgtime,@pbv_jobstarttime));

        IF @p_tablename IS NULL
            BEGIN
                SET @v_tabname = 'None';
            END
        ELSE
            BEGIN
                SET @v_tabname = @p_tablename;
            END;

        INSERT INTO [Maintenance].[PurgeJobHist](jobstarttime
                                          ,jobprogramname
                                          ,tablename
                                          ,deletedrecords
                                          ,jobstatus
                                          ,jobruntime)
        VALUES(@pbv_jobstarttime
              ,@p_jobprogramname
              ,@v_tabname
              ,@p_deletedrecords
              ,@p_jobstatus
              ,@v_jobruntime);

    END;
END;