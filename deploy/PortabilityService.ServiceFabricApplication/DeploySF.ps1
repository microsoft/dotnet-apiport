<#
.PARAMETER ClusterEndpoint
The Service Fabric endpoint to deploy to.

.PARAMETER CertLocation
The location of the .pem file to use for communicating with the secure 
Service Fabric cluster. This can be downloaded from Key Vault by 
selecting the certificate, choosing 'download in PFX/PEM format' and
converting the file from pfx to pem with OpenSSL:
openssl pkcs12 -in filename.pfx -out filename.pem -nodes

.PARAMETER ContainerRegistryUserName
The user name used to pull images from the container registry 
containing the application's Docker images

.PARAMETER ContainerRegistryPassword
The password used to pull images from the container registry 
containing the application's Docker images

.PARAMETER ApplicationId
Identifier for the Service Fabric application to deploy

.PARAMETER Upgrade
Upgrade an existing application to a newer application type. Otherwise, delete and re-create the application.

.PARAMETER EnvironmentName
Specify the ASP.NET Core environment to run (Development, Staging, Production); defaults to Production

.PARAMETER ImageTag
Specify Docker image tag to use when pulling containers

.EXAMPLE
DeploySF.ps1 -ClusterEndpoint https://portabilityservice.eastus.cloudapp.azure.com:19080 -CertLocation C:\certs\SFCert.pem

DeploySF.ps1 -ClusterEndpoint https://portabilityservice.eastus.cloudapp.azure.com:19080 -CertLocation C:\certs\SFCert.pem -ContainerRegistryUserName portabilityservice -EnvironmentName Development -ImageTag 0.3
#>

# Parameters
Param
(
    [Parameter(Mandatory=$true)]
    [string]
    $ClusterEndpoint,

    [String]
    $CertLocation,

    [String]
    $ContainerRegistryUserName,

    [Parameter(Mandatory=$true)]
    [SecureString]
    $ContainerRegistryPassword,

    [String]
    $ApplicationId = 'PortabilityService',

    [Switch]
    $Upgrade,

    [String]
    $EnvironmentName,

    [String]
    $ImageTag
)

function Check-Success
{
    Param (
        [String]
        $TaskName
    )
        
    if($LASTEXITCODE -eq 0)
    {
        Write-Host "Task '$TaskName' succeeded" -ForegroundColor Green
    }
    else 
    {
        Write-Host "TASK '$TaskName' FAILED" -ForegroundColor Red
        exit
    }
}

# Get application type name and version from ApplicationManifest.xml
$applicationManifest = [Xml] (Get-Content $PSScriptRoot\ApplicationManifest.xml)
$applicationName = $applicationManifest.ApplicationManifest.Attributes.GetNamedItem("ApplicationTypeName").Value
$applicationVersion = $applicationManifest.ApplicationManifest.Attributes.GetNamedItem("ApplicationTypeVersion").Value
Write-Host "Deploying $applicationName, $applicationVersion" -ForegroundColor Green

# Connect to the SF cluster
Write-Host "Connecting to Service Fabric cluster at $ClusterEndpoint" -ForegroundColor Cyan
if ($CertLocation)
{
    sfctl cluster select --endpoint $ClusterEndpoint --pem $CertLocation --no-verify --verbose
}
else 
{
    sfctl cluster select --endpoint $ClusterEndpoint --verbose
}
Check-Success "Connect to Service Fabric cluster"

# Upload the application and service manifests
#   Note that this doesn't work, as written for localhost-hosted clusters
#   https://github.com/Azure/service-fabric-cli/issues/51
Write-Host "Uploading app manifests" -ForegroundColor Cyan
sfctl application upload --path $PSScriptRoot --verbose
Check-Success "Upload manifests"


if (-not $Upgrade)
{
    Write-Host "Checking for and removing existing deployments" -ForegroundColor Cyan

    # Delete any existing deployments
    sfctl application delete --application-id $ApplicationId > $null 2>&1

    # Delete any existing application type
    sfctl application unprovision --application-type-name $applicationName --application-type-version $applicationVersion > $null 2>&1
}

# Provision the application type
Write-Host "Provisioning Service Fabric application type" -ForegroundColor Cyan
sfctl application provision --application-type-build-path PortabilityService.ServiceFabricApplication --timeout 180 --verbose
Check-Success "Provision application type"

# Initialize parameters object
$parameters = @{}
$registryPasswordBtr = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($ContainerRegistryPassword)
$parameters.RepositoryPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($registryPasswordBtr) 
if ($ContainerRegistryUserName)
{
    $parameters.RepositoryUserName = $ContainerRegistryUserName
}
if ($EnvironmentName)
{
    $parameters.EnvironmentName = $EnvironmentName
}
if ($ImageTag)
{
    $parameters.ImageTag = $ImageTag
}
$parametersString = ($parameters | ConvertTo-Json -Compress).replace('"', '\"')

if ($Upgrade)
{
    # Upgrade application
    Write-Host "Upgrading application"
    sfhost application upgrade --application-name fabric:/$ApplicationId --application-version $applicationVersion --parameters $parametersString --verbose
    Check-Success "Upgrade application"
}
else 
{
    # Upgrade application
    Write-Host "Creating application"
    sfctl application create --app-name fabric:/$ApplicationId --app-type $applicationName --app-version $applicationVersion --parameters $parametersString --verbose
    Check-Success "Create application"
}