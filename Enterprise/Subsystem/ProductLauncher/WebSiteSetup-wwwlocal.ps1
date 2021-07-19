param 
    (
	[string]$certpassword = "RealPage",
    [string]$certthumbprint = "b3189bc4f641cae6b327d53c19de4c90a50e019f",
	[string]$certfilename = "identitysigningcert20221106local.pfx"
	)

$sspwd = ConvertTo-SecureString -String $certpassword -Force -AsPlainText
try
{
	Import-PfxCertificate -FilePath "$PSScriptRoot\Extra\certs\local\www-local.realpage.com-2022-11-06.pfx" -CertStoreLocation "Cert:\LocalMachine\My" -Password $sspwd
    Import-PfxCertificate -FilePath "$PSScriptRoot\Extra\certs\local\www-local2.realpage.com-2022-11-06.pfx" -CertStoreLocation "Cert:\LocalMachine\My" -Password $sspwd
    Import-PfxCertificate -FilePath "$PSScriptRoot\Extra\certs\local\myactivitylocal.corp.realpage.com-2022-07-23.pfx" -CertStoreLocation "Cert:\LocalMachine\My" -Password $sspwd
}
catch 
{
	Write-Host "Caught an exception:" -ForegroundColor Red
	Write-Host "$($_.Exception)" -ForegroundColor Red
	exit 1;
}

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

function SetUpWebsiteAndAppPool
{
    Param (
        [string] $appname,
        [string] $apppath,
        [AllowEmptyString()][AllowNull()][String] $apppoolname = $null,
        [string] $dnsname
    )

    if (Test-Path IIS:\Sites\$appname)
    {
        StopWebsite $appname
        Remove-Website -Name $appname
    }
    if ($apppoolname -ne ""){
        if (Test-Path IIS:\AppPools\$apppoolname)
        {
            StopAppPool $apppoolname
            Remove-WebAppPool -Name $apppoolname
        }
    }
    Write-Host "mapping to path "$PSScriptRoot\$apppath
    New-Item -ItemType Directory -Force -Path $PSScriptRoot\$apppath
    if ($apppoolname -ne "")
    {
        ## create a new app pool for the website and assign it
        $newapppool = New-WebAppPool -Name $apppoolname 
        New-Website -Name $appname -PhysicalPath $PSScriptRoot\$apppath -ApplicationPool $apppoolname -Ssl -SslFlags 1 -HostHeader $dnsname -Port 443

        $newapppool | Set-ItemProperty -name "startMode" -Value "AlwaysRunning"
        $newapppool | Set-ItemProperty -name "processModel.idleTimeoutAction" -Value "Suspend"
    }
    else
    {
        ## create the website without an app pool
        New-Website -Name $appname -PhysicalPath $PSScriptRoot\$apppath -Ssl -SslFlags 1 -HostHeader $dnsname -Port 443
    }

    Set-ItemProperty "IIS:\Sites\$appname" -Name applicationDefaults.preloadEnabled -Value True

    $cert=(Get-ChildItem cert:\LocalMachine\My | where-object { $_.Subject -match "CN=$dnsname" } | Select-Object -First 1)
    $binding = Get-WebBinding -Name $appname -Protocol "https"
    $binding.AddSslCertificate($cert.Thumbprint, "My")

    If ((Get-Content "$($env:windir)\system32\Drivers\etc\hosts" ) -notcontains "127.0.0.1   $($dnsname)")   
    {
        Add-Content -Encoding UTF8  "$($env:windir)\system32\Drivers\etc\hosts" "127.0.0.1   $($dnsname)" 
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

    if (Test-Path IIS:\Sites\$appname)
    {
        StopWebsite $sitename
        Remove-WebApplication -Name $appname
    }
    if (Test-Path IIS:\AppPools\$apppoolname)
    {
        StopAppPool $apppoolname
        Remove-WebAppPool -Name $apppoolname
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
	
    New-WebApplication -Site $sitename -Name $appname -PhysicalPath $PSScriptRoot\$apppath -ApplicationPool $apppoolname
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


#SetUpWebsiteAndAppPool "IdentityServer" "web\identityserver\identity" "IdentityServer" "myllocal.corp.realpage.com"

SetUpWebsiteAndAppPool "wwwlocal" "web\wwwlocal" "wwwlocal" "www-local.realpage.com"
SetUpWebsiteApplicationAndAppPool "wwwlocal" "home" "wwwhome" "web\landing"
SetUpWebsiteApplicationAndAppPool "wwwlocal" "login" "wwwlogin" "needstobeupdated"

SetUpWebsiteAndAppPool "wwwlocal2" "web\landingmaster" "" "www-local2.realpage.com"

SetUpWebsiteApplicationAndAppPool "wwwlocal2" "api" "wwwlocal2api" "service\landingapi"
SetUpWebsiteApplicationAndAppPool "wwwlocal2" "apienterprise" "wwwlocal2apienterprise" "service\landingapienterprise"
SetUpWebsiteApplicationAndAppPool "wwwlocal2" "apicore" "wwwlocal2apicore" "service\apicore"
SetUpWebsiteApplicationAndAppPool "wwwlocal2" "apicoreenterprise" "wwwlocal2apicoreenterprise" "service\apicoreenterprise"


AddUserToCert -userName ".\IIS_IUSRS" -permission read -certStoreLocation "\LocalMachine\My" -certThumbprint $certthumbprint -certPassword $sspwd
