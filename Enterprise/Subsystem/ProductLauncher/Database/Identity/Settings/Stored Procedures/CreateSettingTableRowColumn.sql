CREATE PROCEDURE [Settings].[CreateSettingTableRowColumn] (
	@PartyId bigint,
	@SettingTableId smallint,
	@editable tinyint = 1,
	@deletable tinyint = 1,
	@JsonUnifiedSettings nvarchar(max),
	@ModifiedBy bigint)
AS
BEGIN
	BEGIN TRY
		DECLARE @settings TABLE (
			Id int identity,
			ColumnName varchar(200),
			ColumnValue varchar(100),
			Editable bit)

		Insert Into @settings(ColumnName,ColumnValue,Editable)
		SELECT	Name,Value,Editable
		FROM	OPENJSON (@JsonUnifiedSettings)
		WITH (	Name varchar(max) '$.Name',
				Value varchar(max) '$.Value',
				Editable bit '$.Editable')
		WHERE	ISJSON(@JsonUnifiedSettings) > 0

		Declare @tableRowId bigint
		Declare @sequence smallint;
		IF (Select COUNT(*) From @settings) > 0
		BEGIN			
			IF NOT EXISTS (Select 1 From [Settings].[SettingTableRow] Where SettingTableId = @SettingTableId )
			BEGIN			
				INSERT INTO [Settings].[SettingTableRow](SettingTableId,Editable,Deletable,
							IsActive,ModifiedBy,CreatedDate)
				Select @SettingTableId,@editable,@deletable,1,
				@ModifiedBy,GETUTCDATE()

				set @tableRowId = SCOPE_IDENTITY();
			END
			Else
			BEGIN
				Select @tableRowId = SettingTableRowId 
				From [Settings].[SettingTableRow] Where SettingTableId = @SettingTableId
			END
			Select @sequence = COUNT(*) 
			From [Settings].[SettingTableColumn] 
			Where SettingTableRowId = @tableRowId

			Insert Into @settings(ColumnName,ColumnValue,Editable)
			Select 'Sequence',@sequence+1,0

			declare @MAX_ID INT
			declare @Current_ID INT = 1
			declare @columnName varchar(200)
			Declare @columnValue varchar(100)
			Declare @editablerow bit

			select @MAX_ID = max(Id) from @settings

			WHILE @Current_ID <= @MAX_ID
			begin
				select @columnName = ColumnName,
					   @columnValue = ColumnValue,
					   @editable = Editable
					from @settings where Id = @Current_ID			
			
				IF ((NULLIF(LTRIM(RTRIM(@columnName)), '') IS NOT NULL) AND
					(NULLIF(LTRIM(RTRIM(@columnValue)), '') IS NOT NULL))
				BEGIN
					INSERT INTO [Settings].[SettingTableColumn](SettingTableRowId,TableColumnName,
							TableColumnValue,ModifiedBy,CreatedDate)
					SELECT @tableRowId,@columnName,@columnValue,@ModifiedBy,GETUTCDATE()
				END
				set @Current_ID = @Current_ID + 1
			end
		END
		
		SELECT	COUNT(Id) AS Id,'' AS ErrorMessage
		FROM @settings
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