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

:r .\PostDeploymentScripts\User_Creation.sql
:r .\PostDeploymentScripts\Update_IsRealpageEmployee_To_Existing_Log.sql

--Job
:r .\PostDeploymentScripts\Activity_Archiving_Job_Script.sql
:r .\PostDeploymentScripts\AddNewLogType.sql
:r .\PostDeploymentScripts\2022-logtype-cleanup.sql
:r .\PostDeploymentScripts\Add_UnifiedReporting_Categories.sql
:r .\PostDeploymentScripts\1611312-roles-rights-activity-logs.sql

