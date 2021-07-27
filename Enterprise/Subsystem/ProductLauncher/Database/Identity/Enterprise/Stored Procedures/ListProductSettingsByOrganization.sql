CREATE PROCEDURE [Enterprise].[ListProductSettingsByOrganization] (
	@OrganizationRealPageId UNIQUEIDENTIFIER,
	@ProductId INT = NULL
)

AS 

BEGIN
	
	DECLARE @NOW  DATETIME = GETUTCDATE();
       
	SELECT	DISTINCT ps.ProductId,
			ps.ProductSettingId, 
			ps.ProductSettingTypeId, 
			pst.[Name], 
			ps.[Value], 
			pst.[Description],
			pst.SensitiveData
	FROM	[Enterprise].[OrganizationProduct] op 
			JOIN [Enterprise].[ProductConfiguration] pc ON pc.ConfigurationId = op.ConfigurationId
			JOIN [Enterprise].[ProductSetting] ps ON ps.ProductSettingId = pc.ProductSettingId
			JOIN [Enterprise].[ProductSettingType] pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId     
			JOIN [Enterprise].[Party] par ON par.PartyId = op.PartyId
	WHERE	
		par.RealPageId = @OrganizationRealPageId
		AND (@productId IS NULL OR op.ProductId = @ProductId) 
		AND ((@NOW BETWEEN op.FromDate AND op.ThruDate) OR (@NOW >= op.FromDate AND op.ThruDate IS NULL))
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))
		AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))

END