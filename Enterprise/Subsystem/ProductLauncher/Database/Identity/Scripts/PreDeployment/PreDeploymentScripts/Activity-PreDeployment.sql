

--IF EXISTS (SELECT 1 FROM Sys.tables WHERE NAME = 'Activity' and Schema_Name(schema_id) = 'Ident')
--AND NOT EXISTS(SELECT 1 FROM Sys.tables WHERE NAME = 'Activity_old' and Schema_Name(schema_id) = 'Ident')
--BEGIN
--	SELECT * INTO Ident.Activity_Old FROM Ident.Activity
--END


--IF EXISTS (select * from information_schema.columns WHERE Table_Schema = 'Ident' AND table_name = 'ActivityAttempts' AND column_name = 'ActivityId')
--AND NOT EXISTS(SELECT 1 FROM Sys.tables WHERE NAME = 'ActivityAttempts_old' and Schema_Name(schema_id) = 'Ident')
--BEGIN
--	SELECT * INTO Ident.ActivityAttempts_Old FROM Ident.ActivityAttempts
--	DELETE FROM Ident.ActivityAttempts
--END

--IF EXISTS (select * from information_schema.columns WHERE Table_Schema = 'Ident' AND table_name = 'ActivityToken' AND column_name = 'ActivityId')
--AND NOT EXISTS(SELECT 1 FROM Sys.tables WHERE NAME = 'ActivityToken_old' and Schema_Name(schema_id) = 'Ident')
--BEGIN
--	SELECT * INTO Ident.ActivityToken_Old FROM Ident.ActivityToken
--	DELETE FROM Ident.ActivityToken

--END

--IF EXISTS (select * from information_schema.columns WHERE Table_Schema = 'Ident' AND table_name = 'PasswordHistory' AND column_name = 'ActivityId')
--AND NOT EXISTS(SELECT 1 FROM Sys.tables WHERE NAME = 'PasswordHistory_Old' and Schema_Name(schema_id) = 'Ident')
--BEGIN
--	SELECT * INTO Ident.PasswordHistory_Old FROM Ident.PasswordHistory
--	DELETE FROM Ident.PasswordHistory

--END