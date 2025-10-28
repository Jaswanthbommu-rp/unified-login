# Unified Login
[![Fortify Scans](https://fortify-badge.realpage.com/badge.php?releaseId=745546)](https://ams.fortify.com/Releases/745546/Overview)
### Repository & framework versions
Repo | Framework
--- | ---
[unified-login-main](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-main) | `.net 4.8`
[unified-login-core](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-core) | `asp.net core 8.0`
[unified-login-coreapi](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-coreapi) | `asp.net core 8.0`
[unified-login-landing](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-landing) | `Angular 16`

**Note** Database projects are part of `uniifed-login-main` Repository

### Tech Stack
 | | | 
--- | ---
UI| Angular
API| .Net Framework 4.8, Core 6.0, Core 8.0
DB| MSSQL server
Auth| Identity Server 7
Logs| Realpage Serilog
Messaging| Kafka queues
Feature Toggle| Launch Darkly
API Performance| APM
Logs|Kibana

### Central Logs and APM
There are built in queries for logs. Click -> Open -> search for "ULE" and select any query to check the logs
[Logs](https://rp-central-log.kb.us-east-2.aws.elastic-cloud.com:9243/s/unity/app/r/s/QDmBt) | [APM](https://rp-central-log.kb.us-east-2.aws.elastic-cloud.com:9243/s/unity/app/apm/services?comparisonEnabled=true&environment=PROD&kuery=&rangeFrom=now-15m&rangeTo=now&serviceGroup=11e2b770-e36b-11ed-a214-15ca9ffaeb43&offset=1d)
--- | ---

## Code setup
Below applications need admin rights (submit salesforce ticket for access)
Applications|Tools|Optional
---|---|---
Command Prompt|Docker/Rancher|PostMan
IIS Manager|VS Code|Postman
Microsoft management console (mmc)|NodeJS|Fiddler
Notepad|Web Platform Installer|Winmerge
Visual Studio 2022|SQL Server Management studio
Visual studio installer|`Asp.Net Core 6.0`|`Asp.Net Core 8.0`
PowerShell|GIT tools

Submit salesforce ticket (TODO: Add sample SR)
- [Kibana Logs](https://rp-central-log.kb.us-east-2.aws.elastic-cloud.com:9243/s/unity/app/discover#/?_g=())
- [DCMS submitter (for all) and approver access if required](http://dcms.realpage.com/) (Sample SR-1247499)
- Add rri to greenbook group to access files on webserver RCDUSOWEB001.Corp.Realpage.com for post build action
- DB Server: RCDUSODBSQL001.Corp.Realpage.com
- Access to TFS Unity collection - https://tfs.realpage.com/tfs/realpage/Unity
## Setting up Code in Local Machine (Use Docker setup)
### Docker version
- Open Powershell in admin mode 
- Run `Set-ExecutionPolicy RemoteSigned`
- Navigate to folder `\unified-login-main\Enterprise\Subsystem\ProductLauncher`
- Run `&'WebSiteSetup-wwwlocal-docker.ps1'`
- All websites will be setup under default website under IIS
### IIS version
- Open Powershell in admin mode 
- Run `Set-ExecutionPolicy RemoteSigned`
- Navigate to folder `\unified-login-main\Enterprise\Subsystem\ProductLauncher`
- Run `&'WebSiteSetup-wwwlocal.ps1'`
- This will create certificates and setup websites in local iis
- Map `wwwlocal2/apicore` and `wwwlocal2/apicoreenterprise` in IIS to `unified-login-coreapi` repo `UnifiedLogin.Api` and `UnifiedLogin.ApiEnterprise` projects
- Map `wwwlocal/login` in iis to `unified-login-core` repo `UnifiedLogin.IdentityServer` project.

### Swagger
For full Url Join any Environment Url + Swagger Urls. For eg: https://www-dev2.realpage.com/api/swagger/ui/index
 | | | 
--- | ---
API | /api/swagger/ui/index
API Enterprise | /apienterprise/swagger/ui/index
API Core | /apicore/swagger/index.html
API Core Enterprise| /apicoreenterprise/swagger/index.html
OpenId | /login/identity/.well-known/openid-configuration
Home | /home

### Environment Urls
Env | API | UI
--- | --- | ---
LOCAL | https://www-local2.realpage.com | https://www-local.realpage.com/home
DEV | https://www-dev2.realpage.com | https://www-dev.realpage.com/home
QA | https://my2qa.realpage.com | https://www-qa.realpage.com/home
SAT | https://my2sat.realpage.com | https://www-sat.realpage.com/home
UAT | https://my2uat.realpage.com | https://www-uat.realpage.com/home
PREPROD | https://my2preprod.realpage.com | https://www-preprod.realpage.com/home
DEMO | https://my2demo.realpage.com | https://www-demo.realpage.com/home
TRAINING | https://my2training.realpage.com | https://www-training.realpage.com/home
EUSAT | https://my2sat.realpage.co.uk | https://www-sat.realpage.co.uk/home
PROD | https://my2.realpage.com | https://www.realpage.com/home
EUPROD | https://my2.realpage.co.uk | https://www.realpage.co.uk/home

# Builds
## Web
Build | Status | Repo
--- | --- | ---
Unity.UL.Master.CI | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status/Unified%20Login/Artifactory/Unity.UL.Master.CI?branchName=master)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=1904&branchName=master) | [unified-login-main](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-main)
Unity.UL.CoreAPI.CI | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status/Unified%20Login/Artifactory/Unity.UL.CoreAPI.CI?branchName=master)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=1949&branchName=master) | [unified-login-coreapi](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-coreapi)
Unity.UL.LandingDevelop.CI | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status/Unified%20Login/Artifactory/Unity.UL.LandingDevelop.CI?branchName=master)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=1950&branchName=master) | [unified-login-landing](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-landing)
Unity.UL.LandingRelease.CI | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status/Unified%20Login/Artifactory/Unity.UL.LandingRelease.CI?branchName=master)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=1951&branchName=master) | [unified-login-landing](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-landing)
Unity.UL.ActivityWeb.CI | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status/Unified%20Login/Artifactory/Unity.UL.ActivityWeb.CI?branchName=master)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=1952&branchName=master) | [unified-login-main](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-main)
unified-login-core-artifacts | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status%2FUnified%20Login%2FArtifactory%2Funified-login-core-artifacts?branchName=main)](https://tfs.realpage.com/tfs/Realpage/Unity/_build?definitionId=6717) | [unified-login-core](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-core)

## Services
Build | Status | Repo
--- | --- | ---
Unity.UL.BatchProcessorService.CI | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status/Unified%20Login/Artifactory/WinService/Unity.UL.BatchProcessorService.CI?branchName=master)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=1962&branchName=master) | [unified-login-main](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-main)
Unity.UL.UserNotificationService.CI | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status/Unified%20Login/Artifactory/WinService/Unity.UL.UserNotificationService.CI?branchName=master)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=1963&branchName=master) | [unified-login-main](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-main)
unified-login-k8s-artifacts | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status/Unified%20Login/unified-login-k8s-artifacts?repoName=unified-login-coreapi&branchName=master)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=2409&repoName=unified-login-coreapi&branchName=master) | [unified-login-coreapi](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-coreapi)

