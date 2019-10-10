GO

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'AuthenticationType' )
begin
	insert into enterprise.ProductSettingType ( name, Description ) values ( 'AuthenticationType', 'Used to determine how to log into the product')
end

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'Authentication_SAML_RelayState' )
begin
	insert into enterprise.ProductSettingType ( name, Description ) values ( 'Authentication_SAML_RelayState', 'The relay state to use building a SAML auth request')
end

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'Authentication_SAML_FallbackUrl' )
begin
	insert into enterprise.ProductSettingType ( name, Description ) values ( 'Authentication_SAML_FallbackUrl', 'The fallback url to use building a SAML auth request')
end

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'Authentication_OpenId_ProductName' )
begin
	insert into enterprise.ProductSettingType ( name, Description ) values ( 'Authentication_OpenId_ProductName', 'The product name to use when building the openid request')
end

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'Authentication_OpenId_ResponseType' )
begin
	insert into enterprise.ProductSettingType ( name, Description ) values ( 'Authentication_OpenId_ResponseType', 'The response type to use when building the openid request')
end

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'Authentication_OpenId_ScopesForAuth' )
begin
	insert into enterprise.ProductSettingType ( name, Description ) values ( 'Authentication_OpenId_ScopesForAuth', 'The scopes to use when building the openid request')
end

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'Authentication_OpenId_ResponseMode' )
begin
	insert into enterprise.ProductSettingType ( name, Description ) values ( 'Authentication_OpenId_ResponseMode', 'The response mode to use when building the openid request')
end

if not exists ( select top 1 1 from Enterprise.ProductSettingType where name = 'MarketingCenterApiSourceID' )
begin
	insert into enterprise.ProductSettingType ( name, Description ) values ( 'MarketingCenterApiSourceID', 'Marketing Center Product ApiSourceID is used while intial company user set up process')
end
-- AuthenticationType , Redirect, SAML, OpenIdCustom, NA

DECLARE @NOW DATETIME = GETUTCDATE(); 
declare @productlist table ( entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(2000))
insert into @productlist values 
	(1, 'AuthenticationType', 'SAML' ),
	(2, 'AuthenticationType', 'NA' ),
	(3, 'AuthenticationType', 'Redirect' ),
	(4, 'AuthenticationType', 'Redirect' ),
	(5, 'AuthenticationType', 'SAML' ),
	(6, 'AuthenticationType', 'SAML' ),
	(7, 'AuthenticationType', 'NA' ),
	(8, 'AuthenticationType', 'SAML' ),
	(8, 'Authentication_SAML_RelayState', 'userId' ),

	(9, 'AuthenticationType', 'SAML' ),
	(9, 'MarketingCenterApiSourceID', '261' ),

	(10, 'AuthenticationType', 'SAML' ),
	(11, 'AuthenticationType', 'SAML' ),
	(12, 'AuthenticationType', 'SAML' ),
	(13, 'AuthenticationType', 'SAML' ),
	(14, 'AuthenticationType', 'SAML' ),
	(14, 'Authentication_SAML_FallbackUrl', 'https://www.realpage.com/clientportal' ),
	(15, 'AuthenticationType', 'SAML' ),
	(16, 'AuthenticationType', 'Redirect' ),
	(17, 'AuthenticationType', 'SAML' ),
	(18, 'AuthenticationType', 'OpenIdCustom' ),

	(18, 'Authentication_OpenId_ProductName', 'rum' ),
	(18, 'Authentication_OpenId_ResponseType', 'id_token token' ),
	(18, 'Authentication_OpenId_ScopesForAuth', 'openid profile roles nwpscope apibrowser managerapi' ),
	(18, 'Authentication_OpenId_ResponseMode', 'form_post' ),

	(19, 'AuthenticationType', 'Redirect' ),
	(20, 'AuthenticationType', 'SAML' ),
	(21, 'AuthenticationType', 'SAML' ),
	(23, 'AuthenticationType', 'OpenIdCustom' ),
	(23, 'Authentication_OpenId_ProductName', 'onsite' ),
	(23, 'Authentication_OpenId_ResponseType', 'id_token token' ),
	(23, 'Authentication_OpenId_ScopesForAuth', 'openid profile userinfoapi' ),
	(23, 'Authentication_OpenId_ResponseMode', 'form_post' ),

	(24, 'AuthenticationType', 'Redirect' ),
	(25, 'AuthenticationType', 'OpenIdCustom' ),
	(25, 'Authentication_OpenId_ProductName', 'SelfProvisioningPortal' ),
	(25, 'Authentication_OpenId_ResponseType', 'id_token token' ),
	(25, 'Authentication_OpenId_ScopesForAuth', 'openid profile selfprovisioningportal' ),
	(25, 'Authentication_OpenId_ResponseMode', 'form_post' ),

	(26, 'AuthenticationType', 'Redirect' ),
	(27, 'AuthenticationType', 'Redirect' ),
	(28, 'AuthenticationType', 'Redirect' ),
	(29, 'AuthenticationType', 'Redirect' ),
	(30, 'AuthenticationType', 'Redirect' ),
	(31, 'AuthenticationType', 'Redirect' ),
	(32, 'AuthenticationType', 'Redirect' ),
	(33, 'AuthenticationType', 'Redirect' ),
	(35, 'AuthenticationType', 'Redirect' ),
	(36, 'AuthenticationType', 'NA' ),
	(37, 'AuthenticationType', 'SAML' ),
	(38, 'AuthenticationType', 'Redirect' ),
	(39, 'AuthenticationType', 'Redirect' ),
	(40, 'AuthenticationType', 'OpenIdCustom' ),
	(40, 'Authentication_OpenId_ProductName', 'leadmanagement' ),
	(40, 'Authentication_OpenId_ResponseType', 'code' ),
	(40, 'Authentication_OpenId_ScopesForAuth', 'openid profile userinfoapi' ),

	(41, 'AuthenticationType', 'OpenIdCustom' ),
	(41, 'Authentication_OpenId_ProductName', 'leadanalytics' ),
	(41, 'Authentication_OpenId_ResponseType', 'code' ),
	(41, 'Authentication_OpenId_ScopesForAuth', 'openid profile userinfoapi' ),

	(43, 'AuthenticationType', 'OpenIdCustom' ),
	(43, 'Authentication_OpenId_ProductName', 'settings-management' ),
	(43, 'Authentication_OpenId_ResponseType', 'code id_token token' ),
	(43, 'Authentication_OpenId_ScopesForAuth', 'openid profile roles offline_access rplandingapi unifiedsettingsapi settings-management-tool' ),
	(43, 'Authentication_OpenId_ResponseMode', 'form_post' ),

	(44, 'AuthenticationType', 'Redirect' ),
	(45, 'AuthenticationType', 'Redirect' ),
	(46, 'AuthenticationType', 'Redirect' ),
	(47, 'AuthenticationType', 'Redirect' ),
	(48, 'AuthenticationType', 'Redirect' )

