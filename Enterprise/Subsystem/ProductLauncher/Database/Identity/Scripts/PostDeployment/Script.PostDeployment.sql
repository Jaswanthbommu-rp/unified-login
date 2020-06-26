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


--:r .\PostDeploymentScripts\SameSiteData.sql

--:r .\PostDeploymentScripts\February2020Release.sql
--:r .\PostDeploymentScripts\March2020Release.sql

--:r .\PostDeploymentScripts\April2020Release.sql

--:r .\PostDeploymentScripts\UserManagementData.sql

:r .\PostDeploymentScripts\August2020Release.sql

:r .\PostDeploymentScripts\UpdateStatistics.sql
:r .\PostDeploymentScripts\RecompileAllProcs.sql

----IF  (
----		SELECT	1
----		FROM	(
----			SELECT	[value] ,
----						name
----			FROM	fn_listextendedproperty( DEFAULT , DEFAULT , DEFAULT , DEFAULT , DEFAULT , DEFAULT , DEFAULT )
----			WHERE	name = N'Major Version'
----			UNION ALL
----			SELECT	[value] ,
----						name
----			FROM	fn_listextendedproperty( DEFAULT , DEFAULT , DEFAULT , DEFAULT , DEFAULT , DEFAULT , DEFAULT )
----			WHERE	name = N'Minor Version'
----			UNION ALL
----			SELECT	[value] ,
----						name
----			FROM	fn_listextendedproperty( DEFAULT , DEFAULT , DEFAULT , DEFAULT , DEFAULT , DEFAULT , DEFAULT )
----			WHERE	name = N'Revision'
----			UNION ALL
----			SELECT	[value] ,
----						name
----			FROM	fn_listextendedproperty(DEFAULT , DEFAULT , DEFAULT , DEFAULT , DEFAULT , DEFAULT , DEFAULT )
----			WHERE	name = N'Build'
----		) p
----		PIVOT (
----			MAX(value)
----			FOR name IN ( [Major Version], [Minor Version], [Revision] , [Build])
----		) AS pvt
----		WHERE	pvt.[Major Version] = '0'
----		AND		pvt.[Minor Version] = '0'
----		AND		pvt.Revision = '0'
----		AND		pvt.Build = '64'
----) = 1
----BEGIN
----	:r .\PostDeploymentScripts\Version0.0.0.64.sql
----END
