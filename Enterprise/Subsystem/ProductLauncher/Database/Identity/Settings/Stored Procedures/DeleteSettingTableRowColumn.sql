CREATE PROCEDURE [Settings].[DeleteSettingTableRowColumn] (
	@PartyId bigint,
	@JsonUnifiedSettings nvarchar(max),
	@ModifiedBy bigint)
AS
BEGIN
	BEGIN TRY
		DECLARE @settingcolumns TABLE (
			SettingTableColumnId bigint,
			SettingTableRowId bigint,			
			ColumnSequence int
		)

		DECLARE @settings TABLE (
			Id int identity,
			ColumnName varchar(200),
			ColumnValue varchar(100))
		Declare @id smallint = 0;
		Insert Into @settings(ColumnName,ColumnValue)
		SELECT	Name,Value
		FROM	OPENJSON (@JsonUnifiedSettings)
		WITH (	Name varchar(max) '$.Name',
				Value varchar(max) '$.Value')
		WHERE	ISJSON(@JsonUnifiedSettings) > 0

		IF EXISTS (SELECT 1 From @settings Where ColumnName = 'TableRowId')
		BEGIN
			--delete data from relevent tables
			DELETE FROM Settings.SettingTableRowValue
			Where SettingTableRowId IN (
				SELECT CONVERT(bigint,ColumnValue) From @settings 
				Where ColumnName = 'TableRowId')

			DELETE FROM Settings.SettingTableColumn
			Where SettingTableRowId IN (
				SELECT CONVERT(bigint,ColumnValue) From @settings 
				Where ColumnName = 'TableRowId')

			DELETE FROM Settings.SettingTableRow
			Where SettingTableRowId IN (
				SELECT CONVERT(bigint,ColumnValue) From @settings 
				Where ColumnName = 'TableRowId')
			
			--after delete row , re-arrainge sequence columns data
			Insert Into @settingcolumns
			Select stc.SettingTableColumnId,stc.SettingTableRowId,CONVERT(int,TableColumnValue)
			From Settings.SettingTableColumn stc
			JOIN Settings.SettingTableRow sr ON
				stc.SettingTableRowId = sr.SettingTableRowId
			JOIN Settings.SettingTable st ON
				sr.SettingTableId = st.SettingTableId
			WHERE TableColumnName = 'Sequence'
			AND st.PartyId = @PartyId

			IF EXISTS (Select 1 FROM @settingcolumns)
			BEGIN
				Update @settingcolumns Set ColumnSequence = ColumnSequence -1
				Update @settingcolumns Set ColumnSequence = 1 Where ColumnSequence = 0

				Update stc SET stc.TableColumnValue = sc.ColumnSequence
				From Settings.SettingTableColumn stc
				JOIN @settingcolumns sc ON
					stc.SettingTableColumnId = sc.SettingTableColumnId And
					stc.SettingTableRowId = sc.SettingTableRowId
			END
			Set @id = 1;
		END
		ELSE IF EXISTS(SELECT * FROM @settings WHERE ColumnName = 'enableluminaproduct')
		BEGIN
			DECLARE @partyTableRows table(TableRowId BIGINT, SettingTableRowId BIGINT, TableColumnName NVARCHAR(200), TableColumnValue NVARCHAR(200))  
			INSERT INTO @partyTableRows  
			Select SR.SettingTableRowId, STC.SettingTableRowId,STC.TableColumnName,stc.TableColumnValue
			From  [Settings].[SettingTableRow] SR  
			Join [Settings].[SettingTable] ST ON  
			SR.[SettingTableId] = ST.[SettingTableId] 
			JOIN settings.SettingTableColumn STC ON STC.SettingTableRowId = SR.SettingTableRowId
			Where ST.PartyId = @PartyId  

			Declare @tableRowId BIGINT
			SELECT @tableRowId = p.SettingTableRowId FROM @settings s
			JOIN @partyTableRows p ON s.ColumnName = p.TableColumnName AND s.ColumnValue = p.TableColumnValue
			WHERE s.ColumnName = 'enableluminaproduct'

			DELETE settings.[SettingTableColumn] WHERE SettingTableRowId = @tableRowId
			DELETE settings.SettingTableRowValue WHERE SettingTableRowId = @tableRowId
			DELETE settings.SettingTableRow WHERE SettingTableRowId = @tableRowId

		END
		SELECT	COUNT(@id) AS Id,'' AS ErrorMessage		
	END TRY
	BEGIN CATCH
        DECLARE @ErrorLogID int;
        EXEC dbo.LogError @ErrorLogID = @ErrorLogID OUTPUT;

        SELECT	0 AS Id,
					ErrorMessage
        FROM	dbo.ErrorLog
        WHERE	ErrorLogID = @ErrorLogID;
	END CATCH
END