--select * from @productlist

declare @MAX_ID INT
declare @Current_ID INT = 1
declare @CurrentProductId INT = 1

select @MAX_ID = max(entid) from @productlist

while @Current_ID <= @MAX_ID
begin
	declare @currentSettingType varchar(500)
	declare @currentsettingValue varchar(2000)

	select @CurrentProductId = productid , @currentSettingType = productsettingtype, @currentSettingValue = productsettingvalue
		from @productlist where entid = @Current_ID

	--print 'productid = ' + convert(varchar,@currentproductid)

	if not exists (
	select top 1 1 
		FROM Enterprise.GlobalProductConfiguration gpc  
		JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
		JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
		JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
			WHERE  gpc.ProductId = @CurrentProductId  
		AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		AND pst.Name = @currentSettingType
		AND ps.Value = @currentsettingValue
	)
	begin
		declare @currentproductconfigurationid INT
		select distinct top 1 @currentproductconfigurationid = pc.configurationid
			FROM Enterprise.GlobalProductConfiguration gpc  
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
				WHERE  gpc.ProductId = @CurrentProductId
			AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		order by pc.ConfigurationId desc

		if (@currentproductconfigurationid is not null)
		begin
			insert into enterprise.ProductSetting ( productid, ProductSettingTypeId, value, FromDate )
				select @CurrentProductId, productsettingtypeid, @currentSettingValue, GETUTCDATE()
					from enterprise.ProductSettingType where name = @currentSettingType
			insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				values ( @currentproductconfigurationid, @@IDENTITY, GETUTCDATE(), null )
		end
	end
	
	set @Current_ID = @Current_ID + 1
end
GO
DECLARE  
@ProductId INT, 
@LoginURI NVARCHAR(100), 
@SigningCertificateThumbprint NVARCHAR(50), 
@ParentProductTypeId INT, 
@ProductName NVARCHAR(100)= 'Payments';
DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

--Create root product type
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM Enterprise.ProductType
    WHERE Name = 'Resident Services'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType]
             @ProductTypeId = 200,
             @ParentProductTypeId = NULL,
             @Name = 'Resident Services',
             @Description = 'Resident Services',
             @ProductTypeGUID = 'C9D127AA-E694-4394-8D6D-AADB2A37B50B';
    END;

SELECT @ParentProductTypeId = ProductTypeId
FROM Enterprise.ProductType
WHERE Name = 'Resident Services'
      AND ParentProductTypeId IS NULL;

IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM enterprise.ProductType
    WHERE Name = 'Payments'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType]
             @ProductTypeId = 206,
             @ParentProductTypeId = @ParentProductTypeId,
             @Name = @ProductName,
             @Description = 'Click Pay',
             @ProductTypeGUID = '52169fda-5c23-495d-b626-8c78be1cd11c';
    END;

SET @ProductId = 48;

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = @ProductName
)
    BEGIN
        EXEC Enterprise.CreateProduct
             @ProductId = @ProductId,
             @ProductGUID = 'bc8eea8f-f822-402e-a37d-8b14b431c8a6',
             @Name = @ProductName,
             @Description = @ProductName,
             @ProductTypeId = 206;
        UPDATE Enterprise.Product
          SET
              BooksProductCode = 'CPAY'
        WHERE ProductId = @ProductId;
    END;



