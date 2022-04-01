namespace Finance.Api.Endpoints

open System
open System.Threading.Tasks
open FSharp.Core
open Finance.Api.ApiBinder
open Finance.Api.Helpers
open Finance.Api.Models
open Finance.Application.Degiro
open Finance.Application.Transaction
open Finance.FSharp
open Finance.Model.Investment
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module Transaction =

    let private getTransactionByBrokerId (transactionContext : ApiTransactionContext) (id : Guid) =
        task{
            return!
                BrokerId id
                |> transactionContext.FetchTransactionByBrokerId
                |> AsyncResult.map (List.map TransactionDto.ofDomain)
                |> IResults.ok
        }
        
    let private uploadFile (degiroContext : DegiroContext) (externalBrokerId : Guid) (formFiles : FormFiles) : Task<IResult> =
        task{
                let processForms (forms : seq<IFormFile>) =
                    forms
                    |> Seq.map(fun form ->
                        use stream = form.OpenReadStream()
                        stream
                        |> Degiro.importCSV degiroContext externalBrokerId)
                    |> AsyncResult.sequence

                return!
                    processForms formFiles.Items
                    |> IResults.ok
        }


    
    let registerEndpoint (app : WebApplication) (transactionContext : ApiTransactionContext) (degiroContext : DegiroContext) =

        app.MapPost("/api/brokers/{id}/transactions", Func<Guid,FormFiles,Task<IResult>> (fun id formFiles -> uploadFile degiroContext id formFiles))
            .Accepts<IFormFile>("multipart/form-data")
            .WithTags("Transactions") |> ignore
        app.MapGet("/api/brokers/{id}/transactions", Func<Guid,Task<IResult>>(fun id -> getTransactionByBrokerId transactionContext id))
            .WithTags("Transactions") |> ignore