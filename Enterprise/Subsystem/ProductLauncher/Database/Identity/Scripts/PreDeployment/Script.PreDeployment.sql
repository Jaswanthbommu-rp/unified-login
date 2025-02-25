/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed before the build script.	
 Use SQLCMD syntax to include a file in the pre-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the pre-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

/*Role and Right Schema Modifications*/

--:r .\PreDeploymentScripts\DeleteProductId_0_In_PersonaConfiguration.sql



--:r .\PreDeploymentScripts\Activity-PreDeployment.sql
--:r .\PreDeploymentScripts\PopulateMultiCompanySchema.sql



