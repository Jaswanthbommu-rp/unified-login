--User Story 952202
DECLARE @UserId INT,@AdminRoleId bigint,@ReadOnlyUser bigint,@StandardUser bigint,@rightId1 bigint,@rightId2 bigint,@rightId3 bigint,@rightId4 bigint,@rightId5 bigint,@rightId6 bigint;
SELECT @UserId = UserId FROM Ident.UserLogin WHERE LoginName like 'realpagead@%';
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'Administrator' AND OrgPartyID IS NULL AND ProductId = 38)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('Administrator', 'Administrator', 'Administrator', 3, NULL, 38, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'ReadOnlyUser' AND OrgPartyID IS NULL AND ProductId = 38)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('ReadOnlyUser', 'ReadOnlyUser', 'Read-Only User', 3, NULL, 38, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.Role WHERE RoleName = 'StandardUser' AND OrgPartyID IS NULL AND ProductId = 38)
BEGIN
	INSERT INTO Security.Role(RoleName, ShortName, Description, RoleTypeID, OrgPartyID, ProductId, CreatedBy, CreatedDate)
	VALUES('StandardUser', 'StandardUser', 'Standard User', 3, NULL, 38, @UserId, GETDATE())	
END	
IF NOT EXISTS(SELECT 1 FROM Security.[Right] WHERE RightName in ('AccesstoBids&ContractsinVendorMarketplace','ExecuteandCloseContracts','AwardBids','CreateEditorCancelBids', 
                                                                   'CreateEditorCancelContracts','ApproveorRejectContracts') )
BEGIN
	INSERT INTO Security.[Right](RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate)
	VALUES('AccesstoBids&ContractsinVendorMarketplace', 'Access to Bids & Contracts in Vendor Marketplace', 'Access to Bids & Contracts in Vendor Marketplace', 13, 9, 38, 38, @UserId, GETDATE())
	     ,('ExecuteandCloseContracts', 'Execute and Close Contracts', 'Execute and Close Contracts', 13, 9, 38, 38, @UserId, GETDATE())
		 ,('AwardBids', 'Award Bids', 'Award Bids', 13, 9, 38, 38, @UserId, GETDATE())
		 ,('CreateEditorCancelBids', 'Create, Edit, or Cancel Bids', 'Create, Edit, or Cancel Bids', 13, 9, 38, 38, @UserId, GETDATE())
		 ,('CreateEditorCancelContracts', 'Create, Edit, or Cancel Contracts', 'Create, Edit, or Cancel Contracts', 13, 9, 38, 38, @UserId, GETDATE())
		 ,('ApproveorRejectContracts', 'Approve or Reject Contracts', 'Approve or Reject Contracts', 13, 9, 38, 38, @UserId, GETDATE())
END
SELECT @AdminRoleId = RoleId FROM Security.Role WHERE RoleName = 'Administrator' AND OrgPartyID IS NULL AND ProductId = 38;
SELECT @ReadOnlyUser = RoleId FROM Security.Role WHERE RoleName = 'ReadOnlyUser' AND OrgPartyID IS NULL AND ProductId = 38;
SELECT @StandardUser = RoleId FROM Security.Role WHERE RoleName = 'StandardUser' AND OrgPartyID IS NULL AND ProductId = 38;
Select @rightId1  = RightId from Security.[Right] where RightName ='AccesstoBids&ContractsinVendorMarketplace';
Select @rightId2  = RightId from Security.[Right] where RightName ='ExecuteandCloseContracts';
Select @rightId3  = RightId from Security.[Right] where RightName ='AwardBids';
Select @rightId4  = RightId from Security.[Right] where RightName ='CreateEditorCancelBids';
Select @rightId5  = RightId from Security.[Right] where RightName ='ApproveorRejectContracts';
Select @rightId6  = RightId from Security.[Right] where RightName ='CreateEditorCancelContracts';
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @AdminRoleId and RightId in (@rightId1,@rightId2,@rightId3,@rightId4,@rightId5))
BEGIN
  INSERT INTO Security.RoleRight values (@AdminRoleId,@rightId1,@UserId,GETDATE())
                                       ,(@AdminRoleId,@rightId2,@UserId,GETDATE())
									   ,(@AdminRoleId,@rightId3,@UserId,GETDATE())
									   ,(@AdminRoleId,@rightId4,@UserId,GETDATE())
									   ,(@AdminRoleId,@rightId5,@UserId,GETDATE())
									   ,(@AdminRoleId,@rightId6,@UserId,GETDATE())
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @ReadOnlyUser and RightId in (@rightId1))
BEGIN
  INSERT INTO Security.RoleRight values (@ReadOnlyUser,@rightId1,@UserId,GETDATE())                     
