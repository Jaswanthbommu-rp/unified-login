CREATE PROCEDURE [Person].[ListPersonsByProductId] (
     @ProductId int,
     @RealPageId uniqueidentifier = NULL,
	 @PersonaId bigint = NULL
)
AS
BEGIN
     DECLARE @PartyId bigint;  
  
     DECLARE @NOW  datetime = GETUTCDATE();   
    
     SELECT @PartyId = PartyId  
     FROM Enterprise.Party  
     WHERE RealPageId = @RealPageId
  
	SELECT	pa.RealpageId AS RealPageID,  
					p.PartyId,  
					p.FirstName,  
					p.MiddleName,
					p.LastName,  
					p.Title,  
					p.Suffix,
					ul.UserId,  
					ul.LoginName,  
					s.Value As 'TimeZoneOffset'
	FROM		Person.Persona pe
		INNER JOIN Ident.UserLoginPersona ULP ON PE.UserLoginPersonaId = ULP.UserLoginPersonaId
		INNER JOIN Enterprise.Organization eo ON (ULP.OrganizationPartyId = eo.PartyId)
		INNER JOIN Ident.UserLogin UL ON eo.PartyId = UL.PersonPartyId
		JOIN Person.Person p ON (p.PartyId = UL.PersonPartyId)
		JOIN Enterprise.Party pa ON (p.PartyId = pa.PartyID)
					LEFT JOIN (  
						SELECT	mc.AttributeId,
										ms.Value                  
						FROM		Enterprise.MasterConfigurationSetting mcs  
										INNER JOIN Enterprise.MasterConfiguration mc ON (mc.MasterConfigurationId = mcs.MasterConfigurationId)
										INNER JOIN Enterprise.MasterSetting ms ON (mcs.MasterSettingId = ms.MasterSettingId)
										INNER JOIN Enterprise.MasterSettingType mst ON (mst.MasterSettingTypeId = ms.MasterSettingTypeId)
										INNER JOIN Enterprise.MasterConfigurationType mct ON (mct.MasterConfigurationTypeId = mst.MasterConfigurationTypeId)
						WHERE	mct.Name = 'UserLogin'                     
						AND			mst.Name = 'TimeZone'  
					) AS s ON s.AttributeId = ul.UserId  
	WHERE	((OrganizationPartyId = @PartyId) OR (@RealPageId IS NULL))
	AND			(
						pe.PersonaId IN
						(
							SELECT	pe.PersonaID
							FROM		Person.Persona pe
											INNER JOIN Enterprise.PersonaConfiguration pec ON (pe.PersonaId = pec.PersonaId)
											INNER JOIN Enterprise.ProductConfiguration prc ON (pec.ConfigurationId = prc.ConfigurationId)
											INNER JOIN Enterprise.ProductSetting ps ON (prc.ProductSettingId = ps.ProductSettingId AND ps.Value IN ('6','7','8'))
											INNER JOIN Enterprise.ProductSettingType pst ON (ps.ProductSettingTypeId = pst.ProductSettingTypeId AND pst.Name = 'ProductStatus')
							WHERE	((@NOW BETWEEN pec.FromDate AND pec.ThruDate) OR (@NOW >= pec.FromDate AND pec.ThruDate IS NULL))  
							AND			((@NOW BETWEEN prc.FromDate AND prc.ThruDate) OR (@NOW >= prc.FromDate AND prc.ThruDate IS NULL))  
							AND			((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
							AND			pec.ProductId = @ProductId
							AND			((@PersonaId IS NULL) OR (pe.PersonaId = @PersonaId))
						) 
						OR @ProductId IS NULL
					)
END