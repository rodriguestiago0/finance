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
            let newExternalId = ExternalTickerId.newExternalTickerId
            let ticker =
                TickerDto.toDomain tickerDto
                |> Result.map (fun t -> { t with ExternalTickerId = newExternalId})
                |> Async.retn
            
            return! 
                ticker
                |> AsyncResult.bind tickerContext.SaveTicker
                |> AsyncResult.map(fun n -> Results.Ok(n))
                |> AsyncResult.mapError(fun e -> Results.BadRequest(e))
                |> IResults.ofAsyncResult
        }
        
    let private getTickers (tickerContext : TickerContext) _ =
        task{
            
            return! 
                tickerContext.FetchTickers()
                |> AsyncResult.map (List.map TickerDto.ofDomain)
                |> AsyncResult.map(fun n -> Results.Ok(n))
                |> AsyncResult.mapError(fun e -> Results.BadRequest(e))
                |> IResults.ofAsyncResult
        }
        
    let private getByExternalId (tickerContext : TickerContext) (externalId : Guid) =
        task{
            
            return!
                ExternalTickerId externalId
                |> tickerContext.FetchTickerByExternalId
                |> AsyncResult.map TickerDto.ofDomain
                |> AsyncResult.map(fun n -> Results.Ok(n))
                |> AsyncResult.mapError(fun e -> Results.BadRequest(e))
                |> IResults.ofAsyncResult
        }
        
    let registerEndpoint (app : WebApplication) (tickerContext : TickerContext) =
        app.MapPost("/tickers", Func<TickerDto,Task<IResult>>(createTicker tickerContext))
            .WithTags("Tickers") |> ignore
        app.MapGet("/tickers", Func<Task<IResult>>(getTickers tickerContext))
            .WithTags("Tickers") |> ignore
        app.MapGet("/tickers/{id}", Func<Guid, Task<IResult>>(fun id -> getTickers tickerContext id))
            .WithTags("Tickers")