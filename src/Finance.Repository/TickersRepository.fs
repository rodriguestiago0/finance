namespace Finance.Repository

open FSharpPlus
open Finance.FSharp
open Microsoft.FSharp.Core
open Npgsql.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

module TickersRepository =
    type TickerDto
    with
        static member ofRowReader (read : RowReader) =
            { TickerDto.TickerId = read.uuid "ticker_id"
              ShortId = read.string "short_id"
              TickerType = read.int "ticker_type"
              ISIN = read.string "isin"
              Name = read.string "name"
              Exchange = read.string "exchange"
              Currency = read.int "currency" }
    
    let getAll connectionString : AsyncResult<List<Ticker>, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM finance.ticker"
        |> Sql.executeAsync TickerDto.ofRowReader
        |> AsyncResult.ofTask 
        |> AsyncResult.map (List.map TickerDto.toDomain >> Result.sequence)
        |> Async.map Result.flatten
        |> AsyncResult.map List.ofSeq

    let getByISIN connectionString (isin : ISIN)  : AsyncResult<Ticker, exn> =
        let isin = deconstruct isin
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM finance.ticker WHERE isin = @isin"
        |> Sql.parameters [ ("@isin", Sql.string isin)]
        |> Sql.executeRowAsync TickerDto.ofRowReader
        |> AsyncResult.ofTask 
        |> Async.map (Result.bind TickerDto.toDomain)
        |> AsyncResult.mapError handleExceptions
        
    let getById connectionString (id : TickerId) : AsyncResult<Ticker, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM finance.ticker
                      WHERE ticker_id = @tickerId"
        |> Sql.parameters [ "@tickerId", Sql.uuid (deconstruct id) ]
        |> Sql.executeRowAsync TickerDto.ofRowReader
        |> AsyncResult.ofTask 
        |> Async.map (Result.bind TickerDto.toDomain)
        |> AsyncResult.mapError handleExceptions
    
    let createTicker connectionString (ticker : Ticker) : AsyncResult<Ticker, exn> =
        async {
            try
                let tickerDto =
                    ticker
                    |> TickerDto.ofDomain
                
                return!
                    connectionString
                    |> Sql.connect
                    |> Sql.query "INSERT INTO
                                  finance.ticker (short_id, ticker_type, isin, name, exchange, currency)
                                  VALUES (@shortId, @tickerType, @isin, @name, @exchange, @currency)
                                  RETURNING *"
                    |> Sql.parameters [ ("@shortId", Sql.string tickerDto.ShortId)
                                        ("@tickerType", Sql.int tickerDto.TickerType) 
                                        ("@isin", Sql.string tickerDto.ISIN) 
                                        ("@name", Sql.string tickerDto.Name) 
                                        ("@exchange", Sql.string tickerDto.Exchange)
                                        ("@currency", Sql.int tickerDto.Currency) ]
                    |> Sql.executeRowAsync TickerDto.ofRowReader
                    |> AsyncResult.ofTask
                    |> Async.map (Result.bind TickerDto.toDomain)
            with ex ->
                return Error ex
        }
        |> AsyncResult.mapError handleExceptions