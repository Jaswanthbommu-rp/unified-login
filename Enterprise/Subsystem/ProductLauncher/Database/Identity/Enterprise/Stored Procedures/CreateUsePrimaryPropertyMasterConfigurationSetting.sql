CREATE PROCEDURE [Enterprise].[CreateUsePrimaryPropertyMasterConfigurationSetting]
(
 @PartyId                 BIGINT,
 @Value                   NVARCHAR(4000),
 @CreatedBy				  BIGINT
)
AS
	BEGIN
     BEGIN TRY
		 DECLARE @mappingValue varchar(100) 
		 Declare @RightId bigint
		 DECLARE @SettingCategoryTypeId smallint
     
			SELECT @RightId = RightId from [Security].[Right] where RightName = 'PrimaryPropertyEnterpriseRole';
			SELECT @SettingCategoryTypeId =  SettingCategoryTypeId FROM [Settings].[SettingCategoryType] where [Name] = 'Company'
			if not exists (select 1 from Settings.OrganizationSettings where MappingName = 'PrimaryPropertyEnterpriseRole' and Partyid = @PartyId)
			Begin
				insert into Settings.OrganizationSettings(PartyId, SettingCategoryTypeId, MappingName, MappingValue, Editable, [Hidden], CreatedBy, CreatedDate, UpdatedDate)
				SELECT @PartyId, @SettingCategoryTypeId, 'PrimaryPropertyEnterpriseRole', @Value, 1, 0, @CreatedBy, GETUTCDATE(), NULL
			End
			else
			begin
				update Settings.OrganizationSettings
				SET MappingValue = @Value,
				UpdatedDate = GETUTCDATE()
				WHERE PartyId = @PartyId 
				AND SettingCategoryTypeId = @SettingCategoryTypeId
				AND MappingName = 'PrimaryPropertyEnterpriseRole'
			end
			SELECT @mappingValue = MappingValue FROM [Settings].[OrganizationSettings]
						WHERE MappingName = 'PrimaryPropertyEnterpriseRole'
						And PartyId = @PartyId
			 
			IF  @mappingValue IS NOT NULL AND @mappingValue = '1' AND NOT EXISTS (SELECT 1 FROM [Security].[OrganizationOverRideRight]
						WHERE RightId = @RightId 
						And OrgPartyId = @PartyId)
			BEGIN 
				INSERT INTO [Security].[OrganizationOverRideRight](RightId, OrgPartyId, VisibilityStatusId, CreatedBy, CreatedDate)
				SELECT @RightId, @PartyId, 9, @CreatedBy, GETUTCDATE()
			END
			ELSE
			BEGIN
				DELETE FROM [Security].[OrganizationOverRideRight]
				WHERE RightId = @RightId 
				AND OrgPartyId = @PartyId
			END		
			
			SELECT '' AS ErrorMessage;      
        END TRY
        BEGIN CATCH
            DECLARE @ErrorLogID INT;
            EXEC dbo.LogError
                @ErrorLogID = @ErrorLogID OUTPUT;
            SELECT 0 AS Id,
                    '' ErrorMessage
            FROM dbo.ErrorLog
            WHERE ErrorLogID = @ErrorLogID;
            ROLLBACK;
        END CATCH;
	END;