INSERT INTO @ProductConfiguration
(SettingName,
SettingDescription,
SettingValue
)
VALUES
('ClientId','','1')
,('ClassName','','payments')
,('ProductUrl','','/product/payments')
,('TitleId','','Payments')
,('TitleUniqueId','','99d1a2c6-daa2-4b95-8a00-550d7f3442eb')
,('IsNewTab','','1')
,('MetatagUniqueId','','Payments')
,('IsResource','','0')
,('IsFavorite','','1')
,('LearnMore','','https://www.realpage.com/')
,('ApiEndPoint','','https://rfqa.novelpay.com/api/realpage/bluebook')
,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
,('ShowInUserDetails','Should the product show in the New/Edit user pages','1')
,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
,('ShowInAppSwitcher','Should the product show in the application switcher','1')
,('ShowInUserListFilter','Should the product show in the user list product pick list','1')
,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
,('LockOnProductAccess', '', '0')
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','1')

,('ApiUserName','','test+rfqarpunity@novelpay.com')
,('ApiPassword','','7mgp43EIvc8c!@')
,('ApiKey','','dGVzdCtyZnFhcnB1bml0eUBub3ZlbHBheS5jb206N21ncDQzRUl2YzhjIUA=')

,('GetCompanyEndpoint','GET Company Endpoint for API','/orgs/{0}')
,('GetParentCompanyEndpoint','GET Company Endpoint for API','/orgs?parentOrgId={0}')
,('GetRoleEndpoint','Role End point for product API','/roles?orgId={0}')
,('GetUserEndpoint','GET User Endpoint for product API','/users?login={0}')
,('GetProfileEndpoint','GET User Profile Endpoint for product API','/users/{0}/profile')
,('GetListUsersEndpoint','','/users?orgId={0}&filter={1}')
,('PostUserEndpoint','POST User Endpoint for product API','/users')
,('PutUserEndpoint','PUT User Endpoint for product API','/users') 
,('DeleteUserEndpoint','DELETE User Endpoint for product API','/users/{0}') 
,('PatchMigrateUsersEndpoint','Patch Migrate Users Endpoint', '/users/{0}/migrate')
,('PatchProfileEndpoint','PATCH Profile Endpoint for product API','/users/{0}/profile')



SET @ProductID = 48
SET @LoginURI = 'https://clickPay/LoginURL'
SET @SigningCertificateThumbprint = null



EXEC Enterprise.ProductConfigurationSetup @ProductId, @LoginURI, @SigningCertificateThumbprint, @ProductConfiguration;

GO
DECLARE @ClientId INT;
IF NOT EXISTS
(
    SELECT 1
    FROM [Auth].[Clients]
    WHERE ClientCode = 'pim' 
)
    BEGIN
        INSERT INTO [Auth].[Clients]
		([ClientCode], [ClientName], [ClientUri], [LogoUri], [Flow],
		 [LogoutUri], [IdentityTokenLifetime], [AccessTokenLifetime], [AuthorizationCodeLifetime],
		 [AbsoluteRefreshTokenLifetime], [SlidingRefreshTokenLifetime], [RefreshTokenUsage],
		 [RefreshTokenExpiration], [AccessTokenType], [UpdateAccessTokenOnRefresh],
		 [Enabled], [LogoutSessionRequired], [RequireSignOutPrompt], [AllowAccessToAllScopes],
		 [AllowClientCredentialsOnly], [RequireConsent], [AllowRememberConsent],
		 [EnableLocalLogin], [IncludeJwtId], [AlwaysSendClientClaims],
		 [PrefixClientClaims], [AllowAccessToAllGrantTypes]
		)
        VALUES
		(N'pim',	 N'Portfolio Invest Management',	 NULL,	 NULL,	 1,
		 NULL,	 36000,	 36000,	 36000,
		 0,	 0,	 0,
		 0,	 1,	 0,
		 1,	 1,	 0,	 1,
		 0,	 0,	 1,
		 1,	 1,	 1,
		 1, 1
	);
	SELECT @ClientId = SCOPE_IDENTITY();

    END;
IF @ClientId IS NOT NULL
   AND NOT EXISTS
		(
		SELECT 1
		FROM [Auth].[ClientScopes]
		WHERE ClientId = @ClientId
				AND [Scope] = 'openid'
		)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
		([ClientId],
		 [Scope]
		)
        VALUES
		(@ClientId,
		 N'openid'
		);
    END;

IF @ClientId IS NOT NULL
   AND NOT EXISTS
		(
		SELECT 1
		FROM [Auth].[ClientScopes]
		WHERE ClientId = @ClientId
				AND [Scope] = 'profile'
		)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
		([ClientId],
		 [Scope]
		)
        VALUES
		(@ClientId,
		 N'profile'
		);
    END;

	IF @ClientId IS NOT NULL
   AND NOT EXISTS
		(
		SELECT 1
		FROM [Auth].[ClientScopes]
		WHERE ClientId = @ClientId
				AND [Scope] = 'userinfoapi'
		)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
		([ClientId],
		 [Scope]
		)
        VALUES
		(@ClientId,
		 N'userinfoapi'
		);
    END;

	 

IF @ClientId IS NOT NULL
   AND NOT EXISTS
	(
		SELECT 1
		FROM [Auth].[ClientRedirectUris]
		WHERE ClientId = @ClientId
	)
    BEGIN
        INSERT INTO [Auth].[ClientRedirectUris]
		([ClientId],
		 Uri
		)
        VALUES
		(@ClientId,
		 'https://aim.realpage.com/pim-unitycallback/test'
		);
    
    END;
 GO
  DECLARE 
