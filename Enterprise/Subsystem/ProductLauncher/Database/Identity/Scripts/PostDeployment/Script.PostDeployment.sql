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

:r .\PostDeploymentScripts\2625610_add_updateIfExists_MEDS_setting.sql
:r .\PostDeploymentScripts\2601376_report_settings.sql
:r .\PostDeploymentScripts\2625222-add-setting-UseNewProductUsersEndPoint.sql
:r .\PostDeploymentScripts\2580556-resend-sms-code-config.sql
:r .\PostDeploymentScripts\2479012_Update_Menu_ClientSettings.sql
:r .\PostDeploymentScripts\2563145_AdminSupportal_Standard_IntegrationScript.sql

-- keep these at the end!
:r .\PostDeploymentScripts\UpdateStatistics.sql
:r .\PostDeploymentScripts\RecompileAllProcs.sql
:R .\PostDeploymentScripts\UL_Identity_DataCleanup.sql
