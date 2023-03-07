Declare @UserId bigint;
SELECT @UserId = UserId
FROM   Ident.UserLogin
WHERE  LoginName LIKE 'realpagead@%';


IF NOT EXISTS (Select Top 1 1 from security.role where productId =92)
BEGIN
     Insert into security.role values ('Sustainability Analyst','SustainabilityAnalyst','Sustainability Analyst',3,null,92,@UserId,GETUTCDATE());
END

IF NOT EXISTS (Select Top 1 1 from  Security.[Right] where RightName = 'ManageSustainabilityAnalystProductaccess')
Begin
    Insert into Security.[Right] values ('ManageSustainabilityAnalystProductaccess','Manage Sustainability Analyst Product access','Manage Sustainability Analyst Product access',13,9,3,3,@UserId,GETUTCDATE(),1);
end

Declare @rightId bigint;
Select @rightId = RightId from Security.[Right] where RightName = 'ManageSustainabilityAnalystProductaccess';

IF Not Exists (Select Top 1 1 from Security.RoleRight where RoleId = 1 and RightId =@rightId)
Begin
   Insert into security.RoleRight values (1,@rightId,@UserId,GETUTCDATE())
End

go

-- For User Story 1338013: LeaseLabs Web2Print Social Integration
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'UserGroupsId')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('UserGroupsId', 'it will assigned to web2print user groups for superuser', 0);
END
-- Enabling 
GO
DECLARE @ProductId INT = 87, @Now DATETIME = GETUTCDATE(), @ProductsettingTypeid int;
IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
    WHERE productid = @ProductId AND pst.Name = 'UserGroupsId' )
BEGIN
    SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'UserGroupsId'
    exec [Enterprise].[SetProductSetting] 0,@Productid,@ProductsettingTypeid,1
END
GO