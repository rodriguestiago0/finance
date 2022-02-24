namespace Finance.Repository

open FSharpPlus
open Finance.FSharp
open Npgsql.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

module TransactionsRepository =
    let mapToDto (read : RowReader) =
        ({ TransactionDto.TransactionId = read.int "transaction_id"
           ExternalTransactionId = read.uuid "external_transaction_id"
           BrokerTransactionId = read.stringOrNone "broker_transaction_id" 
           TickerId = read.int "ticker_id"
           Date = read.datetimeOffset "date"
           Units = read.decimal "units"
           Price = read.decimal "price"
           LocalPrice = read.decimalOrNone "local_price"
           Fee = read.decimalOrNone "fee"
           ExchangeRate = read.decimalOrNone "exchange_rate"
           BrokerId = read.int "broker_id"
           Note = read.stringOrNone "note" }, read.uuid "external_broker_id", read.uuid "external_ticker_id")

    let getByBrokerExternalId connectionString (externalBrokerId : ExternalBrokerId) : AsyncResult<List<Transaction>, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT t.*, b.external_broker_id, ti.external_ticker_id FROM transaction t
                      INNER JOIN broker b on b.broker_id = t.broker_id
                      INNER JOIN ticker ti on ti.ticker_id = t.ticker_id
                      WHERE b.external_broker_id = @externalBrokerId"
        |> Sql.parameters [ "@externalBrokerId", Sql.uuid (deconstruct externalBrokerId) ]
        |> Sql.executeAsync mapToDto
        |> AsyncResult.ofTask 
        |> AsyncResult.map (List.map TransactionDto.toDomain)
        
    let getByTickerId connectionString (tickerId : TickerId) : AsyncResult<List<Transaction>, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT t.*, b.external_broker_id, ti.external_ticker_id FROM transaction t
                      INNER JOIN broker b on b.broker_id = t.broker_id
                      INNER JOIN ticker ti on ti.ticker_id = t.ticker_id
                      WHERE b.ticker_id = @tickerId"
        |> Sql.parameters [ "@tickerId", Sql.int (deconstruct tickerId) ]
        |> Sql.executeAsync mapToDto
        |> AsyncResult.ofTask 
        |> AsyncResult.map (List.map TransactionDto.toDomain)
        
    let createTransactions connectionString (transactions : seq<Transaction>) =
        async {
            try
                let data =
                    transactions
                    |> Seq.map TransactionDto.ofDomain
                    |> Seq.map (fun transaction ->
                            [ "@ExternalTransactionId", Sql.uuid transaction.ExternalTransactionId
                              "@BrokerTransactionId", Sql.stringOrNone transaction.BrokerTransactionId 
                              "@TickerId", Sql.int transaction.TickerId 
                              "@Date", Sql.timestamptz transaction.Date 
                              "@Units", Sql.decimal transaction.Units 
                              "@Price", Sql.decimal transaction.Price 
                              "@LocalPrice", Sql.decimalOrNone transaction.LocalPrice 
                              "@Fee", Sql.decimalOrNone transaction.Fee
                              "@ExchangeRate", Sql.decimalOrNone transaction.ExchangeRate
                              "@BrokerId", Sql.int transaction.BrokerId
                              "@Note", Sql.stringOrNone transaction.Note ]
                        )
                    |> List.ofSeq
                
                let! result =
                    connectionString
                    |> Sql.connect
                    |> Sql.executeTransactionAsync [ "INSERT INTO
                            Transaction (external_transaction_id, broker_transaction_id, ticker_id, date, units, price, local_price, fee, exchange_rate, broker_id, note)
                            VALUES (@ExternalTransactionId, @BrokerTransactionId, @TickerId, @Date, @Units, @Price, @LocalPrice, @Fee, @ExchangeRate, @BrokerId, @Note)
                            RETURNING *", data]
                    |> Async.AwaitTask
                return Ok (List.sum result)
            with ex ->
                return Error ex
        }
