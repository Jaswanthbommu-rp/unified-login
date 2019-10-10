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