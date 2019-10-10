DECLARE @MasterSettingTypeId INT;
DECLARE @MasterConfigurationTypeId INT;
DECLARE @MasterConfigurationId INT;
DECLARE @MasterSettingId INT;

----Populate Base tables for Types
--SELECT @ServerName = @@ServerName;
--SELECT @DBName = DB_NAME();

--SET IDENTITY_INSERT [Enterprise].[MasterConfigurationType] ON;

--INSERT INTO [Enterprise].[MasterConfigurationType]( [MasterConfigurationTypeId], [Name] )
--VALUES( 1, N'Global' );

--INSERT INTO [Enterprise].[MasterConfigurationType]( [MasterConfigurationTypeId], [Name] )
--VALUES( 2, N'Organization' );

--INSERT INTO [Enterprise].[MasterConfigurationType]( [MasterConfigurationTypeId], [Name] )
--VALUES( 3, N'UserLogin' );

--INSERT INTO [Enterprise].[MasterConfigurationType]( [MasterConfigurationTypeId], [Name] )
--VALUES( 4, N'Persona' );

--INSERT INTO [Enterprise].[MasterConfigurationType]( [MasterConfigurationTypeId], [Name] )
--VALUES( 5, N'Product' );

--INSERT INTO [Enterprise].[MasterConfigurationType]( [MasterConfigurationTypeId], [Name] )
--VALUES( 6, N'Person' );

--INSERT INTO [Enterprise].[MasterConfigurationType]( [MasterConfigurationTypeId], [Name] )
--VALUES( 7, N'IdentityProvider' );

--SET IDENTITY_INSERT [Enterprise].[MasterConfigurationType] OFF;

--SET IDENTITY_INSERT [Enterprise].[MasterSettingType] ON;

--INSERT INTO [Enterprise].[MasterSettingType]( [MasterSettingTypeId], [ParentMasterSettingTypeId], [Name], [MasterConfigurationTypeId] )
--VALUES( 1, NULL, N'TimeZone', 1 );

--INSERT INTO [Enterprise].[MasterSettingType]( [MasterSettingTypeId], [ParentMasterSettingTypeId], [Name], [MasterConfigurationTypeId] )
--VALUES( 2, NULL, N'TimeZone', 2 );

--INSERT INTO [Enterprise].[MasterSettingType]( [MasterSettingTypeId], [ParentMasterSettingTypeId], [Name], [MasterConfigurationTypeId] )
--VALUES( 3, NULL, N'ThemeColor', 2 );

--INSERT INTO [Enterprise].[MasterSettingType]( [MasterSettingTypeId], [ParentMasterSettingTypeId], [Name], [MasterConfigurationTypeId] )
--VALUES( 4, NULL, N'TimeZone', 3 );

--INSERT INTO [Enterprise].[MasterSettingType]( [MasterSettingTypeId], [ParentMasterSettingTypeId], [Name], [MasterConfigurationTypeId] )
--VALUES( 5, NULL, N'TimeZone', 4 );

--INSERT INTO [Enterprise].[MasterSettingType]( [MasterSettingTypeId], [ParentMasterSettingTypeId], [Name], [MasterConfigurationTypeId] )
--VALUES( 6, NULL, N'ThemeColor', 4 );

--INSERT INTO [Enterprise].[MasterSettingType]( [MasterSettingTypeId], [ParentMasterSettingTypeId], [Name], [MasterConfigurationTypeId] )
--VALUES( 7, NULL, N'ThemeColor', 3 );

--INSERT INTO [Enterprise].[MasterSettingType]( [MasterSettingTypeId], [ParentMasterSettingTypeId], [Name], [MasterConfigurationTypeId] )
--VALUES( 8, NULL, N'IdentityServerCorsAllowedOrigins', 1 );

--INSERT INTO [Enterprise].[MasterSettingType]( [MasterSettingTypeId], [ParentMasterSettingTypeId], [Name], [MasterConfigurationTypeId] )
--VALUES( 9, NULL, N'LandingApiCorsAllowedOrigins', 1 );

--SET IDENTITY_INSERT [Enterprise].[MasterSettingType] OFF;

----Populate MasterSettingTable for TimeZones for all ConfigurationTypes

