/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

--:r .\Data.Logging.LogCategoryType.sql
--:r .\Data.Logging.LogType.sql
--:r .\Data.Logging.Product.sql
--:r .\Data.Logging.Organization.sql
--:r .\2019-September-Release.sql

--:r .\2020-February-Release.sql

--:r .\2020-August-Release.sql

--:r .\2021Q2Release.sql

:r .\2021Q3Release.sql

:r .\1611312-roles-and-rights-activity-details.sql
