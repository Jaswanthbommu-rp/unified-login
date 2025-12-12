
-- User Story 2336958: Deprecate Products - Migo (DEV)
BEGIN TRY

BEGIN TRAN
declare @produtId int  = 60


delete from Enterprise.[Action]  where productId = @produtId
delete from Enterprise.EmployeeProductMapping  where productId = @produtId
delete from Enterprise.GlobalProductConfiguration  where productId = @produtId
delete from Enterprise.OrganizationProduct  where productId = @produtId
delete from Enterprise.PersonaConfiguration  where productId = @produtId
delete from Enterprise.PersonaProductPropertiesSyncHistory  where productId = @produtId
delete from Enterprise.PersonaProductProperty  where productId = @produtId
delete from Enterprise.PersonaSuggestedProperties  where productId = @produtId
delete from Enterprise.ProductLoginActivitybyUser  where productId = @produtId
delete from Enterprise.ProductProductCenter  where productId = @produtId
delete from Enterprise.ProductRelationship  where productIdTo = @produtId
delete from Enterprise.ProductRight  where productId = @produtId
delete from Enterprise.ProductSetting  where productId = @produtId
delete from Enterprise.ProductUserDependency  where productId = @produtId
delete from Enterprise.ProductUserDependency  where productId = @produtId
delete from Enterprise.ProductValidationRule  where productId = @produtId
delete from Enterprise.PropertyInstanceMapping  where productId = @produtId
delete from Enterprise.PropertyMapping  where productId = @produtId
delete from Enterprise.UserSyncProductPrimaryPropertiesStaging  where productId = @produtId
delete from Ident.SamlAttributeStatement  where productId = @produtId
delete from Ident.SamlProductAttribute  where productId = @produtId
delete from Ident.SamlProductSettings  where productId = @produtId
delete from Ident.SamlUserAttribute  where productId = @produtId
delete from [Security].ADGroupProduct  where productId = @produtId
delete from [Security].ADGroupRole  where productId = @produtId



delete from Enterprise.NavigationMenuRights  where RightId in (select RightId from [Security].[Right]  where productId = @produtId)
delete from [Security].ADGroupRight  where RightId in (select RightId from [Security].[Right] where productId = @produtId)
delete from [Security].ADGroupRight  where RightId in (select RightId from [Security].[Right] where TargetProductId = @produtId)
delete from [Security].OrganizationOverRideRight where RightId in (select RightId from [Security].[Right] where productId = @produtId)
delete from [Security].RightRoute  where RightId in (select RightId from [Security].[Right] where productId = @produtId)
delete from [Security].RoleRight where RightId in (select RightId from [Security].[Right] where productId = @produtId)
delete from [Security].RoleRight where RightId in (select RightId from [Security].[Right] where TargetProductId = @produtId)
delete from [Security].[Right]  where productId = @produtId
delete from [Security].[Right]  where TargetProductId = @produtId

delete from [Security].RoleRight where RoleId in (select RoleId from [Security].[Role]  where productId = @produtId)
delete from [Security].PersonaRole where RoleId in (select RoleId from [Security].[Role]  where productId = @produtId)
delete from [Security].OrganizationOverRideRole where RoleId in (select RoleId from [Security].[Role]  where productId = @produtId)
delete from [Security].[Role] where productId = @produtId


delete from [Security].RoleTemplateAdditionalProductRoleMapping where RoleTemplateProductId in (select RoleTemplateProductId from [Security].RoleTemplateProduct where productId = @produtId)
delete from [Security].RoleTemplateProductRoleMapping where RoleTemplateProductId in (select RoleTemplateProductId from [Security].RoleTemplateProduct where productId = @produtId)
delete from [Security].RoleTemplateProduct  where productId = @produtId
delete from Enterprise.[Product]  where productId = @produtId
COMMIT

END TRY

BEGIN CATCH
ROLLBACK
END CATCH






