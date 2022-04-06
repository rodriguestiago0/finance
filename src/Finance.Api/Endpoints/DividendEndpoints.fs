namespace Finance.Api.Endpoints

open System
open System.Threading.Tasks
open FSharp.Core
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Finance.Api.Models
open Finance.Api.Helpers
open Finance.Application.Dividend
open Finance.FSharp
open Finance.Model.Investment

[<RequireQualifiedAccess>]
module Dividend =

    let private createDividends (dividendContext : DividendContext) (tickerId : Guid) (dto : DividendDto) =
        task{
            let buildUri (dividend : DividendDto) =
                $"/api/tickers/{tickerId}/dividends/{dividend.DividendId}"
            return!
                DividendDto.toDomain dto
                |> dividendContext.SaveDividend
                |> AsyncResult.map DividendDto.ofDomain
                |> IResults.created buildUri
        }

    let private getDividendsByTicketId (dividendContext : DividendContext) (tickerId : Guid) =
        task{
            return!
                TickerId tickerId
                |> dividendContext.FetchDividendByTickerId
                |> AsyncResult.map (List.map DividendDto.ofDomain)
                |> IResults.ok
        }

    let private getDividendsById (dividendContext : DividendContext) (dividendId : Guid) =
        task{
            return!
                DividendId dividendId
                |> dividendContext.FetchDividendById
                |> AsyncResult.map DividendDto.ofDomain
                |> IResults.ok
        }
        
    let registerEndpoint (dividendContext : DividendContext) (app : WebApplication) =
        app.MapPost("/api/tickers/{tickerId}/dividends", Func<Guid, DividendDto, Task<IResult>>(fun tickerId dto -> createDividends dividendContext tickerId dto))
            .WithTags("Tickers") |> ignore
        app.MapGet("/api/tickers/{tickerId}/dividends", Func<Guid, Task<IResult>>(fun tickerId -> getDividendsByTicketId dividendContext tickerId))
            .WithTags("Tickers") |> ignore
        app.MapGet("/api/dividends/{dividendId}", Func<Guid, Task<IResult>>(fun dividendId -> getDividendsById dividendContext dividendId ))
            .WithTags("Tickers") |> ignore
        app