dotnet build ..\src\All.sln

Remove-Item -Recurse -Force -Path ..\artifacts -ErrorAction SilentlyContinue
New-Item ..\artifacts -ItemType Directory
New-Item ..\artifacts\web -ItemType Directory
New-Item ..\artifacts\win-integration -ItemType Directory

dotnet publish ..\src\Amusoft.PCR.Integration.WindowsDesktop -c Release -o ..\artifacts\win-integration
dotnet publish ..\src\Amusoft.PCR.Server -c Release -o ..\artifacts\web

#Copy-Item ..\src\Amusoft.PCR.Integration.WindowsDesktop\bin\Debug\net5.0-windows\* -Recurse -Destination .\artifacts\win-integration
#Copy-Item ..\src\Amusoft.PCR.Server\bin\Debug\net5.0-windows\win7-x64\* -Recurse -Destination .\artifacts\web

Write-Host "Script complete"
Start-Sleep -Seconds 3