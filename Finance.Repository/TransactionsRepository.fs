namespace Finance.Repository

open Finance.FSharp
open Npgsql
open Npgsql.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

module TransactionsRepository =
    let createTransactions connectionString (transactions : Transaction[]) =
        use connection = new NpgsqlConnection(connectionString)
        connection.Open()
        
        use transaction = connection.BeginTransaction()

        let insert (transaction : TransactionDto) =
            connection
            |> Sql.existingConnection
            |> Sql.query "INSERT INTO
                    Transaction (TransactionId, ExternalTransactionId, TickerId, Date, Units, Price, LocalPrice, Fee, ExchangeRate)
                    VALUES (@TransactionId, @ExternalTransactionId, @TickerId, @Date, @Units, @Price, @LocalPrice, @Fee, @ExchangeRate)"
            |> Sql.parameters [ "@TransactionId", Sql.uuid transaction.TransactionId ]
            |> Sql.parameters [ "@ExternalTransactionId", Sql.stringOrNone transaction.ExternalTransactionId ]
            |> Sql.parameters [ "@TickerId", Sql.uuid transaction.TickerId ]
            |> Sql.parameters [ "@Date", Sql.timestamptz transaction.Date ]
            |> Sql.parameters [ "@Units", Sql.decimal transaction.Units ]
            |> Sql.parameters [ "@Price", Sql.decimal transaction.Price ]
            |> Sql.parameters [ "@LocalPrice", Sql.decimalOrNone transaction.LocalPrice ]
            |> Sql.parameters [ "@Fee", Sql.decimal transaction.Fee ]
            |> Sql.parameters [ "@ExchangeRate", Sql.decimalOrNone transaction.ExchangeRate ]
            |> Sql.executeNonQuery
            |> ignore
        
        transactions
        |> Seq.map TransactionDto.ofDomain
        |> Seq.iter insert
        
        transaction.Commit()
        AsyncResult.retn ()
