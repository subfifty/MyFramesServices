Param(
    # SQL Query Parameter
    [string]$UserEmail = "Test@sap.com"

    # SQL Connection Parameter
    , [string]$sqlHost = "(local)\XPCONNECT"
    , [string]$DB = "XPDATA"
    , [string]$User = "sa"
    , [string]$Password = "52145C85E4164eba8ABF870278BB3478"
   
)

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

# SQL Query definieren, die mit einer temporären SQL Tabelle arbeitet
$query = 
@"
    SELECT Name, QueueId, ContainerId, Extensions AS ServiceNumber
    INTO #PowershellTempTable 
    FROM EfCallCenterQueue 
    WHERE QueueId in (
        SELECT QueueId FROM EfCallCenterQueueAgents 
        WHERE UserId = (SELECT dir_Guid FROM t_person WHERE col_mail= '$UserEmail' AND col_c4bAtlas_user_adminRole='0')
    )

    ALTER TABLE #PowershellTempTable ADD Number ntext

    UPDATE #PowershellTempTable Set Number = (SELECT dir_Value FROM tableArray WHERE CHARINDEX(CONVERT(nvarchar(256),QueueId), dir_Value) > 0)

    UPDATE #PowershellTempTable Set Number = SUBSTRING(
                                    Number, 
                                    CHARINDEX( CONCAT( CONVERT(nvarchar(256),QueueId), '|', CONVERT(nvarchar(256),ContainerId) ) , Number) 
                                        + LEN( CONCAT( CONVERT(nvarchar(256),QueueId), '|', CONVERT(nvarchar(256),ContainerId) ) )
                                        + 1, 
                                    100
                                 )

    UPDATE #PowershellTempTable Set Number = SUBSTRING(Number, 1, CHARINDEX( '|', Number ) - 1 )

    UPDATE #PowershellTempTable SET ServiceNumber = '' 
	    WHERE CHARINDEX('ServiceNumber=', ServiceNumber) = 0
    UPDATE #PowershellTempTable SET ServiceNumber = SUBSTRING( ServiceNumber, CHARINDEX('ServiceNumber=', ServiceNumber) + LEN('ServiceNumber='), 50)
	    WHERE CHARINDEX('ServiceNumber=', ServiceNumber) > 0
    UPDATE #PowershellTempTable SET ServiceNumber = SUBSTRING( ServiceNumber, 1, CHARINDEX(';', ServiceNumber) - 1)
        WHERE LEN(ServiceNumber) > 0    
"@

# Non-Query SQL ausühren
$command = new-object system.data.sqlclient.sqlcommand($query,$sqlconnection)
$command.ExecuteNonQuery() | Out-Null

# (Temporäre) Tabelle auslesen und als JSON Objekt zurückgeben
$query = 
@"
    SELECT Name, Number, ServiceNumber FROM #PowershellTempTable
"@
$result = ReadFromTable -SqlQuery $query
ConvertTo-Json -InputObject @( $result | Select-Object * -ExcludeProperty ItemArray, Table, RowError, RowState, HasErrors )

# Nicht vergessen: SQL Connection wieder schließen!
$sqlconnection.Close()
