namespace Finance.Repository

open FSharpPlus
open Finance.FSharp
open Npgsql.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

module TransactionsRepository =
    type TransactionDto
    with
        static member ofRowReader (read : RowReader) =
            { TransactionDto.TransactionId = read.uuid "transaction_id"
              BrokerTransactionId = read.stringOrNone "broker_transaction_id"
              TickerId = read.uuid "ticker_id"
              Date = read.datetimeOffset "date"
              Units = read.decimal "units"
              Price = read.decimal "price"
              LocalPrice = read.decimalOrNone "local_price"
              Fee = read.decimalOrNone "fee"
              ExchangeRate = read.decimalOrNone "exchange_rate"
              BrokerId = read.uuid "broker_id"
              Note = read.stringOrNone "note" }

    let getByBrokerExternalId connectionString (brokerId : BrokerId) : AsyncResult<List<Transaction>, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM transaction
                      WHERE broker_id = @brokerId"
        |> Sql.parameters [ "@brokerId", Sql.uuid (deconstruct brokerId) ]
        |> Sql.executeAsync TransactionDto.ofRowReader
        |> AsyncResult.ofTask 
        |> AsyncResult.map (List.map TransactionDto.toDomain)
        |> AsyncResult.mapError handleExceptions
        
    let getByTickerId connectionString (tickerId : TickerId) : AsyncResult<List<Transaction>, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM transaction
                      WHERE ticker_id = @tickerId"
        |> Sql.parameters [ "@tickerId", Sql.uuid (deconstruct tickerId) ]
        |> Sql.executeAsync TransactionDto.ofRowReader
        |> AsyncResult.ofTask 
        |> AsyncResult.map (List.map TransactionDto.toDomain)
        |> AsyncResult.mapError handleExceptions

    let getOpenTransactionByTickerId connectionString (tickerId : TickerId) : AsyncResult<List<Transaction>, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "with w as
                                (SELECT
                                        t.*,
                                        sum(ct.Units) as total
                                FROM transaction t
                                FULL JOIN closed_transaction ct on ct.buy_transaction_id = t.transaction_id or ct.sell_transaction_id = t.transaction_id
                                where ti.ticker_id = @tickerId
                                group by t.transaction_id)
                            select
                                   w.transaction_id,
                                   w.external_transaction_id,
                                   w.broker_transaction_id,
                                   w.ticker_id,
                                   w.date,
                                   w.price,
                                   w.local_price,
                                   w.fee,
                                   w.exchange_rate,
                                   w.broker_id,
                                   w.note,
                                   w.external_broker_id,
                                   w.external_ticker_id,
                                   case
                                       when w.total is null then  w.units
                                       when w.units < 0 then  w.units + w.total
                                       else w.units - w.total
                                   end as units
                            from w
                            where w.total <> abs(w.units) or w.total is null"
        |> Sql.parameters [ "@tickerId", Sql.uuid (deconstruct tickerId) ]
        |> Sql.executeAsync TransactionDto.ofRowReader
        |> AsyncResult.ofTask
        |> AsyncResult.map (List.map TransactionDto.toDomain)
        |> AsyncResult.mapError handleExceptions
        
    let createTransactions connectionString (transactions : seq<Transaction>) =
        async {
            try
                let data =
                    transactions
                    |> Seq.map TransactionDto.ofDomain
                    |> Seq.map (fun transaction ->
                            [ "@brokerTransactionId", Sql.stringOrNone transaction.BrokerTransactionId
                              "@tickerId", Sql.uuid transaction.TickerId
                              "@date", Sql.timestamptz transaction.Date
                              "@units", Sql.decimal transaction.Units
                              "@price", Sql.decimal transaction.Price
                              "@localPrice", Sql.decimalOrNone transaction.LocalPrice
                              "@fee", Sql.decimalOrNone transaction.Fee
                              "@exchangeRate", Sql.decimalOrNone transaction.ExchangeRate
                              "@brokerId", Sql.uuid transaction.BrokerId
                              "@note", Sql.stringOrNone transaction.Note ]
                        )
                    |> List.ofSeq
                
                let! result =
                    connectionString
                    |> Sql.connect
                    |> Sql.executeTransactionAsync [ "INSERT INTO
                            Transaction (broker_transaction_id, ticker_id, date, units, price, local_price, fee, exchange_rate, broker_id, note)
                            VALUES (@brokerTransactionId, @tickerId, @date, @units, @price, @localPrice, @fee, @exchangeRate, @brokerId, @note)", data]
                    |> Async.AwaitTask
                return Ok (List.sum result)
            with ex ->
                return Error ex
        }
        |> AsyncResult.mapError handleExceptions
