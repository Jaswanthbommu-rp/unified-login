CREATE PROCEDURE [Enterprise].[GetOrganization_Ver03] (  
 @RealPageId uniqueidentifier = NULL,  
 @PartyId bigint = NULL,  
 @BlueBookId bigint = NULL,  
 @BlackBookId bigint = NULL  
)  
AS  
BEGIN  
	-- breaking where clauses into separate queries for performance
	if @RealPageId is not null
	begin
		 SELECT O.PartyId,  
			O.Name,  
			P.RealPageId,  
			COALESCE(ISNULL(D.MasterId, 0),0) AS BooksMasterId,  
			COALESCE(ISNULL(D.CompanyMasterId, 0), 0) AS BooksCustomerMasterId,  
			o.OrganizationTypeId,
			o.OrganizationDomainId,
			o.IsActive,
			CASE WHEN (SELECT CASE WHEN OS.MappingValue = '1' THEN 1 ELSE 0 END FROM Settings.OrganizationSettings OS INNER JOIN Settings.SettingCategoryType st ON st.SettingCategoryTypeId = OS.SettingCategoryTypeId WHERE PartyId = O.PartyId AND MappingName = 'PrimaryPropertyEnterpriseRole' AND st.Name = 'Company' ) = 1 THEN 1 ELSE 0 end [EnablePrimaryPropertiesAndEnterpriseRoles]
		 FROM [Enterprise].Organization AS o  
			INNER JOIN [Enterprise].Party P ON P.PartyId = O.PartyId  
			LEFT OUTER JOIN Enterprise.VW_DataImportMapping D ON(O.PartyId = D.PartyId)  
		 WHERE 
			@RealPageId = P.RealPageId
	end
	else if @PartyId is not null 
	begin
		 SELECT O.PartyId,  
			O.Name,  
			P.RealPageId,  
			COALESCE(ISNULL(D.MasterId, 0),0) AS BooksMasterId,  
			COALESCE(ISNULL(D.CompanyMasterId, 0), 0) AS BooksCustomerMasterId,  
			o.OrganizationTypeId,
			o.OrganizationDomainId,
			o.IsActive,
			CASE WHEN (SELECT CASE WHEN OS.MappingValue = '1' THEN 1 ELSE 0 END FROM Settings.OrganizationSettings OS INNER JOIN Settings.SettingCategoryType st ON st.SettingCategoryTypeId = OS.SettingCategoryTypeId WHERE PartyId = O.PartyId AND MappingName = 'PrimaryPropertyEnterpriseRole' AND st.Name = 'Company' ) = 1 THEN 1 ELSE 0 end [EnablePrimaryPropertiesAndEnterpriseRoles]
		 FROM [Enterprise].Organization AS o  
			INNER JOIN [Enterprise].Party P ON P.PartyId = O.PartyId  
			LEFT OUTER JOIN Enterprise.VW_DataImportMapping D ON(O.PartyId = D.PartyId)  
		 WHERE 
			@PartyId = o.PartyId
	end
	else if @BlueBookId is not null
	begin
		 SELECT O.PartyId,  
			O.Name,  
			P.RealPageId,  
			COALESCE(ISNULL(D.MasterId, 0),0) AS BooksMasterId,  
			COALESCE(ISNULL(D.CompanyMasterId, 0), 0) AS BooksCustomerMasterId,  
			o.OrganizationTypeId,
			o.OrganizationDomainId,
			o.IsActive,
			CASE WHEN (SELECT CASE WHEN OS.MappingValue = '1' THEN 1 ELSE 0 END FROM Settings.OrganizationSettings OS INNER JOIN Settings.SettingCategoryType st ON st.SettingCategoryTypeId = OS.SettingCategoryTypeId WHERE PartyId = O.PartyId AND MappingName = 'PrimaryPropertyEnterpriseRole' AND st.Name = 'Company' ) = 1 THEN 1 ELSE 0 end [EnablePrimaryPropertiesAndEnterpriseRoles]
		 FROM [Enterprise].Organization AS o  
			INNER JOIN [Enterprise].Party P ON P.PartyId = O.PartyId  
			LEFT OUTER JOIN Enterprise.VW_DataImportMapping D ON(O.PartyId = D.PartyId)  
		 WHERE 
			@BlueBookId = D.CompanyMasterId
	end
	else if @BlackBookId is not null
	begin
		 SELECT O.PartyId,  
			O.Name,  
			P.RealPageId,  
			COALESCE(ISNULL(D.MasterId, 0),0) AS BooksMasterId,  
			COALESCE(ISNULL(D.CompanyMasterId, 0), 0) AS BooksCustomerMasterId,  
			o.OrganizationTypeId,
			o.OrganizationDomainId,
			o.IsActive,
			CASE WHEN (SELECT CASE WHEN OS.MappingValue = '1' THEN 1 ELSE 0 END FROM Settings.OrganizationSettings OS INNER JOIN Settings.SettingCategoryType st ON st.SettingCategoryTypeId = OS.SettingCategoryTypeId WHERE PartyId = O.PartyId AND MappingName = 'PrimaryPropertyEnterpriseRole' AND st.Name = 'Company' ) = 1 THEN 1 ELSE 0 end [EnablePrimaryPropertiesAndEnterpriseRoles]
		 FROM [Enterprise].Organization AS o  
			INNER JOIN [Enterprise].Party P ON P.PartyId = O.PartyId  
			LEFT OUTER JOIN Enterprise.VW_DataImportMapping D ON(O.PartyId = D.PartyId)  
		 WHERE 
			@BlackBookId = D.MasterId
	end
	else
	begin
		 SELECT O.PartyId,  
			O.Name,  
			P.RealPageId,  
			COALESCE(ISNULL(D.MasterId, 0),0) AS BooksMasterId,  
			COALESCE(ISNULL(D.CompanyMasterId, 0), 0) AS BooksCustomerMasterId,  
			o.OrganizationTypeId,
			o.OrganizationDomainId,
			o.IsActive,
			CASE WHEN (SELECT CASE WHEN OS.MappingValue = '1' THEN 1 ELSE 0 END FROM Settings.OrganizationSettings OS INNER JOIN Settings.SettingCategoryType st ON st.SettingCategoryTypeId = OS.SettingCategoryTypeId WHERE PartyId = O.PartyId AND MappingName = 'PrimaryPropertyEnterpriseRole' AND st.Name = 'Company' ) = 1 THEN 1 ELSE 0 end [EnablePrimaryPropertiesAndEnterpriseRoles]
		 FROM [Enterprise].Organization AS o  
			INNER JOIN [Enterprise].Party P ON P.PartyId = O.PartyId  
			LEFT OUTER JOIN Enterprise.VW_DataImportMapping D ON(O.PartyId = D.PartyId)  
	end
END;