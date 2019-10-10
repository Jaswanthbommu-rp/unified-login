CREATE PROCEDURE [Enterprise].[UpdateMasterConfigurationSetting]
(@MasterConfigurationSettingId BIGINT        = NULL,
 @Value                        NVARCHAR(4000)
)
AS
     BEGIN
         DECLARE @ErrorLogID INT;
         IF @Value IS NULL
            OR @Value = ''
             BEGIN
                 SELECT @MasterConfigurationSettingId AS MasterConfigurationSettingId,
                        'Value can not be null or empty string. Please validate and try again.' AS ErrorMessage;
                 RETURN;
             END;
         DECLARE @MasterSettingId INT;
         DECLARE @MasterSettingTypeId INT;
         SELECT @MasterSettingTypeId = MS.MasterSettingTypeId
         FROM Enterprise.MasterConfigurationSetting MCS
              INNER JOIN Enterprise.MasterConfiguration MC ON MC.MasterConfigurationId = MCS.MasterConfigurationId
              INNER JOIN Enterprise.MasterSetting MS ON MCS.MasterSettingId = MS.MasterSettingId
              INNER JOIN Enterprise.MasterSettingType MST ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
              INNER JOIN Enterprise.MasterConfigurationType MCT ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
         WHERE MasterConfigurationSettingID = @MasterConfigurationSettingId;
         IF EXISTS
			(
				SELECT 1
				FROM Enterprise.MasterSetting
				WHERE MasterSettingTypeId = @MasterSettingTypeId
					  AND Value = @Value
			)
             BEGIN
                 SELECT @MasterSettingId = MasterSettingid
                 FROM Enterprise.MasterSetting
                 WHERE MasterSettingTypeId = @MasterSettingTypeId
                       AND Value = @Value;
                 IF @MasterSettingId IS NOT NULL
                     BEGIN TRY
                         UPDATE Enterprise.MasterConfigurationSetting
                           SET
                               MasterSettingId = @MasterSettingId
                         WHERE MasterConfigurationSettingId = @MasterConfigurationSettingId;
                         SELECT @MasterConfigurationSettingId AS 'Id',
                                '' AS ErrorMessage;
                     END TRY
                     BEGIN CATCH
                         EXEC dbo.LogError
                              @ErrorLogID = @ErrorLogID OUTPUT;
                         SELECT 0 AS Id,
                                '' ErrorMessage
                         FROM dbo.ErrorLog
                         WHERE ErrorLogID = @ErrorLogID;
                         ROLLBACK;
                     END CATCH;
             END;
             ELSE
             BEGIN
                 SELECT @MasterSettingId = MasterSettingId
                 FROM Enterprise.MasterConfigurationSetting
                 WHERE MasterCOnfigurationSettingId = @MasterConfigurationSettingId;
                 BEGIN TRY
                     UPDATE MS
                       SET
                           Value = @Value
                     FROM Enterprise.MasterSetting MS
                          INNER JOIN Enterprise.MasterCOnfigurationSetting MCS ON MCS.MasterSettingId = MS.MasterSettingId
                     WHERE MCS.MasterCOnfigurationSettingId = @MasterConfigurationSettingId;
                     SELECT @MasterConfigurationSettingId AS 'Id',
                            '' AS ErrorMessage;
                 END TRY
                 BEGIN CATCH
                     EXEC dbo.LogError
                          @ErrorLogID = @ErrorLogID OUTPUT;
                     SELECT 0 AS Id,
                            '' ErrorMessage
                     FROM dbo.ErrorLog
                     WHERE ErrorLogID = @ErrorLogID;
                     ROLLBACK;
                 END CATCH;
             END;
     END;