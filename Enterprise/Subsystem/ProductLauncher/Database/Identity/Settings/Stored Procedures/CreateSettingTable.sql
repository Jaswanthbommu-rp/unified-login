CREATE PROCEDURE [Settings].[CreateSettingTable] (
	@PartyId bigint,
	@Category varchar(100),
	@Name nvarchar(100),
	@ModifiedBy bigint)
AS
BEGIN
	Declare @SettingCategoryTypeId smallint
	Declare @SettingTableId bigint = NULL
	SELECT @SettingCategoryTypeId = SettingCategoryTypeId
	FROM [Settings].[SettingCategoryType] Where [Name] = @Category

	IF (@SettingCategoryTypeId IS NULL)
	BEGIN
		INSERT INTO [Settings].[SettingCategoryType] ([Name])
		SELECT @Category

		SET @SettingCategoryTypeId = SCOPE_IDENTITY();
	END
	
	Select @SettingTableId = SettingTableId From  [Settings].[SettingTable] 
	Where PartyId = @partyid 
	AND SettingCategoryTypeId = @SettingCategoryTypeId
	AND TableName = @Name

	IF (@SettingTableId IS NULL)
	BEGIN
		INSERT INTO [Settings].[SettingTable]([SettingCategoryTypeId],[PartyId],
				[TableName],[ModifiedBy],[CreatedDate])
		Select @SettingCategoryTypeId,@partyid,@Name,@ModifiedBy,GETUTCDATE()

		set @SettingTableId = SCOPE_IDENTITY();
	END	

	Select @SettingTableId
END