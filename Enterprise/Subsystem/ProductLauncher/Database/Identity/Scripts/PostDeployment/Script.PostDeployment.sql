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


:r .\PostDeploymentScripts\ADGroupWithoutUserCreationSetting.sql
:r .\PostDeploymentScripts\ContactCenterMaintenance_Script.sql
:r .\PostDeploymentScripts\IterateUserNameForUserCreationSetting.sql
:r .\PostDeploymentScripts\AddingSetting_MenuRights.sql
:r .\PostDeploymentScripts\AddingRightforSettings.sql
:r .\PostDeploymentScripts\AddingImpersinateRights.sql
:r .\PostDeploymentScripts\2023.01.WMU.Release.sql
:r .\PostDeploymentScripts\KnockProductPanelScript.sql
:r .\PostDeploymentScripts\UserAccessSummarySettings.sql
:r .\PostDeploymentScripts\March2023WMURelease.sql
:r .\PostDeploymentScripts\AddingSettingsForAdminPortal.sql



-- keep these at the end!
:r .\PostDeploymentScripts\UpdateStatistics.sql
:r .\PostDeploymentScripts\RecompileAllProcs.sql


