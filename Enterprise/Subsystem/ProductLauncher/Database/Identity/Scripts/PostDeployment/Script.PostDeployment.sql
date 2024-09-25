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

:r .\PostDeploymentScripts\1711716_add-privacy-link.sql
:r .\PostDeploymentScripts\1663631_Add_2_User_Relationship.sql
:r .\PostDeploymentScripts\1738382PrimaryPropERoles.sql
:r .\PostDeploymentScripts\1726832_delete_product_SAML_data_script.sql
:r .\PostDeploymentScripts\1688264_HOTSCheckUserExcludeProductIds_setting.sql

:r .\PostDeploymentScripts\1791949_AddingAdminRoleType.sql
:r .\PostDeploymentScripts\1608105_ManageSettingRights_Script.sql
:r .\PostDeploymentScripts\1642262_ManageReportsRights_Script.sql
:r .\PostDeploymentScripts\1790971_esupply_productright_script.sql
:r .\PostDeploymentScripts\1775957-User-list-relation-filter.sql
:r .\PostDeploymentScripts\realconnect.sql
:r .\PostDeploymentScripts\1696930_deskdirector_script_changes.sql
:r .\PostDeploymentScripts\1758350_consume_provisioning_kafka_topics_script.sql
:r .\PostDeploymentScripts\1940813_add_customerole_usermgmt_script.sql
:r .\PostDeploymentScripts\1909603_trust_dashboard_script_changes.sql
:r .\PostDeploymentScripts\1971650-add-realconnect-right.sql
:r .\PostDeploymentScripts\1980455_ProductUpdatesDashboard_Script.sql
:r .\PostDeploymentScripts\1826082_Realpage_Emp_UserMgmt_Script.sql
:r .\PostDeploymentScripts\2018027-lumina-settings.sql

-- keep these at the end!
:r .\PostDeploymentScripts\UpdateStatistics.sql
:r .\PostDeploymentScripts\RecompileAllProcs.sql
:R .\PostDeploymentScripts\UL_Identity_DataCleanup.sql