## Database
Build | Status | Repo
--- | --- | ---
Unity.UL.ULDB.CI | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status/Unified%20Login/Artifactory/SQL/Unity.UL.ULDB.CI?branchName=master)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=1960&branchName=master) | [unified-login-main](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-main)
Unity.UL.UPReportingDB.CI | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status/Unified%20Login/Artifactory/SQL/Unity.UL.UPReportingDB.CI?branchName=master)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=4307&branchName=master) | [unified-login-main](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-main)
Unity.UL.ActivityDBV2.CI | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status/Unified%20Login/Artifactory/SQL/Unity.UL.ActivityDBV2.CI?branchName=master)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=2445&branchName=master) | [unified-login-main](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-main)
unified-login-duendedb-artifacts | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status%2FUnified%20Login%2FArtifactory%2Funified-login-duendedb-artifacts?branchName=main)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=2445&branchName=master) | [unified-login-core](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-core?path=/DuendeIdentityServer)

## Sast scans
Build | Status | Repo
--- | --- | ---
unified-login-main-sast | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status/Unified%20Login/Sast-Scans/sast-scan-unified-login-main?branchName=master)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=4546&branchName=master) | [unified-login-main](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-main)
unified-login-core-sast | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status/Unified%20Login/Sast-Scans/unified-login-core-sast?branchName=main)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=4090&branchName=main) | [unified-login-core](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-core)
unified-login-coreapi-sast | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status/Unified%20Login/Sast-Scans/unified-login-coreapi-sast?branchName=master)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=3870&branchName=master) | [unified-login-coreapi](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-coreapi)
unified-login-landing-sast | [![Build Status](https://tfs.realpage.com/tfs/Realpage/Unity/_apis/build/status/Unified%20Login/Sast-Scans/unified-login-landing-sast?branchName=master)](https://tfs.realpage.com/tfs/Realpage/Unity/_build/latest?definitionId=3871&branchName=master) | [unified-login-landing](https://tfs.realpage.com/tfs/Realpage/Unity/_git/unified-login-landing)

# Releases
## Web
Environment | Unity.UL.Master.Web.CD | Unity.UL.Production.Master.Web.CD
--- | --- | --- 
DEV | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/94/412)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=94&_a=releases&view=mine) | [![]()]()
QA | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/94/372)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=94&_a=releases&view=mine) | [![]()]()
SAT | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/94/374)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=94&_a=releases&view=mine) | [![]()]()
UAT | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/94/375)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=94&_a=releases&view=mine) | [![]()]()
PREPROD | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/94/376)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=94&_a=releases&view=mine) | [![]()]()
DEMO | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/94/378)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=94&_a=releases&view=mine) | [![]()]()
TRAINING | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/94/379)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=94&_a=releases&view=mine) | [![]()]()
EUSAT | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/94/465)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=94&_a=releases&view=mine) | [![]()]()
PROD Pool-A | [![]()]() | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/104/426)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=104&view=mine&_a=releases)
PROD Pool-B | [![]()]() | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/104/427)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=104&view=mine&_a=releases)
EUPROD | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/94/501)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=94&_a=releases&view=mine) | [![]()]()

