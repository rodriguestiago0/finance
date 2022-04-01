namespace Finance.Api.Endpoints

open System
open System.Threading.Tasks
open FSharp.Core
open Finance.Api.Helpers
open Finance.Application.BankTransaction
open Finance.FSharp
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module Bank =

    let private getBanks (bankContext : ApiBankTransactionContext) (countryISO : Option<string>) =
        task{
            return!
                bankContext.FetchBanks countryISO
                //|> AsyncResult.map (List.map TransactionDto.ofDomain)
                |> IResults.ok
        }
    
    let registerEndpoint (app : WebApplication) (bankContext : ApiBankTransactionContext) =
        app.MapGet("/api/banks", Func<string, Task<IResult>>(fun countryISO -> getBanks bankContext (countryISO |> Option.ofString)))
            .WithTags("Banks") |> ignore