--Database cleanup script for User Activity Report
GO
IF NOT EXISTS (SELECT 1 FROM Maintenance.PurgeConfigParams WHERE ProgramName = 'PUR_USER_ACTIVITY')
BEGIN
	INSERT INTO Maintenance.PurgeConfigParams(ProgramName,PurgeId,SchemaName,TableName,ColumnName,Hst_TableName,RetentionDays,CommitPoint,PurgeFlag,BackupFlag)
	VALUES(N'PUR_USER_ACTIVITY',6,N'Enterprise',N'ProductLoginActivitybyUser','CreateDate',NULL,365,10000,'Y','N')
END

GO