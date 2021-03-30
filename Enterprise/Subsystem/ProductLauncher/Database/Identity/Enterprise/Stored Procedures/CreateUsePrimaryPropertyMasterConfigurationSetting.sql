Create PROCEDURE [Enterprise].[CreateUsePrimaryPropertyMasterConfigurationSetting]
(@MasterConfigurationType NVARCHAR(200),
 @MasterSettingType       NVARCHAR(100),
 @PartyId                 BIGINT,
 @Value                   NVARCHAR(4000)
)
AS
     BEGIN
         DECLARE @MasterSettingId INT;
         DECLARE @MasterSettingTypeId INT;
         DECLARE @MasterConfigurationId INT;
		 DECLARE @MasterConfigurationSettingId INT;
         
		 SELECT  @MasterConfigurationId = MasterConfigurationId
         FROM Enterprise.MasterConfiguration MC
              INNER JOIN Enterprise.MasterConfigurationType MCT ON MC.MasterConfigurationTypeId = MCT.MasterConfigurationTypeId
         WHERE MC.AttributeId = @PartyId 
               AND MCT.Name = @MasterConfigurationType;
			   
		SELECT @MasterSettingId = MasterSettingId
		FROM Enterprise.MasterSetting MS
				INNER JOIN Enterprise.MasterSettingType MST ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
				INNER JOIN Enterprise.MasterConfigurationType MCT ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
		WHERE MST.Name = @MasterSettingType
		AND MCT.Name = @MasterConfigurationType
		AND MS.Value = @Value

        BEGIN TRY
			IF (@MasterSettingId IS NULL OR @MasterSettingId = 0)
			BEGIN
				 SELECT DISTINCT  @MasterSettingTypeId = MST.MasterSettingTypeId
				 FROM Enterprise.MasterSettingType MST
					  INNER JOIN Enterprise.MasterConfigurationType MCT ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
					  INNER JOIN Enterprise.MasterConfiguration MC ON MC.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
				 WHERE MST.Name = @MasterSettingType 
                AND MCT.Name = @MasterConfigurationType	

				INSERT INTO Enterprise.MasterSetting
						(MasterSettingTypeId,
						 Value,
						 FromDate,
						 ThruDate
						)
				  VALUES
						(@MasterSettingTypeId,
						 @Value,
						 GETUTCDATE(),
						 NULL
						);
				  SELECT @MasterSettingId = SCOPE_IDENTITY();
			END

			IF NOT EXISTS (SELECT 1 FROM Enterprise.MasterConfigurationSetting
			WHERE MasterConfigurationId = @MasterCOnfigurationId
			AND MasterSettingId = @MasterSettingId)
			BEGIN
				---Delete existing MasterConfigurationSetting
				DELETE FROM Enterprise.MasterConfigurationSetting
				WHERE MasterConfigurationId = @MasterCOnfigurationId
				AND MasterSettingId IN (SELECT MasterSettingId
					FROM Enterprise.MasterSetting MS
							INNER JOIN Enterprise.MasterSettingType MST ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
							INNER JOIN Enterprise.MasterConfigurationType MCT ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
					WHERE MST.Name = @MasterSettingType
					AND MCT.Name = @MasterConfigurationType)

				---Insert MasterConfigurationSetting
				INSERT INTO Enterprise.MasterCOnfigurationSetting
				(MasterConfigurationId,
					MasterSettingId
				)
				VALUES
				(@MasterCOnfigurationId,
					@MasterSettingId
				);
				SELECT @MasterConfigurationSettingId = SCOPE_IDENTITY()
			END
			
			SELECT @MasterSettingId AS 'Id',
				'' AS ErrorMessage;      
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
