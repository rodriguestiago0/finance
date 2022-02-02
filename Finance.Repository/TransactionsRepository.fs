namespace Finance.Repository

open FSharpPlus
open Finance.FSharp
open Npgsql
open Npgsql.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

module TransactionsRepository =
    let createTransactions connectionString (transactions : Transaction[]) =
        async {
            
            use connection = new NpgsqlConnection(connectionString)
            do! connection.OpenAsync() |> Async.AwaitTask
            
            use transaction = connection.BeginTransaction()   
            
            let transactionDto =
                transactions
                |> Seq.map TransactionDto.ofDomain
                
            for transaction in transactionDto do
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
                |> Sql.executeNonQueryAsync
                |> AsyncResult.ofTask |> ignore
                    
            do! transaction.CommitAsync() |> Async.AwaitTask
            return Ok()
        }
