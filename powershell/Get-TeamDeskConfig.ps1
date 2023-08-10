Param(
    # SQL Query Parameter
    [string]$UserEmail = "clark.kent@xphone.subfifty.de"

    # SQL Connection Parameter
    , [string]$sqlHost = "(local)\XPCONNECT"
    , [string]$DB = "XPDATA"
#    , [string]$User = "api_user"
#    , [string]$Password = "api_pwd"
)

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

# Connection zur Datenbank aufbauen
if (($User) -and ($Password)) {
    $connectionParam = "Data Source=$sqlHost;User ID=$User;Password=$Password;Initial Catalog=$DB"
} else { 
    $connectionParam = "Data Source=$sqlHost;Integrated Security=SSPI;Initial Catalog=$DB"
 }
$sqlconnection = new-object system.data.SqlClient.SQLConnection($connectionParam)
$sqlconnection.Open()

# OPTIONAL:
# SQL Query definieren, die ggf. mit einer temporären SQL Tabelle arbeitet (#PowershellTempTable)
# Es können auch mehrere SQL Statements hintereinander ausgeführt werden!
#################################################################################################
$query = 
@"
"@

# Non-Query SQL ausühren (falls definiert)
#-----------------------------------------
if ($query -ne "")
{
    $command = new-object system.data.sqlclient.sqlcommand($query,$sqlconnection)
    $command.ExecuteNonQuery() | Out-Null
}

# MANDATORY:
# (Temporäre) Tabelle auslesen und als JSON Objekt zurückgeben
##############################################################
$query = 
@"
    SELECT Name, HasSupervisorRole, CanBeAssignedAgentRole, HasAnalystRole 
    FROM EfCallCenterContainerUsers, EfCallCenterContainer 
    WHERE (EfCallCenterContainerUsers.ContainerId = EfCallCenterContainer.ContainerId) 
    AND UserId = (SELECT dir_Guid 
                  FROM t_person 
                  WHERE col_mail= '$UserEmail' 
                  AND   col_c4bAtlas_user_adminRole='0')
"@
$result = ReadFromTable -SqlQuery $query
ConvertTo-Json -InputObject @( $result | Select-Object * -ExcludeProperty ItemArray, Table, RowError, RowState, HasErrors )

# Nicht vergessen: SQL Connection wieder schließen!
$sqlconnection.Close()
