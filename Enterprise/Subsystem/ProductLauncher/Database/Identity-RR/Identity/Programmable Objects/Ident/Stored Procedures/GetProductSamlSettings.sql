IF OBJECT_ID('[Ident].[GetProductSamlSettings]') IS NOT NULL
	DROP PROCEDURE [Ident].[GetProductSamlSettings];

GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_NULLS ON
GO
CREATE PROCEDURE [Ident].[GetProductSamlSettings] (
	@ProductId int
)
AS
BEGIN
	SELECT	SamlProductSettingsId,
					ProductId,
					LoginUri,
					SigningCertificateThumbprint,
					SubjectIdSamlAttribute
	FROM		Ident.SamlProductSettings
	WHERE	ProductId = @ProductId
END
GO
