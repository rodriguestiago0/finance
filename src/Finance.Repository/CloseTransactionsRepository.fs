namespace Finance.Repository

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
                { TransactionDto.TransactionId = read.uuid $"{prefix}.transaction_id"
                  BrokerTransactionId = read.stringOrNone $"{prefix}.broker_transaction_id"
                  TickerId = read.uuid $"{prefix}.ticker_id"
                  Date = read.datetimeOffset $"{prefix}.date"
                  Units = read.decimal $"{prefix}.units"
                  Price = read.decimal $"{prefix}.price"
                  LocalPrice = read.decimalOrNone $"{prefix}.local_price"
                  Fee = read.decimalOrNone $"{prefix}.fee"
                  ExchangeRate = read.decimalOrNone $"{prefix}.exchange_rate"
                  BrokerId = read.uuid $"{prefix}.broker_id"
                  Note = read.stringOrNone $"{prefix}.note" }

            { BuyTransaction = mkTransaction "buy"
              SellTransaction = mkTransaction "sell"
              CompleteCloseTransactionDto.Units = read.decimal "ct.units" }

    let getClosedTransactionByTickerId connectionString (id : TickerId) =

        connectionString
        |> Sql.connect
        |> Sql.query "select ct.units, buy.*, sell.* from closed_transaction ct
                      inner join transaction buy on buy.transaction_id = ct.buy_transaction_id
                      inner join transaction sell on sell.transaction_id = ct.sell_transaction_id
                      inner join ticker t on t.ticker_id = buy.ticker_id
                      where t.ticker_id == @tickerId"
        |> Sql.parameters [ "@tickerId", Sql.uuid (deconstruct id) ]
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
