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



AddUserToCert -userName ".\IIS_IUSRS" -permission read -certStoreLocation "\LocalMachine\My" -certThumbprint $certthumbprint -certPassword $sspwd
