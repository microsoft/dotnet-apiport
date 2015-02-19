$kpmPath = Join-Path "$env:USERPROFILE" ".kpm"
$krePath = Join-Path "$env:USERPROFILE" ".kre"

$isKPMExist = Test-Path $kpmPath -PathType Container
$isKREExist = Test-Path $krePath -PathType Container

if ($isKPMExist -and $isKREExist)
{
    Write-Host "KPM [$kpmPath] and KRE [$krePath] exists."
    return;
}

& powershell -NoProfile -ExecutionPolicy Unrestricted -Command "Invoke-Expression ((New-Object Net.WebClient).DownloadString('https://raw.githubusercontent.com/aspnet/Home/master/kvminstall.ps1'))"

$KREInstallPath = "$($env:USERPROFILE)\.kre\bin"
& powershell -NoProfile -ExecutionPolicy Unrestricted -Command "& $KREInstallPath\kvm.cmd upgrade"