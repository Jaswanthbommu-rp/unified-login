print 'Inserts Complete.  Updating Statistics'

EXEC sys.sp_updatestats 'resample';