--DECLARE TMZ CURSOR
--FOR SELECT MST.MasterSettingTypeId
--	FROM Enterprise.MasterSettingType AS MST
--		 INNER JOIN
--		 Enterprise.MasterConfigurationType AS MCT
--		 ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
--	WHERE MST.Name = 'TimeZone';

--OPEN TMZ;

--FETCH TMZ INTO @MasterSettingTypeId;

--WHILE @@FETCH_STATUS = 0
--BEGIN
--	INSERT INTO Enterprise.MasterSetting( Value, MasterSettingTypeId, FromDate, ThruDate )
--		   SELECT name, @MasterSettingTypeId, GETUTCDATE(), NULL
--		   FROM sys.time_zone_info;
--	FETCH TMZ INTO @MasterSettingTypeId;
--END;

--CLOSE TMZ;

--DEALLOCATE TMZ;


----Populate MasterSettingTable for ThemeColor for ALL ConfigurationTypes

--DECLARE TMZ CURSOR
--FOR SELECT MST.MasterSettingTypeId, MCT.name
--	FROM Enterprise.MasterSettingType AS MST
--		 INNER JOIN
--		 Enterprise.MasterConfigurationType AS MCT
--		 ON MCT.MasterConfigurationTypeId = MST.MasterConfigurationTypeId
--	WHERE MST.Name = 'ThemeColor';

--OPEN TMZ;

--FETCH TMZ INTO @MasterSettingTypeId, @name;

--WHILE @@FETCH_STATUS = 0
--BEGIN
--	IF @Name = 'Organization'
--	BEGIN
--		INSERT INTO Enterprise.MasterSetting( Value, MasterSettingTypeId, FromDate, ThruDate )
--		VALUES( 'Dark', @MasterSettingTypeId, GETUTCDATE(), NULL );
--		INSERT INTO Enterprise.MasterSetting( Value, MasterSettingTypeId, FromDate, ThruDate )
--		VALUES( 'Light', @MasterSettingTypeId, GETUTCDATE(), NULL );
--	END;
--	IF @Name = 'Persona'
--	BEGIN
--		INSERT INTO Enterprise.MasterSetting( Value, MasterSettingTypeId, FromDate, ThruDate )
--		VALUES( 'Dark', @MasterSettingTypeId, GETUTCDATE(), NULL );
--		INSERT INTO Enterprise.MasterSetting( Value, MasterSettingTypeId, FromDate, ThruDate )
--		VALUES( 'Light', @MasterSettingTypeId, GETUTCDATE(), NULL );
--	END;
--	IF @Name = 'UserLogin'
--	BEGIN
--		INSERT INTO Enterprise.MasterSetting( Value, MasterSettingTypeId, FromDate, ThruDate )
--		VALUES( 'Dark', @MasterSettingTypeId, GETUTCDATE(), NULL );
--		INSERT INTO Enterprise.MasterSetting( Value, MasterSettingTypeId, FromDate, ThruDate )
--		VALUES( 'Light', @MasterSettingTypeId, GETUTCDATE(), NULL );
--	END;
--	FETCH TMZ INTO @MasterSettingTypeId, @name;
--END;

--CLOSE TMZ;

--DEALLOCATE TMZ;

----Populate date for Organization in MasterConFiguration table 


--SELECT @MasterConfigurationTypeId = MasterConfigurationTypeId
--FROM [Enterprise].[MasterConfigurationType]
--WHERE Name = 'Organization';

--INSERT INTO [Enterprise].[MasterConfiguration]( [MasterConfigurationTypeId], [AttributeId], [FromDate], [ThruDate] )
--	   SELECT @MasterConfigurationTypeId, PartyId, GETUTCDATE(), NULL
--	   FROM enterprise.organization;

----Populate UserLogin Date


--SELECT @MasterConfigurationTypeId = MasterConfigurationTypeId
--FROM [Enterprise].[MasterConfigurationType]
--WHERE Name = 'UserLogin';

--INSERT INTO [Enterprise].[MasterConfiguration]( [MasterConfigurationTypeId], [AttributeId], [FromDate], [ThruDate] )
--	   SELECT @MasterConfigurationTypeId, UserId, GETUTCDATE(), NULL
--	   FROM Ident.userLogin;

----Populate Persona

--SELECT @MasterConfigurationTypeId = MasterConfigurationTypeId
--FROM [Enterprise].[MasterConfigurationType]
--WHERE Name = 'Persona';

