--Right Post-Migration Activity

--IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.tables WHERE TABLE_NAME = 'Right_backup' AND TABLE_SCHEMA = 'Enterprise') 
--BEGIN
--	IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.columns WHERE TABLE_NAME = 'Right' AND TABLE_SCHEMA = 'Enterprise' AND COLUMN_NAME = 'RightValueTypeId')
--	BEGIN
--		INSERT INTO Enterprise.RightValueType (Value, Description)
--		SELECT DISTINCT value, '' FROM Enterprise.Right_Backup
--	END
--END



----Update RIGHT table with RightValueTypeId
--UPDATE a
--    SET a.RightValueTypeId = c.RightValueTypeId
--FROM Enterprise.[Right] a
--	   INNER JOIN Enterprise.Right_Backup b
--	   on a.Rightid = b.Rightid
--	   inner join Enterprise.RightValueType c
--		  ON b.value = c.value

----Update RIGHT Table with PartyId
--update a
--set a.partyid = b.partyid
--from enterprise.[right] a inner join enterprise.role b
--on a.roleid = b.roleid




SET @StatusTypeId = NULL
EXEC sys.sp_updateextendedproperty @name=N'Build', @value='22'