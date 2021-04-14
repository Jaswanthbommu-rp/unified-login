CREATE PROCEDURE [Settings].[DeleteSettingTableRowColumn] (
	@PartyId bigint,
	@JsonUnifiedSettings nvarchar(max),
	@ModifiedBy bigint)
AS
BEGIN
	BEGIN TRY
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
			Update [Settings].[SettingTableRow] SET [IsActive] = 0,
													[ModifiedBy] = @ModifiedBy,
													[UpdatedDate] = GETUTCDATE()
			Where [SettingTableRowId] IN (
				SELECT CONVERT(bigint,ColumnValue) From @settings 
				Where ColumnName = 'TableRowId')

			Set @id = 1;
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