--INSERT INTO [Enterprise].[MasterConfiguration]( [MasterConfigurationTypeId], [AttributeId], [FromDate], [ThruDate] )
--	   SELECT @MasterConfigurationTypeId, PersonaId, GETUTCDATE(), NULL
--	   FROM Person.Persona;

----Populate Product

--SELECT @MasterConfigurationTypeId = MasterConfigurationTypeId
--FROM [Enterprise].[MasterConfigurationType]
--WHERE Name = 'Product';

--INSERT INTO [Enterprise].[MasterConfiguration]( [MasterConfigurationTypeId], [AttributeId], [FromDate], [ThruDate] )
--	   SELECT @MasterConfigurationTypeId, ProductId, GETUTCDATE(), NULL
--	   FROM Enterprise.Product;


----Populate MasterConfigurationSetting table for ThemeColor 
----UserLogin

--INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingid )
--	   SELECT MC.MasterConfigurationId, MS.MasterSettingId
--	   FROM Enterprise.MasterSetting AS MS
--			INNER JOIN
--			Enterprise.MasterSettingType AS MST
--			ON MS.MasterSettingTypeId = MST.MasterSettingTypeId
--			INNER JOIN
--			Enterprise.MasterConfigurationType AS MCT
--			ON MST.masterConfigurationTypeId = MCT.masterConfigurationTypeId
--			INNER JOIN
--			Enterprise.MasterConfiguration AS MC
--			ON MC.MasterConfigurationTypeId = MCT.MasterConfigurationTypeId
--	   WHERE MCT.Name = 'UserLogin' AND 
--			 MST.Name = 'ThemeColor' AND 
--			 MS.Value = 'Light';

----Organization

--INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingid )
--	   SELECT MC.MasterConfigurationId, MS.MasterSettingId
--	   FROM Enterprise.MasterSetting AS MS
--			INNER JOIN
--			Enterprise.MasterSettingType AS MST
--			ON MS.MasterSettingTypeId = MST.MasterSettingTypeId
--			INNER JOIN
--			Enterprise.MasterConfigurationType AS MCT
--			ON MST.masterConfigurationTypeId = MCT.masterConfigurationTypeId
--			INNER JOIN
--			Enterprise.MasterConfiguration AS MC
--			ON MC.MasterConfigurationTypeId = MCT.MasterConfigurationTypeId
--	   WHERE MCT.Name = 'Organization' AND 
--			 MST.Name = 'ThemeColor' AND 
--			 MS.Value = 'Light';



----Populate data for Global Setttings

--IF @ServerName = 'RCDUSODBSQL001' AND 
--   @DBName = 'IdentityDevelopMent'
--BEGIN
--	INSERT INTO Enterprise.MasterConfiguration( MasterCOnfigurationTYpeId, FromDate, ThruDate )
--	VALUES( 1, GETDATE(), NULL );
--	SELECT @MasterConfigurationId = SCOPE_IDENTITY();
--	SELECT @MasterSettingTypeId = [MasterSettingTypeId]
--	FROM [Enterprise].[MasterSettingType]
--	WHERE Name = N'IdentityServerCorsAllowedOrigins';
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://myllocal.corp.realpage.com', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://mylocal.corp.realpage.com', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'file://', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'http://localhost:555', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://ulmtlocal.corp.realpage.com/', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	SELECT @MasterSettingTypeId = [MasterSettingTypeId]
--	FROM [Enterprise].[MasterSettingType]
--	WHERE Name = N'LandingApiCorsAllowedOrigins';
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://landing.local', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://logindev.corp.realpage.com', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://ulmtlocal.corp.realpage.com/', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--END;

--IF @ServerName = 'RCDUSODBSQL001' AND 
--   @DBName = 'Identity'
--BEGIN
--	INSERT INTO Enterprise.MasterConfiguration( MasterCOnfigurationTYpeId, FromDate, ThruDate )
--	VALUES( 1, GETDATE(), NULL );
--	SELECT @MasterConfigurationId = SCOPE_IDENTITY();
--	SELECT @MasterSettingTypeId = [MasterSettingTypeId]
--	FROM [Enterprise].[MasterSettingType]
--	WHERE Name = N'IdentityServerCorsAllowedOrigins';
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://myllocal.corp.realpage.com', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://mylocal.corp.realpage.com', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'file://', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'http://localhost:555', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://ulmtdev.corp.realpage.com/', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	SELECT @MasterSettingTypeId = [MasterSettingTypeId]
--	FROM [Enterprise].[MasterSettingType]
--	WHERE Name = N'LandingApiCorsAllowedOrigins';
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://landing.local', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://logindev.corp.realpage.com', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://ulmtdev.corp.realpage.com/', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--END;

