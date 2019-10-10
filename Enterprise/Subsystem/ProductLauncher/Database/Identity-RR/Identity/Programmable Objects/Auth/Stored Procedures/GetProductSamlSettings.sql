IF OBJECT_ID('[Auth].[GetProductSamlSettings]') IS NOT NULL
	DROP PROCEDURE [Auth].[GetProductSamlSettings];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Auth].[GetProductSamlSettings]
	@ProductId		int

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	SELECT        
		ProductSamlSettingsId
		,ProductId
		,LoginUri
		,SigningCertificateThumbprint
		,SubjectIdSamlAttribute

	FROM 
		Auth.ProductSamlSettings

	WHERE
		Auth.ProductSamlSettings.ProductId  = @ProductId

END
GO
