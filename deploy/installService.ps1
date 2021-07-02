Add-Type -AssemblyName System.Windows.Forms
function CheckFolderWritePermission {
    param($path)

    Try { 
        [io.file]::OpenWrite("$path\asdlkfkgasdlk.txt").close() 
        return $true; 
    }
    Catch { 
        return $false
    }
}

Write-Host "Script directory: $($PSScriptRoot)"
Write-Host "Script path: $($MyInvocation.MyCommand.Path)"

Write-Host "Restarting script as admin"
if (!([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    Start-Process PowerShell -Verb RunAs "-NoProfile -ExecutionPolicy Bypass -Command `"cd '$pwd'; & '$PSCommandPath';`"";
    exit;
}    

$serviceName = "PCR2"
$programFolder = [System.Environment+SpecialFolder]::ProgramFiles
$translatedFolder = [System.Environment]::GetFolderPath($programFolder)

$browser = New-Object System.Windows.Forms.FolderBrowserDialog
$browser.ShowNewFolderButton = $true
if(Test-Path "$translatedFolder\Amusoft\PCR2"){
    $translatedFolder = "$translatedFolder\Amusoft\PCR2"}

$browser.SelectedPath = "$translatedFolder"
if($browser.ShowDialog() -eq "OK"){
    $folder = $browser.SelectedPath    
    $hasPermission = CheckFolderWritePermission -path $folder

    Write-Host "Has permission: $hasPermission"
    Write-Host "Copying files to $folder from ..\artifacts\"
    Remove-Item "$folder\*" -Recurse -ErrorAction Stop -Force
    Copy-Item ..\artifacts\* -Recurse -ErrorAction Stop -Destination $folder
    Write-Host "Copy done."

    New-Item -ItemType Directory -Path "$folder\logs" -ErrorAction Stop
    
    $allUser = New-Object System.Security.Principal.SecurityIdentifier([System.Security.Principal.WellKnownSidType]::WorldSid, $null)
    $acl = Get-Acl -Path "$folder\logs" 
    $rule = New-Object System.Security.AccessControl.FileSystemAccessRule($allUser,"FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($rule)
    Set-Acl "$folder\logs" $acl

    #powershell -Command "New-LocalUser -Name $serviceName"
    New-Service -Name "$serviceName" -BinaryPathName "$folder\web\Amusoft.PCR.Server.exe" -Description "PC Remote 2 Website" -DisplayName "Amusoft PC Remote 2 Service" -StartupType Automatic
    Start-Service -Name "$serviceName"
}

Write-Host "Script complete"
Start-Sleep -Seconds 3