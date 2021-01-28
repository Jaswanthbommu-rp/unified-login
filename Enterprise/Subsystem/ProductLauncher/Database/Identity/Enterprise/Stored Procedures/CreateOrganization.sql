CREATE PROCEDURE [Enterprise].[CreateOrganization] @OrganizationId   UNIQUEIDENTIFIER = NULL,
                                                  @OrganizationName NVARCHAR(150),
                                                  @TimeZone         NVARCHAR(200) NULL = 'Central Standard Time',
												  @OrganizationTypeId INT,
                                                  @OrganizationDomainId INT = 1
AS
     BEGIN
         DECLARE @IdentityProviderId INT;
         DECLARE @ContactMechanismId INT;
         DECLARE @MasterConfigurationId BIGINT;
         DECLARE @MasterConfigurationTypeId BIGINT;
         DECLARE @MasterSettingId INT;
         SELECT @IdentityProviderId = IdentityProviderTypeId,
                @ContactMechanismId = ContactMechanismId
         FROM Ident.IdentityProviderType
            WHERE Name = 'ID3';
         
         BEGIN TRY
             IF @OrganizationId IS NULL
                 BEGIN
                     SET @OrganizationId = NEWID();
                 END;
             BEGIN TRANSACTION;
             SET NOCOUNT ON;
             DECLARE @PartyId BIGINT;

             INSERT INTO [Enterprise].Party
				(RealPageId,
				 CreateDate
				)
             VALUES
				(@OrganizationId,
				 GETUTCDATE()
				);

             SET @PartyId = SCOPE_IDENTITY();

             INSERT INTO [Enterprise].Organization
				(PartyId,
				 Name,
				 IdentityProviderTypeId,
				 OrganizationTypeId,
                 OrganizationDomainId,
				 CreateDate
				)
             VALUES
				(@PartyId,
				 @OrganizationName,
				 @IdentityProviderId,
				 @OrganizationTypeId,
                 @OrganizationDomainId,
				 GETUTCDATE()
				);

             INSERT INTO Enterprise.PartyContactMechanism
				(PartyId,
				 ContactMechanismId,
				 FromDate
				)
             VALUES
				(@PartyId,
				 @ContactMechanismId,
				 GETUTCDATE()
				);

		/*
		Add default settings 
		1. Add the row to MasterConfiguration Table
		2. Configure default values (ThemeColor, TimeZone)
		*/

             SELECT @MasterConfigurationTypeId = MasterConfigurationTypeId
             FROM Enterprise.MasterConfigurationType
             WHERE Name = 'Organization';

      --Setup Time Zone
             SELECT @MasterSettingId = MasterSettingId
             FROM Enterprise.MasterSetting AS MS
                  INNER JOIN Enterprise.MasterSettingType AS MST ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
             WHERE MST.MasterConfigurationTypeId = @MasterConfigurationTypeId
                   AND MST.Name = 'TimeZone'
                   AND MS.Value = @TimeZone;
             IF NOT EXISTS
				(
					SELECT 1
					FROM Enterprise.MasterConfiguration
					WHERE MasterConfigurationTypeId = @MasterConfigurationTypeId
						  AND AttributeId = @PartyId
				)
                 BEGIN
                     INSERT INTO Enterprise.MasterConfiguration
						(MasterConfigurationTypeId,
						 AttributeId
						)
                     VALUES
						(@MasterConfigurationTypeId,
						 @PartyId
						);
                     SELECT @MasterConfigurationId = SCOPE_IDENTITY();
                 END;
             IF NOT EXISTS
				(
					SELECT 1
					FROM Enterprise.MasterConfigurationSetting
					WHERE MasterConfigurationId = @MasterConfigurationId
						  AND MasterSettingId = @MasterSettingId
				)
                 BEGIN
                     INSERT INTO Enterprise.MasterConfigurationSetting
						(MasterConfigurationId,
						 MasterSettingId
						)
                     VALUES
						(@MasterConfigurationId,
						 @MasterSettingId
						);
                 END;

	 --Setup ThemeColor
             SELECT @MasterSettingId = MasterSettingId
             FROM Enterprise.MasterSetting AS MS
                  INNER JOIN Enterprise.MasterSettingType AS MST ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
             WHERE MST.MasterConfigurationTypeId = @MasterConfigurationTypeId
                   AND MST.Name = 'ThemeColor'
                   AND MS.Value = 'Light';
             IF NOT EXISTS
				(
					SELECT 1
					FROM Enterprise.MasterConfigurationSetting
					WHERE MasterConfigurationId = @MasterConfigurationId
						  AND MasterSettingId = @MasterSettingId
				)
                 BEGIN
                     INSERT INTO Enterprise.MasterConfigurationSetting
						(MasterConfigurationId,
						 MasterSettingId
						)
                     VALUES
						(@MasterConfigurationId,
						 @MasterSettingId
						);
                 END;
             SET NOCOUNT OFF;
             COMMIT;
             SELECT @PartyId AS Id,
                    @OrganizationId AS RealPageId,
                    '' AS ErrorMessage;
         END TRY
         BEGIN CATCH
             ROLLBACK;
             DECLARE @ErrorLogID INT;
             EXEC dbo.LogError
                  @ErrorLogID = @ErrorLogID OUTPUT;
             SELECT 0 AS Id,
                    ErrorMessage
             FROM [dbo].ErrorLog
             WHERE ErrorLogID = @ErrorLogID;
         END CATCH;
     END;