@ProductId INT, 
@LoginURI NVARCHAR(100), 
@SigningCertificateThumbprint NVARCHAR(50), 
@ParentProductTypeId INT, 
@ProductName NVARCHAR(100)= 'Deposit Alternative';
DECLARE @ProductConfiguration AS PRODUCTCONFIGURATIONTYPE;

--Create root product type

DECLARE @ApiEndPoint nvarchar(4000)
DECLARE @LoginURL nvarchar(4000)
DECLARE @ServerName sysname
SELECT @Servername = @@ServerName



--Create root product type
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM Enterprise.ProductType
    WHERE Name = 'Lease Management'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType]
             @ProductTypeId = 300,
             @ParentProductTypeId = NULL,
             @Name = 'Lease Management',
             @Description = 'Lease Management',
             @ProductTypeGUID = 'D174779E-9DD6-4D7D-A57F-21B3FAEF611F';
    END;

SELECT @ParentProductTypeId = ProductTypeId
FROM Enterprise.ProductType
WHERE Name = 'Lease Management'
      AND ParentProductTypeId IS NULL;
select @ParentProductTypeId
IF NOT EXISTS
(
    SELECT TOP 1 1
    FROM enterprise.ProductType
    WHERE Name = 'Deposit Alternative'
)
    BEGIN
        EXEC [Enterprise].[CreateProductType]
             @ProductTypeId = 310,
             @ParentProductTypeId = @ParentProductTypeId,
             @Name = @ProductName,
             @Description = 'Deposit Alternative',
             @ProductTypeGUID = '17398c48-ab80-443b-8729-4f8891e017ff';
    END;

SET @ProductId = 47;

IF NOT EXISTS
(
    SELECT 1
    FROM Enterprise.Product
    WHERE Name = @ProductName
)
    BEGIN
        EXEC Enterprise.CreateProduct
             @ProductId = @ProductId,
             @ProductGUID = '688899c2-ad58-4779-849c-3f7f638e3f02',
             @Name = @ProductName,
             @Description = @ProductName,
             @ProductTypeId = 310;
        UPDATE Enterprise.Product
          SET
              BooksProductCode = 'DIQ'
        WHERE ProductId = @ProductId;
    END;

IF @ServerName IN ( 'RCDUSODBSQL001', 'RCTUSODBSQL001')
BEGIN
	SET @APiEndPoint = 'https://depositiq-qa.realpage.com/unity_api'
END
IF @ServerName IN ( 'RCQUSODBSQL001', 'RCVEUSODBSQL001', 'RCDUSODBSQL001A','RCIUSODBSQL002', 'RCTUSODBSQL001A')
BEGIN
	SET @ApiEndPoint = 'https://depositiq-uat.realpage.com/unity_api'
