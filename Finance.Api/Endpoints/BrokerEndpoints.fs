namespace Finance.Api.Endpoints

open System
open System.Threading.Tasks
open FSharp.Core
open Finance.FSharp
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Finance.Api.Helpers
open Finance.Api.Models
open Finance.Application.Broker
open Finance.Application.Degiro
open Finance.Model.Investment

[<RequireQualifiedAccess>]
module Broker =
    let private createBroker (brokerContext : BrokerContext) (brokerDto : BrokerDto) =
        task{
            let ticker = BrokerDto.toDomain brokerDto
            
            let buildUri (broker : BrokerDto) =
                sprintf "/brokers/%O" broker.BrokerId
                
            return! 
                brokerContext.SaveBroker ticker
                |> AsyncResult.map BrokerDto.ofDomain 
                |> IResults.created buildUri
        }
    
    let private getBrokers (brokerContext : BrokerContext) _ =
        task{
            return! 
                brokerContext.FetchBrokers()
                |> AsyncResult.map (List.map BrokerDto.ofDomain)
                |> IResults.ok
        }
        
    let private getBroker (brokerContext : BrokerContext) (id : Guid) =
        task{                
            return!
                ExternalBrokerId id
                |> brokerContext.FetchBrokerByExternalId 
                |> AsyncResult.map BrokerDto.ofDomain
                |> IResults.ok
        }
        
    let private getTransactionByExternalBrokerId (brokerContext : BrokerContext) (externalBrokerId : Guid) =
        task{
            return! 
                brokerContext.FetchTransactionByExternalBrokerId (externalBrokerId |> ExternalBrokerId)
                |> AsyncResult.map (List.map TransactionDto.ofDomain)
                |> IResults.ok
        }
        
    let private uploadFile (degiroContext : DegiroContext) (externalBrokerId : Guid) (request : HttpRequest) : Task<IResult> =
        task{
            if not request.HasFormContentType then
                return Results.BadRequest()
            else
                let form =
                    request.ReadFormAsync()
                    |> AsyncResult.ofTask
                    |> AsyncResult.bind (fun formCollection ->
                        match formCollection.Files.Count with
                        | 1 -> Ok formCollection.Files
                        | _ -> sprintf "No Files Found" |> exn |> Error
                        |> Async.retn)

                let validateForms (forms : IFormFileCollection) =
                    let validateForm (form : IFormFile) =
                        if form.Length = 0 then
                            "Empty file" |> exn |> AsyncResult.error
                        else
                            AsyncResult.retn form

                    forms
                    |> Seq.map validateForm
                    |> AsyncResult.sequence

                let processForms (forms : seq<IFormFile>) =
                    forms
                    |> Seq.map(fun form ->
                        form.OpenReadStream()
                        |> Degiro.importCSV degiroContext externalBrokerId)
                    |> AsyncResult.sequence

                return!
                    form
                    |> AsyncResult.bind validateForms
                    |> AsyncResult.bind processForms
                    |> IResults.ok
        }
    
    let registerEndpoint (app : WebApplication) (brokerContext : BrokerContext) (degiroContext : DegiroContext) =
    
        app.MapPost("/api/brokers", Func<BrokerDto,Task<IResult>>(createBroker brokerContext))
            .WithTags("Brokers") |> ignore
        app.MapGet("/api/brokers", Func<Task<IResult>>(getBrokers brokerContext))
            .WithTags("Brokers") |> ignore
        app.MapGet("/api/brokers/{id}", Func<Guid, Task<IResult>> (fun (id :Guid) -> getBroker brokerContext id))
            .WithTags("Brokers") |> ignore
        app.MapPost("/api/brokers/{id}/transactions", Func<_, _, _> (fun id request -> uploadFile degiroContext id request))
            .Accepts<IFormFile>("multipart/form-data")
            .WithTags("Brokers") |> ignore
        app.MapGet("/api/brokers/{id}/transactions", Func<Guid,Task<IResult>>(fun id -> getTransactionByExternalBrokerId brokerContext id))
            .WithTags("Brokers")