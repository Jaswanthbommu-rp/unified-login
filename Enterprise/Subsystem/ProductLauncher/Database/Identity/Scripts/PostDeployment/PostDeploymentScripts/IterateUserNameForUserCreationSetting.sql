-- For User Story User Story 1184649: UL: Product Integration - G5/LL Marketing Products
IF NOT EXISTS (SELECT TOP 1 1 FROM Enterprise.ProductSettingType WHERE [Name] = 'IterateUserNameRequiredForUserCreation')
BEGIN
	INSERT INTO Enterprise.ProductSettingType ([Name], [Description], SensitiveData)
	VALUES ('IterateUserNameRequiredForUserCreation', 'it will iterate username when it finds same in product.', 0);
END
-- Enabling 
GO
DECLARE @ProductId INT = 86, @Now DATETIME = GETUTCDATE(), @ProductsettingTypeid int;
IF NOT EXISTS (SELECT TOP 1 (1) FROM ENTERPRISE.ProductSetting PS INNER JOIN enterprise.ProductSettingType pst ON pst.ProductSettingTypeId = PS.ProductSettingTypeId 
    WHERE productid = @ProductId AND pst.Name = 'IterateUserNameRequiredForUserCreation' )
BEGIN
    SELECT @ProductsettingTypeid = ProductSettingTypeId FROM Enterprise.ProductSettingType WHERE [Name] = 'IterateUserNameRequiredForUserCreation'
    exec [Enterprise].[SetProductSetting] 0,@Productid,@ProductsettingTypeid,1
END

GO

IF NOT EXISTS(SELECT TOP 1 1 FROM [Security].[Right]  where [RightName] = 'G5LLMarketingProductsProductaccess' )
BEGIN 

declare @UserId bigint = 0
declare @RightId int = 0

 SELECT @UserId = UserId
       FROM   Ident.UserLogin
       WHERE  LoginName LIKE 'realpagead@%'

insert into [Security].[Right] 
values('G5LLMarketingProductsProductaccess','Manage G5/LL Marketing Products Product access','Manage G5/LL Marketing Products Product access',13,9,3,3,@UserId,getdate(),0)
select @RightId = RightId from [Security].[Right] where [RightName] = 'G5LLMarketingProductsProductaccess'
insert into [Security].[RoleRight] values (1,@RightId,@UserId,getdate())
END

GO