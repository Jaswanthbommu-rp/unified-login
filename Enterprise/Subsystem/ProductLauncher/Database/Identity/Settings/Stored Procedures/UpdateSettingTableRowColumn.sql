CREATE PROCEDURE [Settings].[UpdateSettingTableRowColumn] (
	@PartyId bigint,
	@JsonUnifiedSettings nvarchar(max),
	@ModifiedBy bigint)
AS
BEGIN
	BEGIN TRY
		Declare @partyTableRows table(
			TableRowId bigint)

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

		INSERT INTO @partyTableRows
		Select SettingTableRowId 
		From  [Settings].[SettingTableRow] SR
		Join [Settings].[SettingTable] ST ON
			SR.[SettingTableId] = ST.[SettingTableId]
		Where ST.PartyId = @PartyId
				
		Declare @tableRowId bigint
		Select @tableRowId = CONVERT(bigint,ColumnValue) From @settings 
		Where ColumnName = 'TableRowId'

		IF EXISTS (SELECT 1 From @partyTableRows Where TableRowId  = @tableRowId)
		BEGIN	
			declare @MAX_ID INT
			declare @Current_ID INT = 1
			declare @columnName varchar(200)
			Declare @columnValue varchar(100)
			
			select @MAX_ID = max(Id) from @settings

			WHILE @Current_ID <= @MAX_ID
			begin
				select @columnName = ColumnName,
					   @columnValue = ColumnValue
					from @settings where Id = @Current_ID			
			
				IF ((NULLIF(LTRIM(RTRIM(@columnName)), '') IS NOT NULL) AND
					(NULLIF(LTRIM(RTRIM(@columnValue)), '') IS NOT NULL))
				BEGIN
					Update [Settings].[SettingTableColumn] SET
						   TableColumnValue = @columnValue,
						   ModifiedBy = @ModifiedBy,
						   UpdatedDate = GETUTCDATE()
					Where SettingTableRowId = @tableRowId
					And TableColumnName = @columnName
							
					set @Current_ID = @Current_ID + 1
				END
			end
			set @id = @Current_ID
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