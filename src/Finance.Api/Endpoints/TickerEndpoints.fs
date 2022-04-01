namespace Finance.Api.Endpoints

open System
open System.Threading.Tasks
open FSharp.Core
open Finance.FSharp
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Finance.Api.Helpers
open Finance.Api.Models
open Finance.Application.Ticker
open Finance.Model.Investment

[<RequireQualifiedAccess>]
module Ticker =
    let private createTicker (tickerContext : TickerContext) (tickerDto : TickerDto) =
        task{
            let ticker =
                TickerDto.toDomain tickerDto
                |> Async.retn
            
            let buildUri (ticker : TickerDto) =
                $"/tickers/{ticker.TickerId}"
                
            return! 
                ticker
                |> AsyncResult.bind tickerContext.SaveTicker
                |> AsyncResult.map TickerDto.ofDomain
                |> IResults.created buildUri
        }
        
    let private getTickers (tickerContext : TickerContext) _ =
        task{
            return! 
                tickerContext.FetchTickers()
                |> AsyncResult.map (List.map TickerDto.ofDomain)
                |> IResults.ok
        }
        
    let private getById (tickerContext : TickerContext) (id : Guid) =
        task{
            return!
                TickerId id
                |> tickerContext.FetchTickerById
                |> AsyncResult.map TickerDto.ofDomain
                |> IResults.ok
        }

    let private createDividends (tickerContext : TickerContext) (id : Guid) (dto : DividendDto) =
        task{
            return!
                TickerId id
                |> tickerContext.FetchTickerById
                |> AsyncResult.map TickerDto.ofDomain
                |> IResults.ok
        }
        
    let registerEndpoint (app : WebApplication) (tickerContext : TickerContext) =
        app.MapPost("/api/tickers", Func<TickerDto,Task<IResult>>(createTicker tickerContext))
            .WithTags("Tickers") |> ignore
        app.MapGet("/api/tickers", Func<Task<IResult>>(getTickers tickerContext))
            .WithTags("Tickers") |> ignore
        app.MapGet("/api/tickers/{id}", Func<Guid, Task<IResult>>(fun id -> getById tickerContext id))
            .WithTags("Tickers") |> ignore
        app.MapPost("/api/tickers/{id}/dividends", Func<Guid, DividendDto, Task<IResult>>(fun id dto -> createDividends tickerContext id dto))
            .WithTags("Tickers") |> ignore