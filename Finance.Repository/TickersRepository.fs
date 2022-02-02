namespace Finance.Repository

open FSharpPlus
open Finance.FSharp
open Microsoft.FSharp.Core
open Npgsql.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

module TickersRepository =
    let getByISINAndExchange connectionString (isin : ISIN) (exchange : string) : AsyncResult<Ticker, exn> =
        let isin = deconstruct isin
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM Ticker WHERE isin = @isin and exchange = @exchange"
        |> Sql.parameters [ "isin", Sql.string isin]
        |> Sql.parameters [ "exchange", Sql.string exchange]
        |> Sql.executeAsync (fun read ->
            { TickerDto.TickerId = read.uuid "ticker_id"
              ShortId = read.string "short_id"
              TickerType = read.int "ticker_type"
              ISIN = read.string "isin"
              Name = read.string "name"
              Exchange = read.string "exchange"
              Currency = read.int "currency" })
        |> AsyncResult.ofTask
        |> AsyncResult.map Seq.tryHead
        |> AsyncResult.ofOption (sprintf "Ticker not found %O %O" isin exchange |> exn) 
        |> Async.map (Result.bind TickerDto.toDomain)
