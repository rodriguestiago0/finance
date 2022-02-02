namespace Finance.Repository

open FSharpPlus
open Finance.FSharp
open Npgsql.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

module TickersRepository =
    let getByISINAndExchange sql (isin : ISIN) (exchange : string) =
        sql
        |> Sql.query "SELECT * FROM Ticker WHERE isin = @isin and exchange = @exchange"
        |> Sql.parameters [ "isin", Sql.string (deconstruct isin)]
        |> Sql.parameters [ "exchange", Sql.string exchange]
        |> Sql.executeAsync (fun read ->
            { TickerDto.TickerId = read.string "ticker_id"
              TickerType = read.int "ticker_type"
              ISIN = read.string "isin"
              Name = read.string "name"
              Exchange = read.string "exchange"
              Currency = read.int "currency" })
        |> AsyncResult.ofTask
        |> Async.map (fun r ->
            r
            |> Result.map (Seq.map TickerDto.toDomain)
            |> Result.bind Result.sequence)
        |> AsyncResult.map Seq.tryHead
