namespace Finance.Api.Endpoints

open System
open System.Threading.Tasks
open FSharp.Core
open Finance.FSharp
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Finance.Api.Helpers
open Finance.Api.Models
open Finance.Application.Dividend
open Finance.Model.Investment

[<RequireQualifiedAccess>]
module Dividends =

    let private createDividends (dividendContext : DividendContext) (id : Guid) (dto : DividendDto) =
        task{
            let buildUri (dividend : DividendDto) =
                $"/api/tickers/{id}/dividends/{dividend.DividendId}"
            return!
                DividendDto.toDomain dto
                |> dividendContext.SaveDividend
                |> AsyncResult.map DividendDto.ofDomain
                |> IResults.created buildUri
        }
        
    let registerEndpoint (app : WebApplication) (dividendContext : DividendContext) =
        app.MapPost("/api/tickers/{id}/dividends", Func<Guid, DividendDto, Task<IResult>>(fun id dto -> createDividends dividendContext id dto))
           .WithTags("Tickers")