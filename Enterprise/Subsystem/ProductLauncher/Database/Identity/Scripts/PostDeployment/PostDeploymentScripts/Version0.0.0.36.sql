IF @@SERVERNAME IN ('RCQUSODBSQL001')
BEGIN
-- Adding Scopes

	IF NOT EXISTS
(
	SELECT 1
	FROM [Auth].[Scopes]
	WHERE Name = 'Migration' AND 
		  DisplayName = 'Migration Tool'
)
	BEGIN
		INSERT INTO [Auth].[Scopes]( [Name], [DisplayName], [Description], [ClaimsRule], [Enabled], [Required], [Emphasize], [Type], [IncludeAllClaimsForUser], [ShowInDiscoveryDocument], [AllowUnrestrictedIntrospection] )
		VALUES( N'migration', N'Migration Tool', NULL, NULL, 1, 0, 0, 1, 1, 1, 0 );
	END;

	IF NOT EXISTS
(
	SELECT 1
	FROM [Auth].[Scopes]
	WHERE Name = 'migrationapi' AND 
		  DisplayName = 'Migration Tool API'
)
	BEGIN
		INSERT INTO [Auth].[Scopes]( [Name], [DisplayName], [Description], [ClaimsRule], [Enabled], [Required], [Emphasize], [Type], [IncludeAllClaimsForUser], [ShowInDiscoveryDocument], [AllowUnrestrictedIntrospection] )
		VALUES( N'migrationapi', N'Migration Tool API', NULL, NULL, 1, 0, 0, 1, 1, 1, 0 );
	END;



-- Adding Clients

	SET @ClientId = NULL

	IF NOT EXISTS
(
	SELECT 1
	FROM [Auth].[Clients]
	WHERE ClientCode = 'migration' AND 
		  [ClientName] = 'MigrationTool'
)
	BEGIN
		INSERT INTO [Auth].[Clients]( [ClientCode], [ClientName], [ClientUri], [LogoUri], [Flow], [LogoutUri], [IdentityTokenLifetime], [AccessTokenLifetime], [AuthorizationCodeLifetime], [AbsoluteRefreshTokenLifetime], [SlidingRefreshTokenLifetime], [RefreshTokenUsage], [RefreshTokenExpiration], [AccessTokenType], [UpdateAccessTokenOnRefresh], [Enabled], [LogoutSessionRequired], [RequireSignOutPrompt], [AllowAccessToAllScopes], [AllowClientCredentialsOnly], [RequireConsent], [AllowRememberConsent], [EnableLocalLogin], [IncludeJwtId], [AlwaysSendClientClaims], [PrefixClientClaims], [AllowAccessToAllGrantTypes] )
		VALUES( N'migration', N'MigrationTool', N'https://ulmtsat.realpage.com/', N'https://ulmtsat.realpage.com/', 1, NULL, 360, 3600, 360, 86400, 3600, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1 );
		SELECT @ClientId = SCOPE_IDENTITY();
	END;
	IF @ClientId IS NOT NULL AND 
	   NOT EXISTS
(
	SELECT 1
	FROM [Auth].[ClientScopes]
	WHERE ClientId = @ClientId AND 
		  [Scope] = 'migration'
)
	BEGIN
		INSERT INTO [Auth].[ClientScopes]( [ClientId], [Scope] )
		VALUES( @ClientId, N'migration' );
	END;

	IF @ClientId IS NOT NULL AND 
	   NOT EXISTS
(
	SELECT 1
	FROM [Auth].[ClientRedirectUris]
	WHERE ClientId = @ClientId
)
	BEGIN
		INSERT INTO [Auth].[ClientRedirectUris]( [ClientId], Uri )
		VALUES( @ClientId, N'https://ulmtapisat.realpage.com/ ' );
		INSERT INTO [Auth].[ClientRedirectUris]( [ClientId], [Uri] )
		VALUES( @ClientId, N'https://ulmtsat.realpage.com/callback/index.html' );
	END;


	IF NOT EXISTS
(
	SELECT 1
	FROM [Auth].[Clients]
	WHERE ClientCode = 'migrationapi' AND 
		  [ClientName] = 'Migration Tool API'
)
	BEGIN
		INSERT INTO [Auth].[Clients]( [ClientCode], [ClientName], [ClientUri], [LogoUri], [Flow], [LogoutUri], [IdentityTokenLifetime], [AccessTokenLifetime], [AuthorizationCodeLifetime], [AbsoluteRefreshTokenLifetime], [SlidingRefreshTokenLifetime], [RefreshTokenUsage], [RefreshTokenExpiration], [AccessTokenType], [UpdateAccessTokenOnRefresh], [Enabled], [LogoutSessionRequired], [RequireSignOutPrompt], [AllowAccessToAllScopes], [AllowClientCredentialsOnly], [RequireConsent], [AllowRememberConsent], [EnableLocalLogin], [IncludeJwtId], [AlwaysSendClientClaims], [PrefixClientClaims], [AllowAccessToAllGrantTypes] )
		VALUES( N'migrationapi', N'Migration Tool API', N'https://ulmtsat.realpage.com/', N'https://ulmtsat.realpage.com/', 1, NULL, 360, 3600, 360, 86400, 3600, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1 );
		SELECT @ClientId = SCOPE_IDENTITY();

	END;


	IF @ClientId IS NOT NULL AND 
	   NOT EXISTS
(
	SELECT 1
	FROM [Auth].[ClientScopes]
	WHERE ClientId = @ClientId AND 
		  [Scope] = 'migrationapi'
)
	BEGIN
		INSERT INTO [Auth].[ClientScopes]( [ClientId], [Scope] )
		VALUES( @ClientId, N'migrationapi' );
	END;
	IF @ClientId IS NOT NULL AND 
	   NOT EXISTS
(
	SELECT 1
	FROM [Auth].[ClientRedirectUris]
	WHERE ClientId = @ClientId
)
	BEGIN
		INSERT INTO [Auth].[ClientRedirectUris]( [ClientId], [Uri] )
		VALUES( @ClientId, N'https://ulmtsat.realpage.com ' );
	END;
END;

--Populate master setting and configurations

IF @@SERVERNAME IN ('RCQUSODBSQL001')
BEGIN

	SELECT @MasterConfigurationId = MasterConfigurationId from EnterPrise.MasterConfiguration
	   WHERE MasterConfigurationTypeId = 1

	SELECT @MasterSettingTypeId = [MasterSettingTypeId]
	FROM [Enterprise].[MasterSettingType]
	WHERE Name = N'IdentityServerCorsAllowedOrigins';
	
	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
	VALUES( @MasterSettingTypeId, 'https://ulmtsat.realpage.com', GETUTCDATE(), NULL );
	SELECT @MasterSettingId = SCOPE_IDENTITY();
	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
	VALUES( @MasterConfigurationId, @MasterSettingId );
	SELECT @MasterSettingTypeId = [MasterSettingTypeId]
	FROM [Enterprise].[MasterSettingType]
	WHERE Name = N'LandingApiCorsAllowedOrigins';

	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
	VALUES( @MasterSettingTypeId, 'https://ulmtsat.realpage.com', GETUTCDATE(), NULL );
	SELECT @MasterSettingId = SCOPE_IDENTITY();
	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
	VALUES( @MasterConfigurationId, @MasterSettingId );
END;



EXEC sys.sp_updateextendedproperty @name=N'Build', @value='37'