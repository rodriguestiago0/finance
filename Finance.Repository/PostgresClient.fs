namespace Finance.Repository

open Npgsql.FSharp

module PostgresClient =        
    let sql connectionString =
        connectionString
        |> Sql.connect
    