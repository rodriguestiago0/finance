namespace Finance.Repository

open System
open FSharpPlus
open Finance.FSharp
open Microsoft.FSharp.Core
open Npgsql.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

module TickersRepository =
    
    let mapToDto (read : RowReader) =
        { TickerDto.TickerId = read.int "ticker_id"
          ExternalTickerId = read.uuid "external_ticker_id"
          ShortId = read.string "short_id"
          TickerType = read.int "ticker_type"
          ISIN = read.string "isin"
          Name = read.string "name"
          Exchange = read.string "exchange"
          Currency = read.int "currency" }
    
    let getAll connectionString : AsyncResult<List<Ticker>, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM ticker"
        |> Sql.executeAsync mapToDto
        |> AsyncResult.ofTask 
        |> AsyncResult.map (List.map TickerDto.toDomain >> Result.sequence)
        |> Async.map Result.flatten
        |> AsyncResult.map List.ofSeq
    
    let getByISINAndExchange connectionString (isin : ISIN) (exchange : string) : AsyncResult<Ticker, exn> =
        let isin = deconstruct isin
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM ticker WHERE isin = @isin and exchange = @exchange"
        |> Sql.parameters [ ("@isin", Sql.string isin)
                            ("@exchange", Sql.string exchange) ]
        |> Sql.executeRowAsync mapToDto
        |> AsyncResult.ofTask 
        |> Async.map (Result.bind TickerDto.toDomain)
        
    let getByExternalId connectionString (tickerExternalId : ExternalTickerId) : AsyncResult<Ticker, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM ticker WHERE external_ticker_id = @tickerExternalId"
        |> Sql.parameters [ "@tickerExternalId", Sql.uuid (deconstruct tickerExternalId) ]
        |> Sql.executeRowAsync mapToDto
        |> AsyncResult.ofTask 
        |> Async.map (Result.bind TickerDto.toDomain)
    
    let createTicker connectionString (ticker : Ticker) =
        async {
            try
                let tickerDto =
                    ticker
                    |> TickerDto.ofDomain
                
                let! result =
                    connectionString
                    |> Sql.connect
                    |> Sql.query "INSERT INTO
                            ticker (external_ticker_id, short_id, ticker_type, isin, name, exchange, currency)
                            VALUES (@externalTickerId, @shortId, @tickerType, @isin, @name, @exchange, @currency)"
                    |> Sql.parameters [ ("@externalTickerId", Sql.uuid tickerDto.ExternalTickerId)
                                        ("@shortId", Sql.string tickerDto.ShortId) 
                                        ("@tickerType", Sql.int tickerDto.TickerType) 
                                        ("@isin", Sql.string tickerDto.ISIN) 
                                        ("@name", Sql.string tickerDto.Name) 
                                        ("@exchange", Sql.string tickerDto.Exchange)
                                        ("@currency", Sql.int tickerDto.Currency) ]
                    |> Sql.executeNonQueryAsync
                    |> Async.AwaitTask
                return Ok (result)
            with ex ->
                return Error ex
        }