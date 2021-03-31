GO
--Updating GetUserEndpoint for ILMLA and ILMLA products
DECLARE @ProductSettingTypeId INT
SELECT @ProductSettingTypeId = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE Name='GetUserEndpoint'
IF EXISTS(SELECT 1 FROM Enterprise.ProductSetting WHERE ProductId IN (40, 41) AND ProductSettingTypeId = @ProductSettingTypeId)
BEGIN
UPDATE Enterprise.ProductSetting SET value = '/{0}/users?loginName={1}'
WHERE ProductId IN (40, 41) AND ProductSettingTypeId = @ProductSettingTypeId
END
GO

-- Adding Rights to HomeSharing Product	
DECLARE @UserId INT
SELECT @UserId = UserId FROM Ident.UserLogin WHERE LoginName like 'realpagead@%'
IF NOT EXISTS(SELECT 1 FROM Security.[Right] WHERE RightName = 'PropertyAdmin')
BEGIN
	INSERT INTO Security.[Right](RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate)
	VALUES('PropertyAdmin', 'Property Admin', 'Property Admin', 13, 9, 60, 60, @UserId, GETDATE())
END
IF NOT EXISTS(SELECT 1 FROM Security.[Right] WHERE RightName = 'PropertyUser')
BEGIN
	INSERT INTO Security.[Right](RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate)
	VALUES('PropertyUser', 'Property User', 'Property User', 13, 9, 60, 60, @UserId, GETDATE())
END
DECLARE @PropertyAdminRoleId INT
DECLARE @PropertyUserRoleId INT		
DECLARE @PropertyAdminRightId INT
DECLARE @PropertyUserRightId INT		
SELECT @PropertyAdminRightId  = RightId FROM Security.[Right] WHERE RightName = 'PropertyAdmin' AND ProductId = 60;
SELECT @PropertyUserRightId = RightId FROM Security.[Right] WHERE RightName='PropertyUser' AND ProductId = 60;
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'Property Admin' AND OrgPartyID IS NULL AND ProductId = 60)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('Property Admin', 'Property Admin', 'Property Admin', 3, NULL, 60, @UserId, GETDATE())	
END	
SELECT @PropertyAdminRoleId = RoleId FROM Security.Role WHERE RoleName = 'Property Admin' AND OrgPartyID IS NULL AND ProductId = 60
IF NOT EXISTS(SELECT 1 FROM Security.RoleRight WHERE RoleId = @PropertyAdminRoleId AND RightId = @PropertyAdminRightId)
BEGIN
	INSERT INTO Security.RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
	VALUES(@PropertyAdminRoleId, @PropertyAdminRightId, @UserId, GETDATE())
END
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'Property User' AND OrgPartyID IS NULL AND ProductId = 60)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('Property User', 'Property User', 'Property User', 3, NULL, 60, @UserId, GETDATE())	
END
SELECT @PropertyUserRoleId = RoleId FROM Security.Role WHERE RoleName = 'Property User' AND OrgPartyID IS NULL AND ProductId = 60
	
IF NOT EXISTS(SELECT 1 FROM Security.RoleRight WHERE RoleId = @PropertyUserRoleId AND RightId = @PropertyUserRightId)
BEGIN
	INSERT INTO Security.RoleRight(RoleId, RightId, CreatedBy, CreatedDate)
	VALUES(@PropertyUserRoleId, @PropertyUserRightId, @UserId, GETDATE())
END
GO

 --Adding Product Setting Type for ILM & ILM LA
			   
  BEGIN TRAN

-- Add ProductIcon product settings

IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'CreateUpdateMultiCompanyUserRequiresPMC')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('CreateUpdateMultiCompanyUserRequiresPMC', 'Create Update MultiCompany User Requires PMC', 0);
END

DECLARE @NOW DATETIME = GETUTCDATE();
declare @productlist table ( entid int identity, productid int, productsettingtype varchar(500), productsettingvalue varchar(100))
insert into @productlist values
(40,  'CreateUpdateMultiCompanyUserRequiresPMC','1'),
(41,  'CreateUpdateMultiCompanyUserRequiresPMC','1');

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

COMMIT TRAN;
GO

--Accounting Location Group
Declare @FSMasterControlId int,@FSLocationGroupControlId int,@MaxControlId int,@MaxControlAttributeId int
DECLARE @UserId bigint,
	@ProductId int ,
	@Now datetime = GETDATE()

