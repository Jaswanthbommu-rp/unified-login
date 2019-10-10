CREATE PROCEDURE [Enterprise].[ListProductSettingsByPersona]
(
	@PersonaId bigint   
)

AS

BEGIN

	DECLARE @NOW DATETIME = GETUTCDATE();

    SELECT	ps.ProductId,
			ps.ProductSettingId, 
			ps.ProductSettingTypeId, 
			pst.[Name], 
			ps.[Value], 
			pst.[Description],
			pc.ConfigurationId
	FROM	[Enterprise].[PersonaConfiguration] perc 
			JOIN [Enterprise].[ProductConfiguration] pc ON pc.ConfigurationId = perc.ConfigurationId
			JOIN [Enterprise].[ProductSetting] ps ON ps.ProductSettingId = pc.ProductSettingId
			JOIN [Enterprise].[ProductSettingType] pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId                  
	WHERE	perc.PersonaId = @PersonaId
			AND ((@NOW BETWEEN perc.FromDate AND perc.ThruDate) OR (@NOW >= perc.FromDate AND perc.ThruDate IS NULL))
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))

END;

