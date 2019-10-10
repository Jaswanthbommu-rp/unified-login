CREATE PROCEDURE [Enterprise].[ListOrganizations] @RealPageId UNIQUEIDENTIFIER = NULL
AS
     BEGIN
         SELECT O.PartyId,
                O.Name,
                P.RealPageId 'OrganizationRealPageId',
                D.SourceId AS 'BooksMasterId',
                MST.Name AS 'SettingName',
                MS.Value AS 'PersonRealPageId',
                UL.LoginName
         FROM Enterprise.Organization O
              INNER JOIN Enterprise.Party P ON O.PartyId = P.PartyId
              INNER JOIN Enterprise.DataImportMapping D ON O.PartyId = D.PartyId
              INNER JOIN Enterprise.MasterConfiguration MC ON MC.AttributeId = O.PartyId
              INNER JOIN Enterprise.MasterConfigurationSetting MCS ON MC.MasterConfigurationId = MCS.MasterConfigurationId
              INNER JOIN Enterprise.MasterSetting MS ON MCS.MasterSettingId = MS.MasterSettingId
              INNER JOIN Enterprise.MasterSettingType MST ON MST.MasterSettingTypeId = MS.MasterSettingTypeId
              INNER JOIN Enterprise.MasterConfigurationType MCT ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
              INNER JOIN
				(
					SELECT P.RealPageId,
						   UL.LoginName
					FROM Enterprise.Party P
						 INNER JOIN Ident.UserLogin UL ON UL.PersonPartyId= P.PartyId
						 INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId 
						 INNER JOIN Person.Persona PP ON PP.UserLoginPersonaId = ULP.UserLoginPersonaId
				) UL ON CONVERT(VARCHAR(40), UL.RealPageId) = MS.Value
         WHERE MCT.Name = 'Organization'
               AND MST.Name = 'RealPageEmployeeAccessID'
			   AND (P.RealPageId = @RealPageId OR @RealPageID IS NULL)
     END;

