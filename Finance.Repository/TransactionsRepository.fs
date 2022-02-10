namespace Finance.Repository

open FSharpPlus
open Finance.FSharp
open Npgsql.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

module TransactionsRepository =
    let createTransactions connectionString (transactions : seq<Transaction>) =
        async {
            try
                let data =
                    transactions
                    |> Seq.map TransactionDto.ofDomain
                    |> Seq.map (fun transaction ->
                            [ "@TransactionId", Sql.int transaction.TransactionId 
                              "@ExternalTransactionId", Sql.uuid transaction.ExternalTransactionId 
                              "@BrokerTransactionId", Sql.stringOrNone transaction.BrokerTransactionId 
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
                    |> List.ofSeq
                
                let! result =
                    connectionString
                    |> Sql.connect
                    |> Sql.executeTransactionAsync [ "INSERT INTO
                            Transaction (transaction_id, external_transaction_id, broker_transaction_id, ticker_id, date, units, price, local_price, fee, exchange_rate, broker, note)
                            VALUES (@TransactionId, @ExternalTransactionId, @BrokerTransactionId, @TickerId, @Date, @Units, @Price, @LocalPrice, @Fee, @ExchangeRate, @Broker, @Note) 
                            RETURNING *", data]
                    |> Async.AwaitTask
                return Ok (List.sum result)
            with ex ->
                return Error ex
        }
