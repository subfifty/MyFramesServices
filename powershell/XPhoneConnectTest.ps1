$cred_path = "C:\temp\xphone_admin_credentials.xml"

Write-Host "Start..."

try {
    Write-Host "Lese Credentials aus: " ("'" + $cred_path + "'")
    $cred = Import-Clixml -Path $cred_path
}
catch {
    Write-Host "Keine Credentials gefunden in: " ("'" + $cred_path + "'")
    Write-Host "Fordere Credentials neu an:"
    $cred = Get-Credential
    $cred | Export-Clixml -Path $cred_path
}

$res = Connect-XpServer $cred
if ( $res -eq $true ) {
    Write-Host "Login Result: TRUE"
} else {
    Write-Host "Login Result: FALSE"
}

# Hier ist der eigentliche Script-Code

Write-Host "Done!"
