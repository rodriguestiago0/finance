namespace Finance.Repository

open FSharpPlus
open Finance.FSharp
open Npgsql.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

module TransactionsRepository =
    let createTransactions connectionString (transactions : Transaction[]) =
        async {
            try
                let data =
                    transactions
                    |> Array.map TransactionDto.ofDomain
                    |> Array.map (fun transaction ->
                            [ "@TransactionId", Sql.uuid transaction.TransactionId 
                              "@ExternalTransactionId", Sql.stringOrNone transaction.ExternalTransactionId 
                              "@TickerId", Sql.uuid transaction.TickerId 
                              "@Date", Sql.timestamptz transaction.Date 
                              "@Units", Sql.decimal transaction.Units 
                              "@Price", Sql.decimal transaction.Price 
                              "@LocalPrice", Sql.decimalOrNone transaction.LocalPrice 
                              "@Fee", Sql.decimal transaction.Fee 
                              "@ExchangeRate", Sql.decimalOrNone transaction.ExchangeRate
                              "@Broker", Sql.int transaction.Broker
                              "@Note", Sql.stringOrNone transaction.Note ]
                        )
                    |> List.ofArray
                
                let! result =
                    connectionString
                    |> Sql.connect
                    |> Sql.executeTransactionAsync [ "INSERT INTO
                            Transaction (TransactionId, ExternalTransactionId, TickerId, Date, Units, Price, LocalPrice, Fee, ExchangeRate, Broker, Note)
                            VALUES (@TransactionId, @ExternalTransactionId, @TickerId, @Date, @Units, @Price, @LocalPrice, @Fee, @ExchangeRate, @Broker, @Note)", data]
                    |> Async.AwaitTask
                return Ok (List.sum result)
            with ex ->
                return Error ex
        }