## Database
Environment | Unity.UL.ULDB.CD |Unity.UL.Production.ULDB.CD| Unity.UL.UPReportingDB.CD
---|---|---|---
DEV | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/93/362)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=93&view=mine&_a=releases) | [![]()]() | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/138/627)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=138&view=mine&_a=releases)
QA | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/93/364)]() | [![]()]() | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/138/629)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=138&view=mine&_a=releases)
SAT | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/93/365)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=93&view=mine&_a=releases) | [![]()]() | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/138/630)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=138&view=mine&_a=releases)
UAT | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/93/367)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=93&view=mine&_a=releases) | [![]()]() | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/138/632)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=138&view=mine&_a=releases)
PREPROD | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/93/370)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=93&view=mine&_a=releases) | [![]()]() | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/138/635)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=138&view=mine&_a=releases)
DEMO | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/93/368)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=93&view=mine&_a=releases) | [![]()]() | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/138/633)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=138&view=mine&_a=releases)
TRAINING | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/93/369)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=93&view=mine&_a=releases) | [![]()]() | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/138/634)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=138&view=mine&_a=releases)
EUSAT | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/93/467)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=93&view=mine&_a=releases) | [![]()]() | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/138/636)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=138&view=mine&_a=releases)
PROD |[![]()]()| [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/103/425)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=103&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/138/638)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=138&view=mine&_a=releases)
EUPROD | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/93/500)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=93&view=mine&_a=releases) | [![]()]() | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/138/637)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=138&view=mine&_a=releases)

## Services
Environment | Unity.UL.BatchProcessorService.CD | Unity.UL.UserNotificationService.CD | UL User Sync Service
---|---|---|---
DEV | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/96/389)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=96&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/95/381)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=95&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/118/520)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=118&view=mine&_a=releases)
QA | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/96/390)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=96&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/95/382)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=95&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/118/522)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=118&view=mine&_a=releases)
SAT | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/96/391)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=96&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/95/383)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=95&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/118/523)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=118&view=mine&_a=releases)
UAT | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/96/392)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=96&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/95/384)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=95&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/118/549)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=118&view=mine&_a=releases)
PREPROD | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/96/393)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=96&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/95/387)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=95&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/118/525)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=118&view=mine&_a=releases)
DEMO | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/96/394)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=96&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/95/385)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=95&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/118/527)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=118&view=mine&_a=releases)
TRAINING | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/96/395)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=96&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/95/386)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=95&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/118/552)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=118&view=mine&_a=releases)
EUSAT | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/96/469)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=96&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/95/468)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=95&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/118/550)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=118&view=mine&_a=releases)
PROD | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/96/396)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=96&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/95/388)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=95&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/118/526)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=118&view=mine&_a=releases)
EUPROD | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/96/506)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=96&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/95/507)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=95&view=mine&_a=releases) | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/118/551)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=118&view=mine&_a=releases)

## Activity
Environment | Unity.UL.Activity.Web.DB.CD | Unity.UL.Production.Activity.Web.DB.CD
---|---|---
DEV | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/92/352)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=92&view=mine&_a=releases) | [![]()]()
QA-Web | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/92/355)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=92&view=mine&_a=releases) | [![]()]()
QA-DB | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/92/354)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=92&view=mine&_a=releases) | [![]()]()
SAT | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/92/356)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=92&view=mine&_a=releases) | [![]()]()
UAT | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/92/358)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=92&view=mine&_a=releases) | [![]()]()
PREPROD | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/92/361)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=92&view=mine&_a=releases) | [![]()]()
DEMO | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/92/359)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=92&view=mine&_a=releases) | [![]()]()
TRAINING | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/92/360)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=92&view=mine&_a=releases) | [![]()]()
EUSAT | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/92/466)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=92&view=mine&_a=releases) | [![]()]()
PROD-DB | [![]()]() | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/102/422)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=102&view=mine&_a=releases)
PROD-Web-Even | [![]()]() | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/102/423)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=102&view=mine&_a=releases)
PROD-Web-Odd | [![]()]() | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/102/424)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=102&view=mine&_a=releases)
EUPROD | [![](https://tfs.realpage.com/tfs/Realpage/_apis/public/Release/badge/a582a24e-8967-428a-a623-1e4ee5f5159d/92/502)](https://tfs.realpage.com/tfs/Realpage/Unity/_release?definitionId=92&view=mine&_a=releases) | [![]()]()

## Running Tests
To run unit tests, Open Solution in VS and navigate to Test -> LandingAPI.Test -> Right Click -> Run tests
