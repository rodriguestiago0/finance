namespace Finance.Api.Endpoints

open System
open System.Threading.Tasks
open FSharp.Core
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Finance.Api.Helpers
open Finance.Application.BankTransaction
open Finance.FSharp

[<RequireQualifiedAccess>]
module Bank =

    let private getBanks (bankContext : ApiBankTransactionContext) (countryISO : Option<string>) =
        task{
            return!
                bankContext.FetchBanks countryISO
                //|> AsyncResult.map (List.map TransactionDto.ofDomain)
                |> IResults.ok
        }
    
    let registerEndpoint (bankContext : ApiBankTransactionContext) (app : WebApplication) =
        app.MapGet("/api/banks", Func<string, Task<IResult>>(fun countryISO -> getBanks bankContext (countryISO |> Option.ofString)))
            .WithTags("Banks") |> ignore
        app