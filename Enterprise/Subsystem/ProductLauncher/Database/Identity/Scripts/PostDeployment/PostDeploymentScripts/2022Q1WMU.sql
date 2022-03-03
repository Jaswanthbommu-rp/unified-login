IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'TokenAuthScopes')
BEGIN
    INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
    VALUES ('TokenAuthScopes', 'Unified Login token scopes to request for product API authentication', 0);
END

-- Add GetAccessTypesEndpoint product setting

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'GetAccessTypesEndpoint')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('GetAccessTypesEndpoint', 'Access Type endpoint for product API', 0);
END

GO

-- RUM API Configuration
DECLARE @NOW DATETIME = GETUTCDATE(); 
declare @productlist table ( entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(2000))
insert into @productlist values 
(18, 'GetRoleEndpoint', '/roles/{0}'),
(18, 'GetAccessTypesEndpoint', '/user/accessTypes/{0}?username={1}'),
(18, 'PostUserEndpoint', '/user'),
(18, 'GetUserEndpoint', '/user?loginName={0}'),
(18, 'PatchProfileEndpoint', '/user/profiles'),
(18, 'PutUserEndpoint', '/user'),
(18, 'PatchMigrateUsersEndpoint', '/user/{0}/migrate'),
(18, 'GetPropertyByGroupEndpoint', '/group/{0}/properties'),
(18, 'GetPropertyEndpoint', '/properties/{0}'),
(18, 'GetPropertyGroupsEndpoint', '/propertyGroups/{0}'),
(18, 'GetListUsersEndpoint', '/user/{companyId}'),
(18, 'ProductIntegrationType', 'Standard v1'),
(18, 'TokenAuthScopes', 'greenbooknwpapi');

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
	if exists ( select top 1 1 from enterprise.product WHERE ProductId = @CurrentProductId )
	begin
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
	end	
	set @Current_ID = @Current_ID + 1
end
GO

IF NOT EXISTS (SELECT TOP 1 1
FROM Auth.Clients c
	INNER JOIN Auth.ClientScopes cs on cs.ClientId = c.ClientId
WHERE ClientCode = 'unifiedlogin-server'
	AND cs.Scope = 'greenbooknwpapi')
BEGIN
	INSERT INTO Auth.ClientScopes (ClientId, Scope)
	SELECT ClientId, 'greenbooknwpapi'
	FROM Auth.Clients
	WHERE ClientCode = 'unifiedlogin-server'
END

GO

-- Add IsEditUserRequiresProduct product setting

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'IsEditUserRequiresProduct')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('IsEditUserRequiresProduct', 'Does edit user requires product', 0);
END

GO

-- Add IsGreenbookCaresCheckRequired product setting

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'IsGreenbookCaresCheckRequired')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('IsGreenbookCaresCheckRequired', 'Does greenbook check flag is required to add product', 0);
END

GO