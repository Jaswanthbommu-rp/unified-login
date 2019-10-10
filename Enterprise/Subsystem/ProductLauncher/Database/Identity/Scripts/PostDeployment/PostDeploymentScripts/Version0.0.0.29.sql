
-- ====================== Start Dev Environment ======================
IF @@SERVERNAME = 'RCDUSODBSQL001'
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



	SELECT *
	FROM [Auth].[Clients]
	ORDER BY ClientId DESC;

-- Adding Clients

	DECLARE @ClientId int;

	IF NOT EXISTS
(
	SELECT 1
	FROM [Auth].[Clients]
	WHERE ClientCode = 'migration' AND 
		  [ClientName] = 'MigrationTool'
)
	BEGIN
		INSERT INTO [Auth].[Clients]( [ClientCode], [ClientName], [ClientUri], [LogoUri], [Flow], [LogoutUri], [IdentityTokenLifetime], [AccessTokenLifetime], [AuthorizationCodeLifetime], [AbsoluteRefreshTokenLifetime], [SlidingRefreshTokenLifetime], [RefreshTokenUsage], [RefreshTokenExpiration], [AccessTokenType], [UpdateAccessTokenOnRefresh], [Enabled], [LogoutSessionRequired], [RequireSignOutPrompt], [AllowAccessToAllScopes], [AllowClientCredentialsOnly], [RequireConsent], [AllowRememberConsent], [EnableLocalLogin], [IncludeJwtId], [AlwaysSendClientClaims], [PrefixClientClaims], [AllowAccessToAllGrantTypes] )
		VALUES( N'migration', N'MigrationTool', N'https://ulmtdev.corp.realpage.com', N'https://ulmtdev.corp.realpage.com', 1, NULL, 360, 3600, 360, 86400, 3600, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1 );
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
		VALUES( @ClientId, N'http://ulmtdev.corp.realpage.com' );
		INSERT INTO [Auth].[ClientRedirectUris]( [ClientId], [Uri] )
		VALUES( @ClientId, N'https://ulmtdev.corp.realpage.com/callback' );
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
		VALUES( N'migrationapi', N'Migration Tool API', N'https://ulmtdevapi.corp.realpage.com', N'https://ulmtdevapi.corp.realpage.com', 1, NULL, 360, 3600, 360, 86400, 3600, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1 );
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
		VALUES( @ClientId, N'https://ulmtdevapi.corp.realpage.com/swagger/ui/o2c-html' );
	END;
END;

-- ====================== End Dev Environment ======================

-- ====================== Start QA Server ======================

IF @@SERVERNAME = 'RCTUSODBSQL001'
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



	SELECT *
	FROM [Auth].[Clients]
	ORDER BY ClientId DESC;

-- Adding Clients



	IF NOT EXISTS
(
	SELECT 1
	FROM [Auth].[Clients]
	WHERE ClientCode = 'migration' AND 
		  [ClientName] = 'MigrationTool'
)
	BEGIN
		INSERT INTO [Auth].[Clients]( [ClientCode], [ClientName], [ClientUri], [LogoUri], [Flow], [LogoutUri], [IdentityTokenLifetime], [AccessTokenLifetime], [AuthorizationCodeLifetime], [AbsoluteRefreshTokenLifetime], [SlidingRefreshTokenLifetime], [RefreshTokenUsage], [RefreshTokenExpiration], [AccessTokenType], [UpdateAccessTokenOnRefresh], [Enabled], [LogoutSessionRequired], [RequireSignOutPrompt], [AllowAccessToAllScopes], [AllowClientCredentialsOnly], [RequireConsent], [AllowRememberConsent], [EnableLocalLogin], [IncludeJwtId], [AlwaysSendClientClaims], [PrefixClientClaims], [AllowAccessToAllGrantTypes] )
		VALUES( N'migration', N'MigrationTool', N'https://ulmtqa.corp.realpage.com', N'https://ulmtdev.corp.realpage.com', 1, NULL, 360, 3600, 360, 86400, 3600, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1 );
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
		VALUES( @ClientId, N'http://ulmtqa.corp.realpage.com' );
		INSERT INTO [Auth].[ClientRedirectUris]( [ClientId], [Uri] )
		VALUES( @ClientId, N'https://ulmtqa.corp.realpage.com/callback' );
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
		VALUES( N'migrationapi', N'Migration Tool API', N'https://ulmtqaapi.corp.realpage.com', N'https://ulmtdevapi.corp.realpage.com', 1, NULL, 360, 3600, 360, 86400, 3600, 0, 0, 1, 1, 1, 0, 0, 1, 0, 0, 1, 1, 1, 1, 1, 1 );
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
		VALUES( @ClientId, N'https://ulmtqaapi.corp.realpage.com/swagger/ui/o2c-html' );
	END;
END;

-- ====================== End QA Server ======================

 EXEC sys.sp_updateextendedproperty @name=N'Build', @value='30'