namespace Finance.Repository

open System
open FSharpPlus
open Finance.FSharp
open Npgsql.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

module CloseTransactionsRepository =
    type CompleteCloseTransactionDto
    with
        static member ofRowReader (read : RowReader) =
            let mkTransaction prefix =
                { TransactionDto.TransactionId = read.uuid $"{prefix}_transaction_id"
                  BrokerTransactionId = read.stringOrNone $"{prefix}_broker_transaction_id"
                  TickerId = read.uuid $"{prefix}_ticker_id"
                  Date = read.datetimeOffset $"{prefix}_date"
                  Units = read.decimal $"{prefix}_units"
                  Price = read.decimal $"{prefix}_price"
                  LocalPrice = read.decimalOrNone $"{prefix}_local_price"
                  Fee = read.decimalOrNone $"{prefix}_fee"
                  ExchangeRate = read.decimalOrNone $"{prefix}_exchange_rate"
                  BrokerId = read.uuid $"{prefix}_broker_id"
                  Note = read.stringOrNone $"{prefix}_note" }

            { BuyTransaction = mkTransaction "buy"
              SellTransaction = mkTransaction "sell"
              CompleteCloseTransactionDto.Units = read.decimal "units" }

    let getClosedTransactionByTickerId connectionString (tickerId : TickerId) (startAt : Option<DateOnly>) (endAt : Option<DateOnly>) =
        let initialQuery =
            "SELECT
                ct.units as units,
                buy.transaction_id as buy_transaction_id,
                buy.broker_transaction_id as buy_broker_transaction_id,
                buy.ticker_id as buy_ticker_id,
                buy.date as buy_date,
                buy.units as buy_units,
                buy.price as buy_price,
                buy.local_price as buy_local_price,
                buy.fee as buy_fee,
                buy.exchange_rate as buy_exchange_rate,
                buy.broker_id as buy_broker_id,
                buy.note as buy_note,
                sell.transaction_id as sell_transaction_id,
                sell.broker_transaction_id as sell_broker_transaction_id,
                sell.ticker_id as sell_ticker_id,
                sell.date as sell_date,
                sell.units as sell_units,
                sell.price as sell_price,
                sell.local_price as sell_local_price,
                sell.fee as sell_fee,
                sell.exchange_rate as sell_exchange_rate,
                sell.broker_id as sell_broker_id,
                sell.note as sell_note
             FROM closed_transaction ct
             INNER JOIN transaction buy ON buy.transaction_id = ct.buy_transaction_id
             INNER JOIN transaction sell ON sell.transaction_id = ct.sell_transaction_id
             INNER JOIN ticker t on t.ticker_id = buy.ticker_id
             WHERE t.ticker_id = @tickerId"
            |> Some
        let query =
            let startFilter =
                startAt
                |> Option.map(fun _ -> "and sell.date >= @startAt")
            let endFilter =
                endAt
                |> Option.map(fun _ -> "and sell.date <= @endAt")
            [| initialQuery; startFilter; endFilter |]
            |> Array.choose id
            |> join " "

        connectionString
        |> Sql.connect
        |> Sql.query query
        |> Sql.parameters [ "@tickerId", Sql.uuid (deconstruct tickerId)
                            "@startAt", Sql.timestampOrNone (startAt |> Option.map (toDateTime TimeOnly.MinValue))
                            "@endAt", Sql.timestampOrNone (endAt |> Option.map (toDateTime TimeOnly.MaxValue)) ]
        |> Sql.executeAsync CompleteCloseTransactionDto.ofRowReader
        |> AsyncResult.ofTask
        |> AsyncResult.map (List.map CompleteCloseTransactionDto.toDomain)

    let createCloseTransactions connectionString (closeTransactions : seq<CloseTransaction>) =
        async {
            try
                let data =
                    closeTransactions
                    |> Seq.map CloseTransactionDto.ofDomain
                    |> Seq.map (fun closeTransaction ->
                            [ "@buyTransactionId", Sql.uuid closeTransaction.BuyTransactionId
                              "@sellTransactionId", Sql.uuid closeTransaction.SellTransactionId
                              "@units", Sql.decimal closeTransaction.Units ]
                        )
                    |> List.ofSeq

                let! result =
                    connectionString
                    |> Sql.connect
                    |> Sql.executeTransactionAsync [ "INSERT INTO
                            finance.closed_Transaction (buy_transaction_id, sell_transaction_id, units)
                            VALUES (@buyTransactionId, @sellTransactionId, @units)", data]
                    |> Async.AwaitTask
                return Ok (List.sum result)
            with ex ->
                return Error ex
        }
        |> AsyncResult.mapError handleExceptions
