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

:r .\PostDeploymentScripts\UpdateStatistics.sql
:r .\PostDeploymentScripts\RecompileAllProcs.sql
:r .\PostDeploymentScripts\ADGroupWithoutUserCreationSetting.sql
:r .\PostDeploymentScripts\ContactCenterMaintenance_Script.sql
:r .\PostDeploymentScripts\IterateUserNameForUserCreationSetting.sql
:r .\PostDeploymentScripts\AddingSetting_MenuRights.sql
:r .\PostDeploymentScripts\AddingRightforSettings.sql
:r .\PostDeploymentScripts\AddingImpersinateRights.sql
:r .\PostDeploymentScripts\2023.01.WMU.Release.sql


