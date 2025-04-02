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

:r .\PostDeploymentScripts\2161258_adding_investment_management_family.sql
:r .\PostDeploymentScripts\2219040-lumina-product-codes.sql
:r .\PostDeploymentScripts\2200712-adding-bulletin-board-rights.sql

-- keep these at the end!
:r .\PostDeploymentScripts\UpdateStatistics.sql
:r .\PostDeploymentScripts\RecompileAllProcs.sql
:R .\PostDeploymentScripts\UL_Identity_DataCleanup.sql
