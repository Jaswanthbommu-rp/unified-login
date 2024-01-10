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

--:r .\PostDeploymentScripts\1568658-company-setup-data-refresh.sql
--:r .\PostDeploymentScripts\1503173-Unified-Login-Data-Cleanup.sql
--:r .\PostDeploymentScripts\LastLoginDate_DBScript.sql
--:r .\PostDeploymentScripts\1677530_clientportalUltraLightRoleId.sql
--:r .\PostDeploymentScripts\1511799_AddingNewRight_DBScript.sql

:r .\PostDeploymentScripts\1711716_add-privacy-link.sql


-- keep these at the end!
:r .\PostDeploymentScripts\UpdateStatistics.sql
:r .\PostDeploymentScripts\RecompileAllProcs.sql
:R .\PostDeploymentScripts\UL_Identity_DataCleanup.sql