CREATE PROCEDURE [Enterprise].[CreateMasterConfigurationSetting]
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
         

		 SELECT DISTINCT
                 @MasterSettingTypeId = MST.MasterSettingTypeId
         FROM Enterprise.MasterSettingType MST
              INNER JOIN Enterprise.MasterConfigurationType MCT ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
              INNER JOIN Enterprise.MasterConfiguration MC ON MC.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
         WHERE MST.Name = @MasterSettingType 
               AND MCT.Name = @MasterConfigurationType

         IF EXISTS
		(
			SELECT 1             
                            FROM Enterprise.MasterConfigurationSetting mcs
                            INNER JOIN Enterprise.MasterConfiguration mc ON mc.MasterConfigurationId = mcs.MasterConfigurationId
                            INNER JOIN Enterprise.MasterSetting ms ON mcs.MasterSettingId = ms.MasterSettingId
                            INNER JOIN Enterprise.MasterSettingType mst ON mst.MasterSettingTypeId = ms.MasterSettingTypeId
                            INNER JOIN Enterprise.MasterConfigurationType mct ON mct.MasterConfigurationTypeId = mst.MasterConfigurationTypeId
                            WHERE MST.Name = @MasterSettingType
				  AND MCT.Name = @MasterConfigurationType
				  AND MC.AttributeId = @PartyId
			
			
			
		)
		BEGIN
			SELECT @MasterSettingId = MasterSettingId
			FROM Enterprise.MasterSetting MS
				 INNER JOIN Enterprise.MasterSettingType MST ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
				 INNER JOIN Enterprise.MasterConfigurationType MCT ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
			WHERE MST.Name = @MasterSettingType
				  AND MCT.Name = @MasterConfigurationType
		
			SELECT @MasterSettingId As Id, 'Setting already exists. Try updating the value.' as ErrorMessage
		END
		ELSE
             BEGIN TRY
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
                 INSERT INTO Enterprise.MasterCOnfigurationSetting
					(MasterCOnfigurationId,
					 MasterSettingId
					)
                 VALUES
					(@MasterCOnfigurationId,
					 @MasterSettingId
					);
				 SELECT @MasterConfigurationSettingId = SCOPE_IDENTITY()
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