SELECT	@UserId = UserId
FROM	Ident.UserLogin
WHERE	LoginName LIKE 'realpagead@%'

Select @FSMasterControlId = ControlId From UserManagement.Control 
Where UIId = 'FinancialSuiteProductAccessTabGroupUIId' AND ControlTypeId = 8

Select @FSLocationGroupControlId = ControlId From UserManagement.Control 
Where UIId = 'FinancialSuiteProductAccessLocationGroupTabUIId' AND ControlTypeId = 9

update [UserManagement].[Control] set Sequence = 8
where uiid = 'FinancialSuiteProductAccessEntitiesTabUIId' and ControlTypeId = 9

IF NOT EXISTS (SELECT TOP 1 1 FROM[UserManagement].[Control] WHERE ControlId = @FSLocationGroupControlId)
BEGIN
	
	SET IDENTITY_INSERT [UserManagement].[Control] ON 
	SELECT @MaxControlId = max(ControlId) from UserManagement.Control

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@MaxControlId +1, @FSMasterControlId, 9, N'FinancialSuiteProductAccessLocationGroupTabUIId', N'Location Group', NULL, 7, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@MaxControlId +2, @MaxControlId +1, 3, N'FinancialSuiteProductAccessLocationGroupMultiSelectGridUIId', NULL, NULL, 1, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@MaxControlId +3, @MaxControlId +2, 10, N'FinancialSuiteProductAccessLocationGroupCheckboxUIId', NULL, N'isAssigned', 2, @UserId, @Now)

	INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
	VALUES (@MaxControlId +4, @MaxControlId +2, 5, N'FinancialSuiteProductAccessLocationGroupLabelUIId', N'Location Group', N'name', 3, @UserId, @Now)

	--PGSlide
			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (@MaxControlId +5, @MaxControlId +2, 11, N'FinancialSuiteProductAccessLocationGroupIconUIId', NULL, N'InfoIcon', 4, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (@MaxControlId +6, @MaxControlId +5, 5, N'FinancialSuiteProductAccessLocationGroupDetailsLabelUIId', N'Location Group Details', NULL, 1, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (@MaxControlId +7, @MaxControlId +5, 12, N'FinancialSuiteProductAccessLocationGroupDetailsGridUIId', NULL, NULL, 2, @UserId, @Now)

			INSERT [UserManagement].[Control] ([ControlId], [ParentControlId], [ControlTypeId], [UIId], [DisplayName], [DataSource], [Sequence], [CreatedBy], [CreatedDate])
			VALUES (@MaxControlId +8, @MaxControlId +7, 5, N'FinancialSuiteProductAccessLocationGroupDetailsPropertyLabelUIId', N'Entity', N'name', 2, @UserId, @Now)
			
			SET IDENTITY_INSERT [UserManagement].[Control] OFF
END

		SET IDENTITY_INSERT [UserManagement].[ControlAttribute] ON 
            
			SELECT @MaxControlAttributeId = max(ControlAttributeId) from UserManagement.ControlAttribute
			Declare @FSLGSelallID int,@FSLGinfoiconID int

			Select @FSLGSelallID = ControlId From UserManagement.Control 
            Where UIId = 'FinancialSuiteProductAccessLocationGroupMultiSelectGridUIId' AND ControlTypeId = 3

            Select @FSLGinfoiconID = ControlId From UserManagement.Control 
            Where UIId = 'FinancialSuiteProductAccessLocationGroupIconUIId' AND ControlTypeId = 11

			IF NOT EXISTS ( SELECT TOP 1 1 FROM [UserManagement].[ControlAttribute] WHERE ControlId = @FSLGSelallID)
			BEGIN
				INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
				VALUES (@MaxControlAttributeId + 1, @FSLGSelallID, N'ShowSelectAll', N'False', @UserId, @Now)
			END

			--SELECT @MaxControlAttributeId = max(ControlAttributeId) from UserManagement.ControlAttribute
			IF NOT EXISTS ( SELECT TOP 1 1 FROM [UserManagement].[ControlAttribute] WHERE ControlId = @FSLGinfoiconID)
			BEGIN
				INSERT [UserManagement].[ControlAttribute] ([ControlAttributeId], [ControlId], [Key], [Value], [CreatedBy], [CreatedDate]) 
				VALUES (@MaxControlAttributeId + 2, @FSLGinfoiconID, N'InfoIcon', N'Slide', @UserId, @Now)		
			END
			
            SET IDENTITY_INSERT [UserManagement].[ControlAttribute] OFF

GO