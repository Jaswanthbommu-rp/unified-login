IF OBJECT_ID('[Auth].[GetProductSamlDetailsByUserId]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetProductSamlDetailsByUserId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetProductSamlDetailsByUserId]
	@PortfolioProductUserId		int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT        
		SAS.SamlAttributeId
		, SA.Name
		, SA.Type
		, USA.Value
	
	FROM 
		Auth.SamlAttribute SA
		INNER JOIN Auth.UserSamlAttribute USA
			ON SA.SamlAttributeId = USA.SamlAttributeId 
		INNER JOIN Auth.PortfolioProductUser PPU
			ON USA.PortfolioProductUserId = PPU.PortfolioProductUserId
		RIGHT OUTER JOIN Auth.SamlAttributeStatement SAS
			ON SA.SamlAttributeId = SAS.SamlAttributeId
		
	WHERE
		PPU.PortfolioProductUserId = @PortfolioProductUserId

END
GO
