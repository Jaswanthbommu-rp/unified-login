CREATE PROCEDURE [Person].[ListPersonsForMigrationTool]
(@RealPageId            UNIQUEIDENTIFIER = NULL,
 @Name                  NVARCHAR(100)    = NULL,
 @ProductId             INT              = NULL,
 @ParentPartyRoleTypeId INT              = NULL,
 @IsCustomFieldsEnabled BIT				 = 0
)
AS
     BEGIN
        DECLARE @RelationshipType VARCHAR(50);
        DECLARE @PartyId BIGINT;
        DECLARE @NOW DATETIME= GETUTCDATE(); 
		--DECLARE @Offset INT = (@PageNumber - 1) * @RowCount

	-- Technical Debt Item for Backwards Compatibility

		SELECT @RelationshipType = 
				CASE
					WHEN @ParentPartyRoleTypeId = 400
					THEN 'User Type'
					WHEN @ParentPartyRoleTypeId = 200
					THEN 'Employment'
					ELSE NULL
				END;
		SELECT @PartyId = PartyId
		FROM Enterprise.Party
		WHERE RealPageId = @RealPageId;

		WITH ProductsCTE AS (
					 SELECT p.PersonaID, 
					   pec.ProductId
					 FROM Enterprise.ProductSettingType PST
					 INNER JOIN Enterprise.ProductSetting PS ON ps.ProductSettingTypeId = pst.ProductSettingTypeId AND pst.Name = 'ProductStatus' AND ps.Value IN('8')
					 INNER JOIN Enterprise.ProductConfiguration PRC ON prc.ProductSettingId = ps.ProductSettingId 
					 INNER JOIN Enterprise.PersonaConfiguration PEC ON prc.ConfigurationId = pec.ConfigurationId
					 INNER JOIN Person.Persona P ON p.PersonaId = pec.PersonaId
					 WHERE ((@NOW BETWEEN pec.FromDate AND pec.ThruDate) OR (@NOW >= pec.FromDate AND pec.ThruDate IS NULL))
					   AND ((@NOW BETWEEN prc.FromDate AND prc.ThruDate) OR (@NOW >= prc.FromDate AND prc.ThruDate IS NULL))
					   AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
					   AND pec.ProductId NOT IN (4, 14, 19, 25, 34, 24)),
			 ProductCount AS (
					 SELECT PersonaId, 
					   COUNT(ProductId) AS ProductCount 
					 FROM ProductsCTE 
					 GROUP BY PersonaId),
			 SettingsCTE AS (
					 SELECT AttributeId,
							TimeZone,
							ThemeColor,
							CustomFields
					 FROM
						(
							SELECT mc.AttributeId,
								   mst.Name AS SettingName,
								   ms.Value
							FROM Enterprise.MasterConfigurationType mct
							INNER JOIN Enterprise.MasterSettingType mst ON mct.MasterConfigurationTypeId = mst.MasterConfigurationTypeId 
								AND mst.Name IN('TimeZone', 'ThemeColor', 'CustomFields')
							INNER JOIN Enterprise.MasterSetting ms ON mst.MasterSettingTypeId = ms.MasterSettingTypeId 
							INNER JOIN Enterprise.MasterConfigurationSetting mcs ON mcs.MasterSettingId = ms.MasterSettingId
							INNER JOIN Enterprise.MasterConfiguration mc ON mc.MasterConfigurationId = mcs.MasterConfigurationId
							WHERE mct.Name = 'UserLogin') AS T 
							PIVOT(MAX(T.Value) FOR T.SettingName IN([TimeZone],
																	[ThemeColor],
																	[CustomFields])) AS P),
				PrimaryEmail AS
					(	SELECT  P.PartyId, 
				ea.ElectronicAddressString AS NotificationEmail
		FROM    Enterprise.ContactMechanismUsage cmu
				JOIN Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID
				JOIN Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId
				JOIN Enterprise.ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID
				JOIN Enterprise.Party p ON p.PartyId = pcm.PartyId
		WHERE	(pcm.ThruDate IS NULL OR pcm.ThruDate > GETUTCDATE())
			AND cmu.ContactMechanismUsageTypeID = 301
		)
			 SELECT DISTINCT
					pa.RealpageId AS RealPageID,
					p.PartyId,
					p.FirstName,
					p.MiddleName,
					p.LastName,
					p.Title,
					p.Suffix,
					S.CustomFields AS 'CustomField',
					ul.UserId,
					ul.LoginName,
					EM.NotificationEmail,
					ul.LastLoginDate AS LastLogin,
					ul.FromDate,
					ul.ThruDate,
					Is3rdPartyIDP = CASE
										WHEN ipt.Name = 'ID3'
										THEN 0
										ELSE 1
									END,
					ISNULL(PCNT.ProductCount, 0) AS Products,
					0 AS Properties,
					ISNULL(rt.Name, '') AS UserType,
					prs.RoleTypeIdFrom AS PartyRoleTypeId,
					S.TimeZone AS TimeZoneOffset
			 FROM Ident.IdentityProviderType ipt
			 INNER JOIN Ident.UserLogin ul ON ul.IdentityProviderTypeId = ipt.IdentityProviderTypeId
			 INNER JOIN SettingsCTE S ON S.AttributeId = UL.UserId
			 INNER JOIN Ident.UserLoginPersona ULP ON ULP.UserLoginId = UL.UserId
			 INNER JOIN Person.Persona PE ON ULP.UserLoginPersonaId = PE.UserLoginPersonaId
			 LEFT OUTER JOIN PrimaryEmail EM ON EM.PartyId = PE.PersonPartyId
			 INNER JOIN Enterprise.Organization AS o ON o.PartyId = pe.OrganizationPartyId
			 INNER JOIN Person.Person AS p ON p.PartyId = pe.PersonPartyId
			 LEFT OUTER JOIN ProductsCTE PCT ON PCT.PersonaId = pe.PersonaID
			 LEFT OUTER JOIN ProductCount PCNT ON PCT.PersonaId = PCNT.PersonaId
			 INNER JOIN Enterprise.Party AS pa ON p.PartyId = pa.PartyID
			 INNER JOIN Enterprise.PartyRelationship AS prs ON prs.PartyIdFrom = p.PartyId
															   AND (prs.PartyIdTo = @PartyId
																	OR @RealPageId IS NULL)
			 INNER JOIN Enterprise.RelationshipType AS rst ON rst.RelationshipTypeId = prs.PartyRelationshipTypeId
			 INNER JOIN Enterprise.RoleType AS rt ON rt.PartyRoleTypeId = rst.RoleTypeIdValidFrom
			 WHERE(OrganizationPartyId = @PartyId
				   OR @RealPageId IS NULL)
				  AND ((CHARINDEX(@Name, FirstName, 1) > 0)
					   OR (CHARINDEX(@Name, LastName, 1) > 0)
					   OR (CHARINDEX(@Name, LoginName, 1) > 0)
					   --OR (CHARINDEX(@Name, FirstName+' '+LastName, 1) > 0)
					   OR (CHARINDEX(@Name, S.CustomFields, 1) > 0)
					   OR @Name IS NULL
					   OR S.CustomFields LIKE CASE
												  WHEN(CHARINDEX(@Name, S.CustomFields, 1) > 0)
													  AND @IsCustomFieldsEnabled = 1
												  THEN '%'+@name+'%'
												  ELSE '~~~~~'
											  END)
				  AND ((@NOW BETWEEN prs.FromDate AND prs.ThruDate)
					   OR (@NOW >= prs.FromDate
						   AND prs.ThruDate IS NULL))
				  AND (rst.Name = @RelationshipType
					   OR @RelationshipType IS NULL)
				  AND (pe.PersonaId IN
						(
							SELECT PersonaId
							FROM ProductsCTE
							WHERE ProductId = @ProductId
						)
					   OR @ProductId IS NULL)

			OPTION(OPTIMIZE FOR UNKNOWN);

			
			
	END