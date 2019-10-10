CREATE PROCEDURE [Enterprise].[ListMasterConfigurationType](@ConfigurationType NVARCHAR(100))
AS
     BEGIN
         SELECT DISTINCT
                MST.Name
         FROM Enterprise.MasterSettingType MST
              INNER JOIN Enterprise.MasterConfigurationType MCT ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
              INNER JOIN Enterprise.MasterConfiguration MC ON MC.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
         WHERE MCT.Name = @ConfigurationType;
     END;