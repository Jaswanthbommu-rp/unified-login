CREATE PROCEDURE [Ident].[ListIdentityProviderByIdentityProviderTypeName]
( 
				 @IdentityProviderTypeName varchar(50)
)
AS
BEGIN
	DECLARE @ProviderGroupTable TABLE ( ProviderFilter VARCHAR(20) )

	INSERT INTO @ProviderGroupTable (ProviderFilter ) VALUES ( @IdentityProviderTypeName )

	IF @IdentityProviderTypeName IN ( 'SAML', 'OIDC' )
	BEGIN
		DELETE FROM @ProviderGroupTable
		INSERT INTO @ProviderGroupTable ( ProviderFilter ) VALUES ( @IdentityProviderTypeName+'%' )
	END

	IF @IdentityProviderTypeName IN ( 'ALL' )
	BEGIN
		DELETE FROM @ProviderGroupTable
		INSERT INTO @ProviderGroupTable ( ProviderFilter ) VALUES ( '%' )
	END

	SELECT pvt.AuthenticationMode, 
			CONVERT(bit, pvt.ValidateIssuer) AS ValidateIssuer, 
			@IdentityProviderTypeName AS ProviderName, 
			pvt.Description, 
			pvt.AuthenticationType, 
			pvt.Caption, 
			pvt.ProviderClientId, 
			pvt.AuthorityUri, 
			pvt.PostLogoutRedirectUri, 
			pvt.RedirectUri, 
			pvt.TokenValidationAuthenticationType, 
			pvt.Scope, 
			pvt.EntityId, 
			pvt.MetadataLocation, 
			pvt.ClientSecret, 
			pvt.ValidAudience,
			pvt.UserLoginClaim,
			pvt.SigningBehavior,
			pvt.NameIdFormat
		FROM
		(
			SELECT ipt.ContactMechanismId, ipt.IdentityProviderTypeId, ipt.Name AS IdentityTypeName, ipt.Description, ipst.Name AS SettingTypeName, ips.Value
			FROM Ident.IdentityProviderType IPT
				 INNER JOIN
				 [Ident].[IdentityProviderSettingType] ipst
				 ON ipst.IdentityProviderTypeId = IPT.IdentityProviderTypeId
				 INNER JOIN
				 [Ident].[IdentityProviderSetting] ips
				 ON ipst.IdentityProviderSettingTypeId = ips.IdentityProviderSettingTypeId
				 INNER JOIN @ProviderGroupTable PGT
				 ON ipt.Name LIKE PGT.ProviderFilter
		) p PIVOT(MAX(Value) FOR SettingTypeName 
			IN(
				[AuthenticationType], 
				[Caption], 
				[ProviderClientId], 
				[AuthorityUri], 
				[PostLogoutRedirectUri], 
				[RedirectUri], 
				[AuthenticationMode], 
				[ValidateIssuer], 
				[TokenValidationAuthenticationType], 
				[Scope], 
				[EntityId], 
				[MetadataLocation], 
				[ClientSecret], 
				[ValidAudience],
				[UserLoginClaim],
				[SigningBehavior],
				[NameIdFormat]
			)) AS pvt
END;

