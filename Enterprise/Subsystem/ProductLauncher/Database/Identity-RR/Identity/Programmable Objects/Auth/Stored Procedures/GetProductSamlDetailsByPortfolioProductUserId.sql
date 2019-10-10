IF OBJECT_ID('[Auth].[GetProductSamlDetailsByPortfolioProductUserId]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetProductSamlDetailsByPortfolioProductUserId];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetProductSamlDetailsByPortfolioProductUserId]
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
		Auth.PortfolioProductUser PPU
		INNER JOIN Auth.UserSamlAttribute USA
			ON PPU.PortfolioProductUserId = USA.PortfolioProductUserId
		INNER JOIN Auth.SamlAttribute SA
				ON SA.SamlAttributeId = USA.SamlAttributeId 
		RIGHT OUTER JOIN Auth.SamlAttributeStatement SAS
			ON SA.SamlAttributeId = SAS.SamlAttributeId AND SAS.ProductId = PPU.ProductId

		
	WHERE
		PPU.PortfolioProductUserId = @PortfolioProductUserId

END
GO
