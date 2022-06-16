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

:r .\PostDeploymentScripts\June2022Release.sql
:r .\PostDeploymentScripts\2022.05.WMU.Release.sql

:r .\PostDeploymentScripts\UpdateStatistics.sql
:r .\PostDeploymentScripts\RecompileAllProcs.sql

