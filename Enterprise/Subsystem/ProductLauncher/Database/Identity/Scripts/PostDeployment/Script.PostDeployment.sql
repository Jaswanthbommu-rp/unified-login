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

-- remove these if no longer needed
--:r .\PostDeploymentScripts\_SeedData.sql

:r .\PostDeploymentScripts\1503173-Unified-Login-Data-Cleanup.sql

-- keep these at the end!
:r .\PostDeploymentScripts\UpdateStatistics.sql
:r .\PostDeploymentScripts\RecompileAllProcs.sql
:R .\PostDeploymentScripts\UL_Identity_DataCleanup.sql