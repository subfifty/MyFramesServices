Param(
    # SQL Query Parameter
    [string]$UserAccount = "test@v9.subfifty.de"

    # SQL Connection Parameter
    , [string]$sqlHost = "(local)\XPCONNECT"
    , [string]$DB = "XPDATA"
    #, [string]$User = "api_user"
    #, [string]$Password = "api_pwd"
)

$CharArray = $UserAccount.Split("@")
$AccountName = $CharArray[0]

# Execute SELECT Statement to retrieve data from SQL Server
function ReadFromTable {
    Param(
        [string]$SqlQuery
    )
    $command = new-object system.data.sqlclient.sqlcommand($SqlQuery,$sqlconnection)

    $sqlDataAdapter = New-Object System.Data.sqlclient.sqlDataAdapter $command
    $dataset = New-Object System.Data.DataSet
    $sqlDataAdapter.Fill($dataSet) | Out-Null
    
    $dataSet.Tables
}

# Connection zur Datenbank aufbauen...
if (($User) -and ($Password)) {
    #...mit Credentials (User / Password)
    $connectionParam = "Data Source=$sqlHost;User ID=$User;Password=$Password;Initial Catalog=$DB"
} else { 
    #...mit integrierter Windowsanmeldung
    $connectionParam = "Data Source=$sqlHost;Integrated Security=SSPI;Initial Catalog=$DB"
 }
$sqlconnection = new-object system.data.SqlClient.SQLConnection($connectionParam)
$sqlconnection.Open()

# (Temporäre) Tabelle auslesen und als JSON Objekt zurückgeben
##############################################################
$query = 
@"
    SELECT TOP (10) col_c4bAtlas_user_accountName as AccountName, col_givenName as FirstName, col_sn as LastName, col_mail as Email, col_telephoneNumber as OneNumber
    FROM t_person 
    WHERE col_c4bAtlas_user_isAdmin = 'False' AND col_c4bAtlas_user_internalUser = 'False' AND col_c4bAtlas_user_loginMode > 0 AND col_c4bAtlas_user_accountName = '$AccountName'
"@
$result = ReadFromTable -SqlQuery $query

# Ausgabe als Tabelle:
#$result

# Ausgabe/Verarbeitung in Schleife
#foreach ($User in $result) {
#    Write-Host "Name ="  $User.FirstName $User.LastName
#}

# Ausgabe als JSON:
ConvertTo-Json -InputObject @( $result | Select-Object * -ExcludeProperty ItemArray, Table, RowError, RowState, HasErrors )

# Nicht vergessen: SQL Connection wieder schließen!
$sqlconnection.Close()

