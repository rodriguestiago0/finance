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
                    |> AsyncResult.map (fun formCollection -> formCollection.Files["File"])
                
                let validateForm (form : IFormFile) =
                    form
                    |> Option.ofObj
                    |> Option.map(fun formFile ->
                        if formFile.Length = 0 then
                            "Empty file" |> exn |> AsyncResult.error
                        else
                            AsyncResult.retn formFile )
                    |> Option.defaultValue ("No file found" |> exn |> AsyncResult.error)
                
                return!
                    form
                    |> AsyncResult.bind validateForm
                    |> AsyncResult.map(fun form -> form.OpenReadStream())
                    |> AsyncResult.map (Degiro.importCSV degiroContext externalBrokerId)
                    |> IResults.ok
        }
    
    let registerEndpoint (app : WebApplication) (brokerContext : BrokerContext) (degiroContext : DegiroContext) =
    
        app.MapPost("/brokers", Func<BrokerDto,Task<IResult>>(createBroker brokerContext))
            .WithTags("Brokers") |> ignore
        app.MapGet("/brokers", Func<Task<IResult>>(getBrokers brokerContext))
            .WithTags("Brokers") |> ignore
        app.MapGet("/brokers/{id}", Func<Guid, Task<IResult>> (fun (id :Guid) -> getBroker brokerContext id))
            .WithTags("Brokers") |> ignore
        app.MapPost("/brokers/{id}/transactions", Func<Guid, HttpRequest,Task<IResult>>(fun id request -> uploadFile degiroContext id request))
            .Accepts<IFormFile>("multipart/form-data")
            .WithTags("Brokers") |> ignore
        app.MapGet("/brokers/{id}/transactions", Func<Guid,Task<IResult>>(fun id -> getTransactionByExternalBrokerId brokerContext id))
            .Accepts<IFormFile>("multipart/form-data")
            .WithTags("Brokers")