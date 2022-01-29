namespace Finance.Repository

open Npgsql.FSharp

module PostgresClient = 
    let private connectionString : string =
        Sql.host "localhost"
        |> Sql.database "finance"
        |> Sql.username "finance"
        |> Sql.password "finance"
        |> Sql.port 5432
        |> Sql.formatConnectionString
       
    let sql =
        connectionString
        |> Sql.connect
    