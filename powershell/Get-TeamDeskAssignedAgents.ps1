Param(
    # SQL Query Parameter
    [string]$TeamDeskName = "Test PJ",

    # SQL Connection Parameter
    [string]$sqlHost = "(local)\XPCONNECT"
    , [string]$DB = "XPDATA"
    #, [string]$User = "api_user"
    #, [string]$Password = "api_pwd"
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

# Optional: 
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

# Mandatory:
# (Temporäre) Tabelle auslesen und als JSON Objekt zurückgeben
##############################################################
$query = 
@"
	SELECT col_givenName as FirstName, col_sn as LastName, col_mail as Email, case when q.UserId IS NOT NULL THEN 1 ELSE 0 END as IsAssigned
    FROM t_person p
    JOIN ( SELECT UserId
                        FROM EfCallCenterContainerUsers 
                        WHERE ContainerId = ( SELECT ContainerId 
                                          FROM EfCallCenterContainer 
                                          WHERE Name='$TeamDeskName' ) 
	) as c
	ON p.dir_Guid = c.UserId
	LEFT OUTER JOIN ( SELECT UserId
                        FROM EfCallCenterQueueAgents 
                        WHERE QueueId = ( SELECT QueueId 
                                          FROM EfCallCenterQueue 
                                          WHERE Name='$TeamDeskName' ) 
	) as q 
	ON p.dir_Guid = q.UserId
	ORDER BY IsAssigned DESC
"@
$result = ReadFromTable -SqlQuery $query
ConvertTo-Json -InputObject @( $result | Select-Object * -ExcludeProperty ItemArray, Table, RowError, RowState, HasErrors )

# Nicht vergessen: SQL Connection wieder schließen!
$sqlconnection.Close()
