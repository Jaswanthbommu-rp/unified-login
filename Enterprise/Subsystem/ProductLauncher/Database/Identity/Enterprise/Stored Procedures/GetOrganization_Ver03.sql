CREATE PROCEDURE [Enterprise].[GetOrganization_Ver03] (  
 @RealPageId uniqueidentifier = NULL,  
 @PartyId bigint = NULL,  
 @BlueBookId bigint = NULL,  
 @BlackBookId bigint = NULL  
)  
AS  
BEGIN  
	DECLARE @UsePrimaryProperty tinyint = 0;
    
	IF (@PartyId IS NULL AND @RealPageId IS NOT NULL)
	BEGIN
		Select @PartyId = PartyId FROM Enterprise.Party WHERE RealPageId = @RealPageId
	END
	IF (@PartyId IS NULL AND @RealPageId IS NULL AND @BlueBookId IS NOT NULL)
	BEGIN
		Select @PartyId = PartyId FROM Enterprise.VW_DataImportMapping WHERE CompanyMasterId = @BlueBookId
	END
	IF (@PartyId IS NULL AND @RealPageId IS NULL AND @BlueBookId IS NULL AND @BlackBookId IS NOT NULL)
	BEGIN
		Select @PartyId = PartyId FROM Enterprise.VW_DataImportMapping WHERE MasterId = @BlueBookId
	END
	
	SELECT @UsePrimaryProperty = MS.Value            
        FROM Enterprise.MasterConfigurationSetting mcs
        INNER JOIN Enterprise.MasterConfiguration mc ON mc.MasterConfigurationId = mcs.MasterConfigurationId
        INNER JOIN Enterprise.MasterSetting ms ON mcs.MasterSettingId = ms.MasterSettingId
        INNER JOIN Enterprise.MasterSettingType mst ON mst.MasterSettingTypeId = ms.MasterSettingTypeId
        INNER JOIN Enterprise.MasterConfigurationType mct ON mct.MasterConfigurationTypeId = mst.MasterConfigurationTypeId
	WHERE MST.Name = 'UsePrimaryProperties'
			AND MCT.Name = 'Organization'
			AND  MC.AttributeId = @PartyId
		
	SELECT O.PartyId,  
			O.Name,  
			P.RealPageId,  
			COALESCE(ISNULL(D.MasterId, 0),0) AS BooksMasterId,  
			COALESCE(ISNULL(D.CompanyMasterId, 0), 0) AS BooksCustomerMasterId,  
			o.OrganizationTypeId,
			o.OrganizationDomainId,
			o.IsActive,
			@UsePrimaryProperty AS 'UsePrimaryProperties'
		 FROM [Enterprise].Organization AS o  
			INNER JOIN [Enterprise].Party P ON P.PartyId = O.PartyId  
			LEFT OUTER JOIN Enterprise.VW_DataImportMapping D ON(O.PartyId = D.PartyId)  
		 WHERE 
			@PartyId = o.PartyId
	
END;