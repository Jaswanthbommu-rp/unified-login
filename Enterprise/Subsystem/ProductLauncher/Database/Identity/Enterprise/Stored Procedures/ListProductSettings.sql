CREATE PROCEDURE [Enterprise].[ListProductSettings] (
	@ProductId INT,
	@PersonaId BIGINT,
	@OrganizationRealPageId UNIQUEIDENTIFIER
)
AS
BEGIN
	DECLARE @NOW DATETIME = GETUTCDATE();

	IF (CASE WHEN @PersonaId IS NULL THEN 0 ELSE 1 END) ^ (CASE WHEN @OrganizationRealPageId IS NULL THEN 0 ELSE 1 END) = 1
	BEGIN
		SELECT	ps.ProductId,
				ps.ProductSettingId, 
				ps.ProductSettingTypeId, 
				pst.[Name], 
				ps.[Value], 
				pst.[Description],
				pc.ConfigurationId
		FROM	[Enterprise].[ProductConfiguration] pc 
		INNER JOIN [Enterprise].[ProductSetting] ps ON ps.ProductSettingId = pc.ProductSettingId
		INNER JOIN [Enterprise].[ProductSettingType] pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId
		LEFT OUTER JOIN [Enterprise].[PersonaConfiguration] perc ON pc.ConfigurationId = perc.ConfigurationId
		LEFT OUTER JOIN [Enterprise].[OrganizationProduct] op ON pc.ConfigurationId = op.ConfigurationId
		LEFT OUTER JOIN [Enterprise].[Party] par ON op.PartyId = par.PartyId
		WHERE	((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))
		AND		(perc.PersonaId = @PersonaId 
					AND ((@NOW BETWEEN perc.FromDate AND perc.ThruDate) 
						OR (@NOW >= perc.FromDate AND perc.ThruDate IS NULL))
				OR @PersonaId IS NULL)
		AND		(par.RealPageId = @OrganizationRealPageId
					AND ((@NOW BETWEEN op.FromDate AND op.ThruDate)
						OR (@NOW >= op.FromDate AND op.ThruDate IS NULL))
				OR @OrganizationRealPageId IS NULL)
		AND		(ps.ProductId = @ProductId OR @ProductId IS NULL)				
	END
END;