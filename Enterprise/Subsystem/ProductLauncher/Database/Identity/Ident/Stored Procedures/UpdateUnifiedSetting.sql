Create PROCEDURE [Ident].[UpdateUnifiedSetting] (
	@PartyId bigint,
	@Category Varchar(50),
	@JsonUnifiedSettings nvarchar(max),
	@CreatedBy bigint)
AS
BEGIN
	BEGIN TRY
		DECLARE @settings TABLE (
			Id int identity,
			MappingName varchar(200),
			MappingValue varchar(100),
			Editable bit,
			Hidden bit
		)

		Insert Into @settings(MappingName,MappingValue,Editable,Hidden)
		SELECT	Name,Value,Editable,Hidden
		FROM	OPENJSON (@JsonUnifiedSettings)
		WITH (	Name varchar(max) '$.Name',
				Value varchar(max) '$.Value',
				Editable bit '$.Editable',
				Hidden bit '$.Hidden')
		WHERE	ISJSON(@JsonUnifiedSettings) > 0

		declare @MAX_ID INT
		declare @Current_ID INT = 1
		declare @mappingName varchar(200)
		Declare @mappingValue varchar(100)
		Declare @editable bit,@hidden bit
		Declare @categoryId int

		Select @categoryId = SettingCategoryTypeId 
		From [Ident].[SettingCategoryType] 
		Where Name = @Category

		select @MAX_ID = max(Id) from @settings

		WHILE @Current_ID <= @MAX_ID
		begin
			select @mappingName = MappingName,
				   @mappingValue = MappingValue,
				   @editable = Editable,@hidden = Hidden
				from @settings where Id = @Current_ID

			IF NOT EXISTS (SELECT 1 FROM [Ident].[OrganizationSettings]
							WHERE MappingName = @mappingName
							And PartyId = @PartyId
							And SettingCategoryTypeId = @categoryId)

			BEGIN
				INSERT INTO [Ident].[OrganizationSettings](PartyId,SettingCategoryTypeId,MappingName,
					MappingValue,Editable,Hidden,CreatedBy,CreatedDate)
				SELECT @PartyId,@categoryId,@mappingName,@mappingValue,@editable,@hidden,@CreatedBy,GETUTCDATE()
			END
			ELSE
			BEGIN
				UPDATE [Ident].[OrganizationSettings] SET MappingValue = @mappingValue,
						UpdatedDate = GETUTCDATE()
				WHERE MappingName = @mappingName
				And PartyId = @PartyId
				And SettingCategoryTypeId = @categoryId
			END
			set @Current_ID = @Current_ID + 1
		end
		SELECT	COUNT(Id) AS Id,
					'' AS ErrorMessage
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