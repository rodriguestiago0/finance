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
open Microsoft.Extensions.Logging

[<RequireQualifiedAccess>]
module Ticker =
    let private createTicker (tickerContext : TickerContext) (tickerDto : TickerDto) =
        task{
            let ticker =
                TickerDto.toDomain tickerDto
                |> Async.retn
            
            let buildUri (ticker : TickerDto) =
                sprintf "/tickers/%O" ticker.TickerId
                
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
        
    let private getByExternalId (tickerContext : TickerContext) (externalId : Guid) =
        task{
            return!
                ExternalTickerId externalId
                |> tickerContext.FetchTickerByExternalId
                |> AsyncResult.map TickerDto.ofDomain
                |> IResults.ok
        }
        
    let registerEndpoint (app : WebApplication) (tickerContext : TickerContext) =
        app.MapPost("/tickers", Func<TickerDto,Task<IResult>>(createTicker tickerContext))
            .WithTags("Tickers") |> ignore
        app.MapGet("/tickers", Func<Task<IResult>>(getTickers tickerContext))
            .WithTags("Tickers") |> ignore
        app.MapGet("/tickers/{id}", Func<Guid, Task<IResult>>(fun id -> getByExternalId tickerContext id))
            .WithTags("Tickers")