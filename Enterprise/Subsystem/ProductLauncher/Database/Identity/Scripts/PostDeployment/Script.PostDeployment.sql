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

--:r .\PostDeploymentScripts\UserAccessSummarySettings.sql


:r .\PostDeploymentScripts\AddingNewRightforUserManagement.sql
:r .\PostDeploymentScripts\AddingNewSettingForPropagateRights.sql
:r .\PostDeploymentScripts\SustainabilityAdmin.sql


-- keep these at the end!
:r .\PostDeploymentScripts\UpdateStatistics.sql
:r .\PostDeploymentScripts\RecompileAllProcs.sql