--IF @ServerName = 'RCTUSODBSQL001' AND 
--   @DBName = 'Identity'
--BEGIN
--	INSERT INTO Enterprise.MasterConfiguration( MasterCOnfigurationTYpeId, FromDate, ThruDate )
--	VALUES( 1, GETDATE(), NULL );
--	SELECT @MasterConfigurationId = SCOPE_IDENTITY();
--	SELECT @MasterSettingTypeId = [MasterSettingTypeId]
--	FROM [Enterprise].[MasterSettingType]
--	WHERE Name = N'IdentityServerCorsAllowedOrigins';
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://myllocal.corp.realpage.com', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://mylocal.corp.realpage.com', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'file://', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'http://localhost:555', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://ulmtqa.corp.realpage.com/', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	SELECT @MasterSettingTypeId = [MasterSettingTypeId]
--	FROM [Enterprise].[MasterSettingType]
--	WHERE Name = N'LandingApiCorsAllowedOrigins';
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://landing.local', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://logindev.corp.realpage.com', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
--	VALUES( @MasterSettingTypeId, 'https://ulmtqa.corp.realpage.com/', GETUTCDATE(), NULL );
--	SELECT @MasterSettingId = SCOPE_IDENTITY();
--	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
--	VALUES( @MasterConfigurationId, @MasterSettingId );
--END;


IF @ServerName = 'RCPGBKDBSQL005A' AND 
   @DBName = 'Identity'
BEGIN
	INSERT INTO Enterprise.MasterConfiguration( MasterCOnfigurationTYpeId, FromDate, ThruDate )
	VALUES( 1, GETDATE(), NULL );
	SELECT @MasterConfigurationId = SCOPE_IDENTITY();
	SELECT @MasterSettingTypeId = [MasterSettingTypeId]
	FROM [Enterprise].[MasterSettingType]
	WHERE Name = N'IdentityServerCorsAllowedOrigins';
	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
	VALUES( @MasterSettingTypeId, 'https://my.realpage.com', GETUTCDATE(), NULL );
	SELECT @MasterSettingId = SCOPE_IDENTITY();
	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
	VALUES( @MasterConfigurationId, @MasterSettingId );
	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
	VALUES( @MasterSettingTypeId, 'https://myl.realpage.com', GETUTCDATE(), NULL );
	SELECT @MasterSettingId = SCOPE_IDENTITY();
	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
	VALUES( @MasterConfigurationId, @MasterSettingId );
	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
	VALUES( @MasterSettingTypeId, 'file://', GETUTCDATE(), NULL );
	SELECT @MasterSettingId = SCOPE_IDENTITY();
	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
	VALUES( @MasterConfigurationId, @MasterSettingId );
	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
	VALUES( @MasterSettingTypeId, 'http://localhost:555', GETUTCDATE(), NULL );
	SELECT @MasterSettingId = SCOPE_IDENTITY();
	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
	VALUES( @MasterConfigurationId, @MasterSettingId );
	
	SELECT @MasterSettingTypeId = [MasterSettingTypeId]
	FROM [Enterprise].[MasterSettingType]
	WHERE Name = N'LandingApiCorsAllowedOrigins';
	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
	VALUES( @MasterSettingTypeId, 'https://my.realpage.com', GETUTCDATE(), NULL );
	SELECT @MasterSettingId = SCOPE_IDENTITY();
	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
	VALUES( @MasterConfigurationId, @MasterSettingId );
	INSERT INTO [Enterprise].[MasterSetting]( [MasterSettingTypeId], [Value], [FromDate], [ThruDate] )
	VALUES( @MasterSettingTypeId, 'https://myl.realpage.com', GETUTCDATE(), NULL );
	SELECT @MasterSettingId = SCOPE_IDENTITY();
	INSERT INTO Enterprise.MasterConfigurationSetting( MasterConfigurationId, MasterSettingId )
	VALUES( @MasterConfigurationId, @MasterSettingId );
	
END;

EXEC sys.sp_updateextendedproperty @name=N'Build', @value='32'