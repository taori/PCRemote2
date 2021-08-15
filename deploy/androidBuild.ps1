$msbuildPath = &"${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe
#/p:AndroidKeyStore=true /p:AndroidSigningKeyAlias="$env:ANDROID_KEY_ALIAS" /p:AndroidSigningKeyPass="$env:ANDROID_KEY_PASS" /p:AndroidSigningKeyStore="$env:WS\android.keystore" /p:AndroidSigningStorePass="$env:ANDROID_KEYSTORE_PASS"
$scriptRoot = $PSScriptRoot

#$extensionRoot = "$msbuildPath\..\..\..\..\..\Community\MSBuild"

Write-Host $scriptRoot
Write-Host $msbuildPath
#Remove-Item $scriptRoot\..\artifacts\android\ -Recurse -Force

#/p:MSBuildExtensionsPath="$extensionRoot"
#&$msbuildPath "$scriptRoot\..\src\Amusoft.PCR.Mobile.Droid\Amusoft.PCR.Mobile.Droid.csproj" /verbosity:minimal /restore /t:"SignAndroidPackage" /bl /p:Configuration=Release /p:OutputPath="$scriptRoot\..\artifacts\android" 
New-Item -ItemType Directory -Force -Path "$scriptRoot\..\mobile-artifacts\android" | Out-Null

Get-ChildItem "$scriptRoot\..\artifacts\android\" -Filter "*.apk" | Foreach { Move-Item $_.FullName "$scriptRoot\..\mobile-artifacts\android\$($_.Name)" }
Get-ChildItem "$scriptRoot\..\mobile-artifacts\android" | Foreach { Move-Item $_.FullName $_.FullName.Replace("-Signed","") }
#Get-ChildItem "$scriptRoot\..\artifacts\android\" -Filter "*.apk" | Foreach { Move-Item -Path $_.FullName -Destination "$scriptRoot\..\mobile-artifacts\android\$($_.Name)" }
#Get-ChildItem "$scriptRoot\..\artifacts\android\" -Filter "*.apk" | Foreach { Write-Host "$scriptRoot\..\mobile-artifacts\android\$($_.Name)" }
#Get-ChildItem "$scriptRoot\..\artifacts\android\" -Filter "*.apk" | Foreach { Write-Host  $_.FullName -Destination "$scriptRoot\..\mobile-artifacts\android\" }
#Move-Item "$scriptRoot\..\artifacts\android\*.apk" -Destination "$scriptRoot\..\mobile-artifacts\android\"
#Move-Item $scriptRoot\..\mobile-artifacts\android\amusoft.pcr.mobile.droid-Signed.apk  $scriptRoot\..\mobile-artifacts\android\amusoft.pcr.mobile.droid.apk
