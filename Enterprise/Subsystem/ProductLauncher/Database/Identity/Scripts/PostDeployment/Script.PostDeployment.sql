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
:r .\PostDeploymentScripts\_SeedData.sql

:r .\PostDeploymentScripts\2022.05.WMU.Release.sql
:r .\PostDeploymentScripts\2022-primaryproperty-operator.sql
:r .\PostDeploymentScripts\Batchprocessor-settings.sql
:r .\PostDeploymentScripts\624898-independent-facilities.sql
:r .\PostDeploymentScripts\AccessUDM-viaKongOrDirectly.sql
:r .\PostDeploymentScripts\1213354-add-operators.sql

:r .\PostDeploymentScripts\AllowChangeCompanyDuringLoginSetting.sql
:r .\PostDeploymentScripts\elklogsettings.sql

:r .\PostDeploymentScripts\UpdateStatistics.sql
:r .\PostDeploymentScripts\RecompileAllProcs.sql
:r .\PostDeploymentScripts\ADGroupWithoutUserCreationSetting.sql
:r .\PostDeploymentScripts\ContactCenterMaintenance_Script.sql
:r .\PostDeploymentScripts\IterateUserNameForUserCreationSetting.sql