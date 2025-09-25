/*
Post-Deployment Script Template
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.
 Use SQLCMD syntax to include a file in the post-deployment script.
 Example:		:r .\myfile.sql
 Use SQLCMD syntax to reference a variable in the post-deployment script.	
 Example:		:setvar TableName MyTable
					SELECT * FROM [$(TableName)]
--------------------------------------------------------------------------------------
*/

:r .\PostDeploymentScripts\2156346-redis-cache-settings.sql
:r .\PostDeploymentScripts\2161258_adding_investment_management_family.sql
:r .\PostDeploymentScripts\2219040-lumina-product-codes.sql
:r .\PostDeploymentScripts\2200712-adding-bulletin-board-rights.sql

:r .\PostDeploymentScripts\2252341_ContractMangementSetting.sql
:r .\PostDeploymentScripts\2173457BulkUserUnAssignProducts.sql

:r .\PostDeploymentScripts\2255379-consume-demo-orgtype.sql
--:r .\PostDeploymentScripts\RemoveOldIdentityServerSchema.sql
:r .\PostDeploymentScripts\UserStory2310608RealConnectCache.sql

:r .\PostDeploymentScripts\2375427-product-add-setting-friendlyurlproductname.sql
:r .\PostDeploymentScripts\2310599-add-settings-rights.sql
:r .\PostDeploymentScripts\2251226_adding_IsActivityNotCheckRequired.sql

:r .\PostDeploymentScripts\2481503-lumina-product-codes.sql
:r .\PostDeploymentScripts\FusionCacheSettings_2156341.sql
:r .\PostDeploymentScripts\2251226_adding_IsActivityNotCheckRequired.sql
:r .\PostDeploymentScripts\2422401_Setting_TrustedDeviceExpiryDays.sql
:r .\PostDeploymentScripts\1164582-remove-suggestProperties.sql
:r .\PostDeploymentScripts\2541436-add-setting-PlatformAdminRole.sql

:r .\PostDeploymentScripts\2472100-add-reporting-access-to-sde-right.sql

-- keep these at the end!
:r .\PostDeploymentScripts\UpdateStatistics.sql
:r .\PostDeploymentScripts\RecompileAllProcs.sql
:R .\PostDeploymentScripts\UL_Identity_DataCleanup.sql
