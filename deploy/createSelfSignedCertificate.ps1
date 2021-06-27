Add-Type -AssemblyName System.Windows.Forms

$rootCert = New-SelfSignedCertificate -NotAfter (Get-Date).AddYears(10) -KeyAlgorithm "RSA" -KeyLength 2048 -HashAlgorithm "SHA256" -KeyExportPolicy Exportable -CertStoreLocation Cert:\LocalMachine\My\ -FriendlyName "Amusoft Root Certificate Authority" -Subject 'CN=AmusoftRootCA,O=AmusoftRootCA,OU=AmusoftRootCA'  -KeyUsage CertSign,CRLSign,DigitalSignature -KeyUsageProperty All -Provider 'Microsoft Enhanced RSA and AES Cryptographic Provider'

$siteCert = New-SelfSignedCertificate -NotAfter (Get-Date).AddYears(10) -KeyAlgorithm "RSA" -KeyLength 2048 -HashAlgorithm "SHA256" -KeyExportPolicy Exportable -CertStoreLocation Cert:\LocalMachine\My\ -FriendlyName "Amusoft PC Remote 2 Certificate" -Subject "localhost" -Signer $rootCert -KeyUsage KeyEncipherment,DigitalSignature -KeyUsageProperty All

#Write-Host "Select a password for the site certificate"

$browser = New-Object System.Windows.Forms.SaveFileDialog
$browser.AddExtension = $true
$browser.Filter = "Certificate file (*.pfx)|*.pfx"

if($browser.ShowDialog() -eq "OK"){

    #$certPassword = ConvertTo-SecureString -String “YourPassword” -Force –AsPlainText
    
    #New-Item -Path $browser.FileName
    $siteCertPassword = Read-Host -AsSecureString -Prompt "Select a password for the site certificate"
    $tempCertPath = $browser.FileName
    Export-PfxCertificate -Cert $siteCert -FilePath $tempCertPath -Password $siteCertPassword
     
    #$tempCertPath = [System.IO.Path]::GetTempFileName();
    #$caCertPassword = Read-Host -AsSecureString -Prompt "Select a password for the CA certificate"
    #Export-PfxCertificate -Cert $rootCert -FilePath $tempCertPath -Password $caCertPassword
     
    #Import-PfxCertificate -CertStoreLocation Cert:\LocalMachine\AuthRoot -FilePath $tempCertPath 
}

Write-Host "Script complete"
Start-Sleep -Seconds 3