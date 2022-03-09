namespace Finance.Repository

open FSharpPlus
open Finance.FSharp
open Microsoft.FSharp.Core
open Npgsql.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

module DividendsRepository =
    type DividendDto
    with
        static member ofRowReader (read : RowReader) =
            { DividendDto.DividendId = read.uuid "dividend_id"
              TickerId = read.uuid "ticker_id"
              Value = read.decimal "value"
              Taxes = read.decimalOrNone "taxes"
              ReceivedAt = read.datetimeOffset "received_at" }
    
    let getDividends connectionString : AsyncResult<List<Dividend>, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM dividends"
        |> Sql.executeAsync DividendDto.ofRowReader
        |> AsyncResult.ofTask 
        |> AsyncResult.map (List.map DividendDto.toDomain)

    let getDividendsByTickerId connectionString (tickerId : TickerId) : AsyncResult<List<Dividend>, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM dividends
                      where ticker_id = @tickerId"
        |> Sql.parameters [ "@tickerId", Sql.uuid (deconstruct tickerId) ]
        |> Sql.executeAsync DividendDto.ofRowReader
        |> AsyncResult.ofTask
        |> AsyncResult.map (List.map DividendDto.toDomain)

    let getById connectionString (id : DividendId) : AsyncResult<Dividend, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM dividends
                      where d.dividend_id = @dividendId"
        |> Sql.parameters [ "@dividendId", Sql.uuid (deconstruct id)]
        |> Sql.executeRowAsync DividendDto.ofRowReader
        |> AsyncResult.ofTask
        |> AsyncResult.map DividendDto.toDomain
        |> AsyncResult.mapError handleExceptions
    
    let createDividend connectionString (dividend : Dividend) : AsyncResult<Dividend, exn> =
        async {
            try                    
                let dividendDto =
                    dividend
                    |> DividendDto.ofDomain
                
                return!
                    connectionString
                    |> Sql.connect
                    |> Sql.query "INSERT INTO
                            dividend (ticker_id, value, taxes, received_at)
                            VALUES (@tickerId, @value, @taxes, @receivedAt)
                            RETURNING *"
                    |> Sql.parameters [ ("@tickerId", Sql.uuid dividendDto.TickerId)
                                        ("@value", Sql.decimal dividendDto.Value)
                                        ("@taxes", Sql.decimalOrNone dividendDto.Taxes)
                                        ("@receivedAt", Sql.timestamptz dividendDto.ReceivedAt)]
                    |> Sql.executeRowAsync DividendDto.ofRowReader
                    |> AsyncResult.ofTask
                    |> AsyncResult.map DividendDto.toDomain
            with ex ->
                return Error ex
        }
        |> AsyncResult.mapError handleExceptions