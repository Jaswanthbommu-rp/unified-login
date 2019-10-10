----Role Post-Migration Activity

declare @rolesourcecount int
declare @rolebackupcount int
declare @rightsourcecount int
declare @rightbackupcount int
declare @statustypeid int

--SELECT @RoleSourceCount = COUNT(*)   FROM Enterprise.Role 
--SELECT @RoleBackupCount = Count(*) FROM Enterprise.Role_Backup

--SELECT @StatusTypeId = ST.StatusTypeid
--FROM Enterprise.StatusTypeCategoryType STCT
--     JOIN Enterprise.StatusTypeCategory STC 
--	   ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
--     JOIN Enterprise.StatusTypeCategoryClassification STCC 
--	   ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
--     JOIN Enterprise.StatusType ST 
--	   ON ST.StatusTypeId = STCC.StatusTypeId
--WHERE STC.Name = 'Role Type'
--    AND ST.Name = 'Default'


--IF @RoleBackupCount  = @RoleSourceCount
--BEGIN
--	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.tables WHERE TABLE_NAME = 'Role_backup' AND TABLE_SCHEMA = 'Enterprise') AND (@RoleSourceCount = @RoleBackupCount)
--	BEGIN
--		IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.columns WHERE TABLE_NAME = 'Role' AND TABLE_SCHEMA = 'Enterprise' AND COLUMN_NAME = 'RoleValueTypeId')
--		BEGIN
--		   INSERT INTO Enterprise.RoleValueType (Value, Description, StatusTypeId)
--		   SELECT DISTINCT value, '', @StatusTypeId FROM Enterprise.Role_Backup
--		END
--	END
--END

----Update ROLE table with RoleValueTypeId
--UPDATE a
--    SET a.RoleValueTypeId = c.RoleValueTypeId
--    FROM Enterprise.Role a
--	   INNER JOIN Enterprise.Role_Backup b
--	   on a.roleid = b.roleid
--	   inner join Enterprise.RoleValueType c
--		  ON b.value = c.value


EXEC sys.sp_updateextendedproperty @name=N'Build', @value='21'