END
IF @ServerName IN ( 'RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @ApiEndPoint = 'https://app.depositiq.com/unity_api'
END

INSERT INTO @ProductConfiguration
(SettingName,
SettingDescription,
SettingValue
)
VALUES
('ClientId','','1')
,('ClassName','','depositalternative')
,('ProductUrl','','/product/depositalternative')
,('TitleId','','Deposit Alternative')
,('TitleUniqueId','','082850ad-415a-454b-ab6a-7ee22d4550f7')
,('IsNewTab','','1')
,('MetatagUniqueId','','DepositAlternative')
,('IsResource','','0')
,('IsFavorite','','1')
,('LearnMore','','https://www.realpage.com/')
,('ProductStatus','Show if the external application was configured for the dashboard user.','8')
,('ShowInUserDetails','Should the product show in the New/Edit user pages','1')
,('ShowInRolesAndRights','Should the product show in the Role/Rights page','0')
,('ShowInAppSwitcher','Should the product show in the application switcher','1')
,('ShowInUserListFilter','Should the product show in the user list product pick list','1')
,('ProductAPIRequiresUser','Does the product require a user for api calls','0')
,('ProductNotAvailableForRegularUserNoEmail','Product Attribute for Product Not Available for Regular User No Email.','1')
,('LockOnProductAccess', '', '0')
,('ApiEndPoint','',@ApiEndPoint)
,('ApiUserName','','realpageunity')
,('ApiPassword','','MCuR699P47w9T6K734UM')
,('PostUserEndpoint','POST User Endpoint for product API','/users')
,('PutUserEndpoint','PUT User Endpoint for product API','/users') 
,('PatchProfileEndpoint','PATCH Profile Endpoint for product API','/users/profiles')
,('GetPropertyGroupsEndpoint','GET PropertyGroups Endpoint Endpoint for product API','/companies/{0}/groups')
,('GetRoleEndpoint','Role End point for product API','/roles')
,('GetUserEndpoint','GET User Endpoint for product API','/users?loginName={0}')
,('GetPropertyEndpoint','GET Property Endpoint for API','/properties/{0}')
,('GetListUsersEndpoint','','/users/{0}?filter={1}&startRow={2}&resultsperpage={3}')
,('PatchMigrateUsersEndpoint','', '/{0}/migrate-users')


SET @ProductID = 47
IF @ServerName IN ( 'RCDUSODBSQL001')
BEGIN
	SET @LoginURL = 'https://depositiq-uat.realpage.com/sso/unified-login'
END
IF @ServerName = 'RCTUSODBSQL001'
BEGIN
	SET @LoginURL = 'https://depositiq-qa.realpage.com/sso/unified-login'
END

IF @ServerName IN ( 'RCQUSODBSQL001', 'RCVEUSODBSQL001', 'RCDUSODBSQL001A','RCIUSODBSQL002', 'RCTUSODBSQL001A')
BEGIN
	SET @LoginURL = 'https://depositiq-qa.realpage.com/sso/unified-login'
END
IF @ServerName IN ( 'RCPGBKDBSQL005A', 'RCPGBKDBSQL005B')
BEGIN
	SET @LoginURL = 'https://app.depositiq.com/sso/unified-login'
END





SET @LoginURI = @LoginURL
SET @SigningCertificateThumbprint = null



EXEC Enterprise.ProductConfigurationSetup @ProductId, @LoginURI, @SigningCertificateThumbprint, @ProductConfiguration;

GO

GO
DECLARE @ClientId INT;
IF NOT EXISTS
(
    SELECT 1
    FROM [Auth].[Clients]
    WHERE ClientCode = 'DepositAlternative'
          AND [ClientName] = 'Deposit Alternative'
)
    BEGIN
        INSERT INTO [Auth].[Clients]
		([ClientCode], [ClientName], [ClientUri], [LogoUri], [Flow],
		 [LogoutUri], [IdentityTokenLifetime], [AccessTokenLifetime], [AuthorizationCodeLifetime],
		 [AbsoluteRefreshTokenLifetime], [SlidingRefreshTokenLifetime], [RefreshTokenUsage],
		 [RefreshTokenExpiration], [AccessTokenType], [UpdateAccessTokenOnRefresh],
		 [Enabled], [LogoutSessionRequired], [RequireSignOutPrompt], [AllowAccessToAllScopes],
		 [AllowClientCredentialsOnly], [RequireConsent], [AllowRememberConsent],
		 [EnableLocalLogin], [IncludeJwtId], [AlwaysSendClientClaims],
		 [PrefixClientClaims], [AllowAccessToAllGrantTypes]
		)
        VALUES
		(N'DepositAlternative',	 N'Deposit Alternative',	 NULL,	 NULL,	 0,
		 NULL,	 36000,	 36000,	 36000,
		 86400,	 36000,	 1,
		 36000,	 0,	 1,
		 1,	 1,	 0,	 1,
		 0,	 0,	 1,
		 1,	 1,	 1,
		 1, 1
	);
        SELECT @ClientId = SCOPE_IDENTITY();

	INSERT INTO Auth.ClientSecrets( ClientId, Value, Description, Expiration )
	VALUES( @ClientId, 'RnLaeOYVnJjS7Qzy+l/93sveiwaoc7G26bHTYgSXiVg=', 'DIQ-Sec-33B5F798-xxxx-xxxx-xxxx-0025B903DC3B', '2099-12-31 00:00:00.0000000 -06:00' );

	 
    END;
IF @ClientId IS NOT NULL
   AND NOT EXISTS
		(
		SELECT 1
		FROM [Auth].[ClientScopes]
		WHERE ClientId = @ClientId
				AND [Scope] = 'openid'
		)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
		([ClientId],
		 [Scope]
		)
        VALUES
		(@ClientId,
		 N'openid'
		);
    END;

IF @ClientId IS NOT NULL
   AND NOT EXISTS
		(
		SELECT 1
		FROM [Auth].[ClientScopes]
		WHERE ClientId = @ClientId
				AND [Scope] = 'profile'
		)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
		([ClientId],
		 [Scope]
		)
        VALUES
		(@ClientId,
		 N'profile'
		);
    END;

   IF @ClientId IS NOT NULL
   AND NOT EXISTS
		(
		SELECT 1
		FROM [Auth].[ClientScopes]
		WHERE ClientId = @ClientId
				AND [Scope] = 'userinfoapi'
		)
    BEGIN
        INSERT INTO [Auth].[ClientScopes]
		([ClientId],
		 [Scope]
		)
        VALUES
		(@ClientId,
		 N'userinfoapi'
		);
    END;

	 

IF @ClientId IS NOT NULL
   AND NOT EXISTS
	(
		SELECT 1
		FROM [Auth].[ClientRedirectUris]
		WHERE ClientId = @ClientId
	)
    BEGIN
        INSERT INTO [Auth].[ClientRedirectUris]
		([ClientId],
		 Uri
		)
        VALUES
		(@ClientId,
		 'https://diq/oauth/redirect'
		);
    
    END;
GO

DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;
DECLARE @TRightShortName NVARCHAR(100);
DECLARE @TargetProductId INT;
DECLARE @VisibilityStatusID INT;


IF OBJECT_ID('tempdb..#RightsUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #RightsUnifiedSettings;
END;

IF OBJECT_ID('tempdb..#HoldPartyForUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyForUnifiedSettings;
END;

CREATE TABLE #RightsUnifiedSettings
( 
			 RightId int, Name nvarchar(500), description nvarchar(500), shortname varchar(100)
);

INSERT INTO #RightsUnifiedSettings( rightid, name, description, shortname )
VALUES( 1, 'Manage ClickPay Product Access', 
'', 
'ManageClickPayProductAccess' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%Management%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Payments';

SET @ActionValueID = 1

SELECT @RoleCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'System';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'System';


SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
	 JOIN
	 Enterprise.StatusTypeCategory
	 ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification
	 ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType
	 ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'ALL' AND 
	  StatusTypeCategoryType.Name = 'Security';

SET @VisibilityStatusId = @Status;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Manage ClickPay Product Access' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Manage ClickPay Product Access', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'Dashboard' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Manage ClickPay Product Access' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Manage ClickPay Product Access', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
--WHERE O.PartyId = 350;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = 'User Administrator' AND 
		  R.PartyId = @PartyId;
	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #RightsUnifiedSettings;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ManageClickPayProductAccess', @ShortName = 'ManageClickPayProductAccess', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage ClickPay Product Access' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_ManageClickPayProductAccess';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage ClickPay Product Access' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;



		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

DECLARE @Dashboard int;

SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ManageClickPayProductAccess';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage ClickPay Product Access' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @DashBoard
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @DashBoard );

END;
GO


DECLARE @OrgRowNum INT;
DECLARE @ActionID INT;
DECLARE @RightID INT;
DECLARE @RoleID INT;
DECLARE @Status INT;
DECLARE @ActionValueID INT;
DECLARE @OrgID INT;
DECLARE @ProductID INT;
DECLARE @ParentActionId INT;
DECLARE @UserActionId INT;
DECLARE @RightCategoryId INT;
DECLARE @PartyId INT;
DECLARE @RightName VARCHAR(100);
DECLARE @RVT INT;
DECLARE @DefaultRoute NVARCHAR(200);
DECLARE @RightValueTypeId INT;
DECLARE @StatusId INT;
DECLARE @PersonaId INT;
DECLARE @FromDate DATETIME;
DECLARE @TRoleId INT;
DECLARE @TRoleName NVARCHAR(500);
DECLARE @TRoleDesc NVARCHAR(500);
DECLARE @TRightId INT;
DECLARE @TRightName NVARCHAR(500);
DECLARE @TRightDesc NVARCHAR(500);
DECLARE @RightCategory INT;
DECLARE @RoleCategory INT;
DECLARE @RoleName NVARCHAR(500);
DECLARE @RoleTypeID INT;
DECLARE @PerosonaP INT;
DECLARE @PartyRowNum INT;
DECLARE @TRightShortName NVARCHAR(100);
DECLARE @TargetProductId INT;
DECLARE @VisibilityStatusID INT;


IF OBJECT_ID('tempdb..#RightsUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #RightsUnifiedSettings;
END;

IF OBJECT_ID('tempdb..#HoldPartyForUnifiedSettings') IS NOT NULL
BEGIN
	DROP TABLE #HoldPartyForUnifiedSettings;
END;

CREATE TABLE #RightsUnifiedSettings
( 
			 RightId int, Name nvarchar(500), description nvarchar(500), shortname varchar(100)
);

INSERT INTO #RightsUnifiedSettings( rightid, name, description, shortname )
VALUES( 1, 'Manage Deposit Alternative Product Access', 
'', 
'ManageDepositAlternativeProductAccess' );

SELECT @ProductId = ProductId
FROM Enterprise.Product
WHERE name = 'Unified Platform';

--select * from enterprise.product where name like '%Management%'

SELECT @TargetProductId = ProductId
FROM Enterprise.Product
WHERE Name = 'Deposit Alternative';

SET @ActionValueID = 1

SELECT @RoleCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Role Type' AND 
	  ST.Name = 'System';

SELECT @RightCategory = ST.StatusTypeId
FROM Enterprise.StatusTypeCategoryType AS STCT
	 JOIN
	 Enterprise.StatusTypeCategory AS STC
	 ON STCT.StatusTypeCategoryTypeId = STC.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification AS STCC
	 ON STCC.StatusTypeCategoryId = STC.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType AS ST
	 ON ST.StatusTypeId = STCC.StatusTypeId
WHERE STC.Name = 'Right Type' AND 
	  ST.Name = 'System';


SELECT @Status = StatusType.StatusTypeID
FROM Enterprise.StatusTypeCategoryType
	 JOIN
	 Enterprise.StatusTypeCategory
	 ON StatusTypeCategory.StatusTypeCategoryTypeId = StatusTypeCategoryType.StatusTypeCategoryTypeId
	 JOIN
	 Enterprise.StatusTypeCategoryClassification
	 ON StatusTypeCategoryClassification.StatusTypeCategoryId = StatusTypeCategory.StatusTypeCategoryId
	 JOIN
	 Enterprise.StatusType
	 ON StatusType.StatusTypeId = StatusTypeCategoryClassification.StatusTypeId
WHERE StatusType.name = 'ALL' AND 
	  StatusTypeCategoryType.Name = 'Security';

SET @VisibilityStatusId = @Status;

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Manage Deposit Alternative Product Access' AND 
		  ParentActionId IS NULL
)
BEGIN
	EXEC Enterprise.CreateAction @ProductID = @ProductId, @Action = N'Manage Deposit Alternative Product Access', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT @ParentActionId = ActionId
FROM Enterprise.ACTION
WHERE ObjectValue = 'Dashboard' AND 
	  ObjectType = 'Route' AND 
	  Description = 'SuperUser';

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.ACTION
	WHERE ObjectValue = 'Manage Deposit Alternative Product Access' AND 
		  ParentActionID = @ParentActionId
)
BEGIN
	EXEC [Enterprise].[CreateAction] @ProductID = @ProductId, @Action = N'Manage Deposit Alternative Product Access', @ActionTarget = N'Right', @ActionbValueTypeId = 1, @Description = '', @ParentActionID = @ParentActionId, @ActionID = @ActionID OUTPUT;
	SELECT @ActionID AS N'@ActionID';
END;

SELECT DISTINCT 
	   IDENTITY(int, 1, 1) AS RowNumber, OrganizationPartyID, 0 AS PStatus
INTO #HoldPartyForUnifiedSettings
FROM Person.Persona AS P
	 INNER JOIN
	 Enterprise.Organization AS O
	 ON P.OrganizationPartyId = O.PartyId
--WHERE O.PartyId = 350;

WHILE EXISTS
(
	SELECT 1
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0
)
BEGIN
	SELECT TOP 1 @PartyRowNum = Rownumber, @PartyId = OrganizationPartyID
	FROM #HoldPartyForUnifiedSettings
	WHERE PStatus = 0;
	SELECT @RoleId = RoleId
	FROM Enterprise.Role AS R
		 INNER JOIN
		 Enterprise.RoleValueType AS RR
		 ON RR.RoleValueTypeId = R.RoleValueTypeId
	WHERE RR.Value = 'User Administrator' AND 
		  R.PartyId = @PartyId;
	DECLARE Rights CURSOR
	FOR SELECT RightId, Name, Description, ShortName
		FROM #RightsUnifiedSettings;
	OPEN Rights;
	FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXECUTE Enterprise.CreateRight @RoleId = -1, @RightName = 'Default_ManageDepositAlternativeProductAccess', @ShortName = 'ManageDepositAlternativeProductAccess', @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Description = '',  @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;
		EXECUTE Enterprise.CreateRight @RoleId = @RoleId, @RightName = @TRightName, @RightCategoryId = @RightCategory, @PartyId = @PartyId, @ProductId = @ProductId, @Shortname = @TRightShortName, @Description = @TRightDesc, @TargetProductId = @TargetProductId, @VisibilityStatusId = @VisibilityStatusId, @RightId = @RightId OUTPUT;


		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage Deposit Alternative Product Access' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;

		SELECT @RightId = RightId
		FROM Enterprise.[Right] AS R
			 INNER JOIN
			 Enterprise.RightValueType AS RR
			 ON RR.RightValueTypeId = R.RightValueTypeId
		WHERE RR.Value = 'Default_ManageDepositAlternativeProductAccess';
		SELECT @ActionID = ActionID
		FROM Enterprise.ACTION
		WHERE ObjectValue = 'Manage Deposit Alternative Product Access' AND 
			  ObjectType = 'Right' AND 
			  ParentActionId IS NULL;
		EXEC [Enterprise].[LinkActionToRights] @ActionID = @ActionID, @RightId = @RightId, @StatusId = @Status, @UserActionId = @UserActionId OUTPUT;



		FETCH Rights INTO @TRightId, @TRightName, @TRightDesc, @TRightShortName;
	END;
	CLOSE Rights;
	DEALLOCATE Rights;
	UPDATE #HoldPartyForUnifiedSettings
	  SET PStatus = 1
	WHERE RowNumber = @PartyRowNum;
END;

DECLARE @Dashboard int;

SELECT @DashBoard = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value = 'Default_ManageDepositAlternativeProductAccess';

SELECT @RightValueTypeId = RightValueTypeId
FROM Enterprise.RightValueType
WHERE value IN( 'Manage Deposit Alternative Product Access' );

IF NOT EXISTS
(
	SELECT 1
	FROM Enterprise.RightDependency
	WHERE RightValueTypeId = @RightValueTypeId AND 
		  DependentRightValueTypeId = @DashBoard
)
BEGIN
	INSERT INTO Enterprise.RightDependency( RightValueTypeId, DependentRightValueTypeId )
	VALUES( @RightValueTypeId, @DashBoard );

END;
--GO;


--DECLARE @JSON NVARCHAR(MAX)
--DECLARE @MasterSettingId BIGINT
----DECLARE @PartyId BIGINT
--DECLARE @UserLoginId BIGINT
--DECLARE @AlphaId INT
--DECLARE @NumrId INT
--DECLARE @Result TABLE(
--	 [PartyId] BIGINT
--	,[UserLoginId] BIGINT
--	,[MasterSettingId] BIGINT
--	,[Id] BIGINT  
--	,[Sequence]INT  
--	,[FieldName] NVARCHAR(255)
--	,[FieldValue] NVARCHAR(MAX)
--	,[FieldType] NVARCHAR(50)  
--	,[MinCharLength] INT    
--	,[MaxCharLength] INT 
--	,[IsRequiredField] bit  
--	,[isFieldEnabled] bit)


--	IF NOT EXISTS (SELECT 1 FROM CustomField.FieldType WHERE Name = 'Alphanumeric')
--	BEGIN
--		INSERT INTO CustomField.FieldType ([Name], [CreatedDate], [CreatedBy])
--		VALUES('Alphanumeric',GETDATE(),'11')
--		SET @AlphaId = SCOPE_IDENTITY();
--	END
--	ELSE
--		SELECT @AlphaId = FieldTypeId FROM CustomField.FieldType WHERE Name = 'Alphanumeric'

	

--	IF NOT EXISTS (SELECT 1 FROM CustomField.FieldType WHERE Name = 'Numeric')
--	BEGIN
--		INSERT INTO CustomField.FieldType ([Name], [CreatedDate], [CreatedBy])
--		VALUES('Numeric',GETDATE(),'11')
--		SET @NumrId = SCOPE_IDENTITY()
--	END
--	ELSE
--		SELECT @NumrId = FieldTypeId FROM CustomField.FieldType WHERE Name = 'Numeric'


--IF(SELECT COUNT(1) FROM CustomField.Field) = 0
--BEGIN
--	DECLARE ValueLooper CURSOR FOR SELECT Distinct EO.PartyId, PP.UserId, MS.MasterSettingId, Value 
--		FROM Enterprise.MasterSetting MS
--		INNER JOIN Enterprise.MasterSettingType MST ON MS.MasterSettingTypeId = MST.MasterSettingTypeId
--		INNER JOIN Enterprise.MasterConfigurationSetting MCS ON mcs.MasterSettingId = MS.MasterSettingId 
--		INNER JOIN Enterprise.MasterConfiguration MC ON mc.MasterConfigurationId = MCS.MasterConfigurationId
--		INNER JOIN Person.Persona PP ON (PP.UserId = MC.AttributeId)
--		INNER JOIN Enterprise.Organization EO ON (PP.OrganizationPartyId = EO.PartyId)
--	WHERE MST.Name = 'CustomFields' AND LEN(Value)<4000

--	OPEN Valuelooper
	
--	FETCH NEXT FROM ValueLooper INTO @PartyId, @UserLoginId, @MasterSettingId, @JSON

--	WHILE @@FETCH_STATUS  = 0
--	BEGIN
--		INSERT INTO @Result([PartyId], [UserLoginId], [MasterSettingId], [Id], [Sequence], [FieldName], [FieldValue], [FieldType], [MinCharLength], [MaxCharLength], [IsRequiredField], [isFieldEnabled])
--		SELECT @PartyId, @UserLoginId, @MasterSettingId, [Id], [Sequence], [FieldName], [FieldValue], [FieldType], [MinCharLength], [MaxCharLength],[IsRequiredField], [isFieldEnabled]
--		FROM OPENJSON(@JSON)
--		WITH ([Id] BIGINT  
--			,[Sequence]INT  
--			,[FieldName] NVARCHAR(255)
--			,[FieldValue] NVARCHAR(MAX)
--			,[FieldType] NVARCHAR(50)  
--			,[MinCharlength] INT    
--			,[MaxCharlength] INT 
--			,[isRequiredField] bit 
--			,[isFieldEnabled] bit)
--		WHERE LEN([FieldValue]) > 0

	


--		FETCH NEXT FROM ValueLooper INTO @PartyId, @UserLoginId, @MasterSettingId, @JSON

--	END

--	CLOSE ValueLooper;  
--	DEALLOCATE ValueLooper; 

--		INSERT INTO CustomField.Field
--			([OrganizationId]
--			,[DependentFieldId]
--			,[Enabled]
--			,[Name]
--			,[Description]
--			,[FieldTypeId]
--			,[Required]
--			,[ReadOnly]
--			,[DefaultValue]
--			,[SyncField]
--			,[Sequence]
--			,[HelpText]
--			,[MinCharLength]
--			,[MaxCharLength]
--			,[CreatedDate]
--			,[CreatedBy])
--		SELECT Distinct 
--			 PartyId
--			,NULL
--			,[isFieldEnabled]
--			,FieldName
--			,NULL
--			,CASE WHEN FieldType='alphanumeric' THEN @AlphaId WHEN FieldType='numeric' THEN @NumrId ELSE @AlphaId END as FieldTypeId
--			,[IsRequiredField]
--			,NULL
--			,NULL
--			,NULL
--			,Sequence
--			,NULL
--			,[MinCharLength]
--			,[MaxCharLength]
--			,GETDATE()
--			,'11' 
--		FROM @Result
--		ORDER BY PartyId, Sequence ASC

--		INSERT INTO CustomField.FieldValue
--			   ([UserLoginID]
--			   ,[FieldID]
--			   ,[Value]
--			   ,[CreatedDate]
--			   ,[CreatedBy])
--		SELECT DISTINCT
--			  R.UserLoginId
--			 ,F.FieldId
--			 ,R.FieldValue
--			 ,GETDATE()
--			 ,'11'
--		FROM @Result R
--		INNER JOIN CustomField.Field F ON F.OrganizationId = R.PartyId AND F.Name = R.FieldName and F.Sequence = R.Sequence

--END;
----GO;


--GO
--IF EXISTS (SELECT 1 FROM Enterprise.RIghtValueTYpe WHERE value  = 'Ability to Manage Unified Settings')
--BEGIN
--	UPDATE Enterprise.RightValueType
--		SET Value = 'Manage All Unified Settings'
--	WHERE Value  = 'Ability to Manage Unified Settings'
--END

--IF EXISTS (SELECT 1 FROM Enterprise.RIghtValueTYpe WHERE value = 'View Unified Settings')
--BEGIN
--	UPDATE Enterprise.RightValueType
--		SET Value = 'View all Unified Settings'
--	WHERE Value  = 'View Unified Settings'
--END
