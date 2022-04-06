namespace Finance.Api.Endpoints

open System
open System.Threading.Tasks
open FSharp.Core
open Finance.Api.ApiBinder
open Finance.Api.Helpers
open Finance.Api.Models
open Finance.Application.FileImporter
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
        
    let private uploadFile (fileImporterContext : FileImporterContext) (externalBrokerId : Guid) (formFiles : FormFiles) : Task<IResult> =
        task{
                let broker =
                    fileImporterContext.FetchBroker (externalBrokerId |> BrokerId)

                let processForms (forms : seq<IFormFile>) (broker : Broker) =
                    let mkImport fileImporter =
                        match fileImporter with
                        | Degiro ->
                            Degiro.importCSV fileImporterContext broker
                        | Revolut ->
                            Revolut.importCSV fileImporterContext broker

                    let import fileImporter =
                        forms
                        |> Seq.map(fun form ->
                            form.OpenReadStream()
                            |> mkImport fileImporter)
                        |> AsyncResult.sequence

                    broker.FileImporter
                    |> Option.map import
                    |> Option.defaultValue ("No File Importer specified" |> exn |> AsyncResult.error)

                return!
                    broker
                    |> AsyncResult.bind (processForms formFiles.Items)
                    |> IResults.ok
        }


    
    let registerEndpoint (transactionContext : ApiTransactionContext) (degiroContext : FileImporterContext) (app : WebApplication) =

        app.MapPost("/api/brokers/{id}/transactions", Func<Guid,FormFiles,Task<IResult>> (fun id formFiles -> uploadFile degiroContext id formFiles))
            .Accepts<IFormFile>("multipart/form-data")
            .WithTags("Transactions") |> ignore
        app.MapGet("/api/brokers/{id}/transactions", Func<Guid,Task<IResult>>(fun id -> getTransactionByBrokerId transactionContext id))
            .WithTags("Transactions") |> ignore
        app