param 
    (
	[string]$certpassword = "RealPage",
    [string]$certthumbprint = "64b1365f881c1127c107d1762a31db71cc09feca",
	[string]$certfilename = "dev-identityserver-2022.pfx"
	)

$sspwd = ConvertTo-SecureString -String $certpassword -Force -AsPlainText

Import-Module WebAdministration

function StopAppPool { 
	param ($webAppPoolName)
	
	if((Get-WebAppPoolState $webAppPoolName).Value -ne 'Stopped') {
		Stop-WebAppPool -Name $webAppPoolName
		while((Get-WebAppPoolState $webAppPoolName).Value -ne 'Stopped') {
			Start-Sleep -s 1
		}
		Write-Host "-$webAppPoolName app pool stopped"
	}
}

function StopWebsite { 
	param ($webAppPoolName)
	
	if((Get-WebSiteState $webAppPoolName).Value -ne 'Stopped') {
		Stop-WebSite $webAppPoolName
		while((Get-WebSiteState $webAppPoolName).Value -ne 'Stopped') {
			Start-Sleep -s 1
		}
		Write-Host "-$webAppPoolName website stopped"
	}
}

function SetUpWebsiteApplicationAndAppPool
{
    Param (
        [string] $sitename,
        [string] $appname,
        [string] $apppoolname,
        [string] $apppath
    )

    if (Test-Path IIS:\Sites\$sitename)
    {
        StopWebsite $sitename
    }
    if (Test-Path IIS:\AppPools\$apppoolname)
    {
        StopAppPool $apppoolname
        Remove-WebAppPool -Name $apppoolname
    }
	$apptoremove = $sitename+"\"+$appname
	if (Test-Path IIS:\Sites\$apptoremove)
	{
		Remove-WebApplication -Site $sitename -Name $appname
	}
    
	New-Item -ItemType Directory -Force -Path $PSScriptRoot\$apppath

    $newapppool = New-WebAppPool -Name $apppoolname 
    $newapppool | Set-ItemProperty -name "startMode" -Value "AlwaysRunning"
	
	if ($appname -eq "apicore" -or $appname -eq "apicoreenterprise" -or $appname -eq "login")
	{
		$newapppool | Set-ItemProperty -name "managedRuntimeVersion" -Value ""
	}
	
	if ($appname -ne "login")
	{
		$newapppool | Set-ItemProperty -name "processModel.idleTimeoutAction" -Value "Suspend"
    }
	$TestPath = $PSScriptRoot+"\"+$apppath
	$NewPath = (Resolve-Path $TestPath).Path
	
    New-WebApplication -Site $sitename -Name $appname -PhysicalPath $NewPath -ApplicationPool $apppoolname
}

function AddUserToCert 
{
    param(
        [string]$userName,
        [string]$permission,
        [string]$certStoreLocation,
        [string]$certThumbprint,
        [System.Security.SecureString]$certPassword
    );
    # check if certificate is already installed
    $certificateInstalled = Get-ChildItem cert:$certStoreLocation | Where-Object thumbprint -eq $certThumbprint

    # download & install only if certificate is not already installed on machine
    if ($certificateInstalled -eq $null)
    {
        Import-PfxCertificate -FilePath "$PSScriptRoot\extra\$certfilename" -CertStoreLocation "Cert:\LocalMachine\My" -Password $certPassword
    }

    $certificateInstalled = Get-ChildItem cert:$certStoreLocation | Where-Object thumbprint -eq $certThumbprint
    if ($certificateInstalled -eq $null)
    {
        $message="Certificate with thumbprint:"+$certThumbprint+" does not exist at "+$certStoreLocation
        Write-Host $message -ForegroundColor Red
        exit 1;
    }
    else
    {
        try
        {
            $rule = New-Object security.accesscontrol.filesystemaccessrule $userName, $permission, allow
            $root = "c:\programdata\microsoft\crypto\rsa\machinekeys"
            $l = Get-ChildItem Cert:$certStoreLocation
            $l = $l | Where-Object {$_.thumbprint -like $certThumbprint}
            $l | ForEach-Object{
                $keyname = $_.privatekey.cspkeycontainerinfo.uniquekeycontainername
                $p = [io.path]::combine($root, $keyname)
                $privatekey_file_permissions = Get-Acl -Path $p
                Write-Output "Assigning read access to certificate $certThumbprint"
                if ([io.file]::exists($p))
                {
                    $acl = get-acl -path $p
                    $acl.addaccessrule($rule)
                    Write-Output $p
                    set-acl $p $acl
                }
            }
        }
        catch 
        {
            Write-Host "Caught an exception:" -ForegroundColor Red
            Write-Host "$($_.Exception)" -ForegroundColor Red
            exit 1;
        }    
    }
}



SetUpWebsiteApplicationAndAppPool "default web site" "home" "wwwhome" "web\landing"
SetUpWebsiteApplicationAndAppPool "default web site" "login" "wwwlogin" "..\..\..\..\unified-login-core\UnifiedLogin.IdentityServer"

SetUpWebsiteApplicationAndAppPool "default web site" "api" "wwwlocal2api" "service\landingapi"
SetUpWebsiteApplicationAndAppPool "default web site" "apienterprise" "wwwlocal2apienterprise" "service\landingapienterprise"

Start-Website "default web site"
AddUserToCert -userName ".\IIS_IUSRS" -permission read -certStoreLocation "\LocalMachine\My" -certThumbprint $certthumbprint -certPassword $sspwd
