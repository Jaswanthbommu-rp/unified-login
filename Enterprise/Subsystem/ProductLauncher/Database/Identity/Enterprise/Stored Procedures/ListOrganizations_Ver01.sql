CREATE PROCEDURE [Enterprise].[ListOrganizations_Ver01] 
    @RealPageId UNIQUEIDENTIFIER = NULL,
    @Filter VARCHAR(100) = NULL
AS
     BEGIN
         SELECT O.PartyId,
                O.Name,
                O.IsActive,
                P.RealPageId 'OrganizationRealPageId',
                COALESCE(ISNULL(D.MasterId, 0),0)  AS 'BooksMasterId',
				COALESCE(ISNULL(D.CompanyMasterId, 0), 0)  AS 'BooksCustomerMasterId',
                MST.Name AS 'SettingName',
                MS.Value AS 'PersonRealPageId',
                UL.LoginName,
				OD.Name as Domain
         FROM Enterprise.Organization O
              INNER JOIN Enterprise.Party P ON O.PartyId = P.PartyId
              INNER JOIN Enterprise.VW_DataImportMapping D ON O.PartyId = D.PartyId
			  INNER JOIN Enterprise.OrganizationDomain OD on O.OrganizationDomainId = OD.OrganizationDomainId
              INNER JOIN Enterprise.MasterConfiguration MC ON MC.AttributeId = O.PartyId
              INNER JOIN Enterprise.MasterConfigurationSetting MCS ON MC.MasterConfigurationId = MCS.MasterConfigurationId
              INNER JOIN Enterprise.MasterSetting MS ON MCS.MasterSettingId = MS.MasterSettingId
              INNER JOIN Enterprise.MasterSettingType MST ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
              INNER JOIN Enterprise.MasterConfigurationType MCT ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
              INNER JOIN
				(
					SELECT P.RealPageId,
						   UL.LoginName
					FROM 
						Ident.UserLogin UL
						INNER JOIN Enterprise.Party P ON UL.PersonPartyId = P.PartyId
				) UL ON CONVERT(VARCHAR(40), UL.RealPageId) = MS.Value
         WHERE MCT.Name = 'Organization'
               AND MST.Name = 'RealPageEmployeeAccessID'
               AND (o.Name like '%'+@Filter+'%' OR @Filter IS NULL)
			   AND (P.RealPageId = @RealPageId OR @RealPageID IS NULL)
		order by O.Name
     END;