END
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where  RoleId = @StandardUser and RightId in (@rightId1,@rightId4,@rightId6))
BEGIN
  INSERT INTO Security.RoleRight values (@StandardUser,@rightId1,@UserId,GETDATE()) 
                                       ,(@StandardUser,@rightId4,@UserId,GETDATE()) 
									   ,(@StandardUser,@rightId6,@UserId,GETDATE()) 
END
Declare @userAdminn bigint,@Rig bigint,@RId bigint;
SELECT @RId = RoleId from [Security].[Role] where RoleName='User Administrator';
IF NOT EXISTS (Select Top 1 1 from Security.[Right] where RightName = 'ManageVendorMarketplaceProductAccess')
BEGIN
  INSERT INTO Security.[Right](RightName, Description, Value, StatusTypeId, VisibilityStatusId, ProductId, TargetProductId, CreatedBy, CreatedDate)
	VALUES('ManageVendorMarketplaceProductAccess', 'Manage Vendor Marketplace Product Access', 'Manage Vendor Marketplace Product Access', 13, 9, 3, 38, @UserId, GETDATE())
END
Select @Rig = RightId from Security.[Right] where RightName = 'ManageVendorMarketplaceProductAccess';
IF NOT EXISTS (Select Top 1 1 from Security.RoleRight where RoleId =@RId and RightId = @Rig)
BEGIN
 Insert into Security.RoleRight values(@RId,@Rig,@UserId,GETDATE())
 END
GO

-- Add New Setting for Multi-family Orgtypes
IF NOT EXISTS (SELECT TOP (1) 1 FROM Enterprise.ProductSettingType WHERE Name = 'DisableUserManagementForOrgType' )
BEGIN
    INSERT INTO Enterprise.ProductSettingType
    (
        Name,
        Description,
        SensitiveData
    )
    VALUES
    (   N'DisableUserManagementForOrgType',    -- Name - nvarchar(50)
        'Enable product only for Multi-family Org type ',   -- Description - nvarchar(100)

        0 -- SensitiveData - tinyint
    )
END
-- Muti-family Setting type
DECLARE @NOW DATETIME = GETUTCDATE()
if NOT EXISTS (
	select TOP (1) 1 
		FROM Enterprise.GlobalProductConfiguration gpc  
		JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
		JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
		JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
			WHERE  gpc.ProductId = 38  
		AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
		AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		AND pst.Name = 'DisableUserManagementForOrgType'

	)
	BEGIN
		declare @currentproductconfigurationid INT
		select distinct TOP (1) @currentproductconfigurationid = pc.configurationid
			FROM Enterprise.GlobalProductConfiguration gpc  
			JOIN Enterprise.ProductConfiguration pc ON pc.ConfigurationId = gpc.ConfigurationId  
			JOIN Enterprise.ProductSetting ps ON ps.ProductSettingId = pc.ProductSettingId  
			JOIN Enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId  
				WHERE  gpc.ProductId = 38
			AND ((@NOW BETWEEN gpc.FromDate AND gpc.ThruDate) OR (@NOW >= gpc.FromDate AND gpc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN pc.FromDate AND pc.ThruDate) OR (@NOW >= pc.FromDate AND pc.ThruDate IS NULL))  
			AND ((@NOW BETWEEN ps.FromDate AND ps.ThruDate) OR (@NOW >= ps.FromDate AND ps.ThruDate IS NULL))  
		order by pc.ConfigurationId DESC
		if (@currentproductconfigurationid is not null)
		begin
			insert into enterprise.ProductSetting ( productid, ProductSettingTypeId, value, FromDate )
				select 38, productsettingtypeid, 'Vendor,Other', GETUTCDATE()
					from enterprise.ProductSettingType where name = 'DisableUserManagementForOrgType'
			insert into enterprise.ProductConfiguration ( ConfigurationId, ProductSettingId, FromDate, ThruDate )
				values ( @currentproductconfigurationid, SCOPE_IDENTITY(), GETUTCDATE(), null )
		end
	END
GO

 -- Updating Setting URL for Admin Console (Settings)
IF EXISTS (Select Top 1 1 from ENterprise.NavigationMenu where PageId = 'Admin Console' and Origin = 'unified-settings' and [URL] = '/settings')
BEGIN
   UPDATE ENterprise.NavigationMenu SET [URL] = '/settings/admin' where PageId = 'Admin Console' and Origin = 'unified-settings' and [URL] = '/settings';
END