Create PROCEDURE [Enterprise].[GetOrganizationMasterConfiguration](
    @PartyId bigint ,
	@Name varchar(50),
    @MasterConfigurationId INT OUTPUT)
AS
    BEGIN
        Select @MasterConfigurationId = MasterConfigurationId 
		FROM Enterprise.MasterConfiguration MC
		JOIN Enterprise.MasterConfigurationType MCT ON
			MCT.MasterConfigurationTypeId = mc.MasterConfigurationTypeId
		WHERE MCT.Name = @Name
		AND AttributeId = @PartyId
    END;