IF OBJECT_ID('[Enterprise].[ListProductSettingsByOrganization]') IS NOT NULL
	DROP PROCEDURE [Enterprise].[ListProductSettingsByOrganization];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Enterprise].[ListProductSettingsByOrganization] (
	@OrganizationRealPageId UNIQUEIDENTIFIER
)

AS 

BEGIN
	
	DECLARE @NOW  DATETIME = GETUTCDATE();
       
	SELECT	ps.ProductId,
			ps.ProductSettingId, 
			ps.ProductSettingTypeId, 
			pst.[Name], 
			ps.[Value], 
			pst.[Description]
	FROM	[Enterprise].[OrganizationProduct] op 
			JOIN [Enterprise].[ProductConfiguration] pc ON pc.ConfigurationId = op.ConfigurationId
			JOIN [Enterprise].[ProductSetting] ps ON ps.ProductSettingId = pc.ProductSettingId
			JOIN [Enterprise].[ProductSettingType] pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId     
			JOIN [Enterprise].[Party] par ON par.PartyId = op.PartyId
	WHERE	par.RealPageId = @OrganizationRealPageId
	AND ((@NOW BETWEEN op.FromDate AND op.ThruDate) OR (@NOW >= op.FromDate AND op.ThruDate IS NULL))
	AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))

END
GO
