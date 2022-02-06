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
open Finance.Model.Investment

[<RequireQualifiedAccess>]
module Broker =
    let private createBroker (brokerContext : BrokerContext) (brokerDto : BrokerDto) =
        task{
            let newExternalId = ExternalBrokerId.newExternalBrokerId
            let ticker =
                BrokerDto.toDomain brokerDto
                |> AsyncResult.retn
                |> AsyncResult.map (fun t -> { t with ExternalBrokerId = newExternalId})
            
            return! 
                ticker
                |> AsyncResult.bind brokerContext.SaveBroker
                |> AsyncResult.map(fun n -> Results.Ok(n))
                |> AsyncResult.mapError(fun e -> Results.BadRequest(e))
                |> IResults.ofAsyncResult
        }
    
    let private getBrokers (brokerContext : BrokerContext) _ =
        task{
            return! 
                brokerContext.FetchBrokers()
                |> AsyncResult.map (List.map BrokerDto.ofDomain)
                |> AsyncResult.map(fun n -> Results.Ok(n))
                |> AsyncResult.mapError(fun e -> Results.BadRequest(e))
                |> IResults.ofAsyncResult
        }
        
    let private getBroker (brokerContext : BrokerContext) (externalId : Guid) =
        task{
            return!
                ExternalBrokerId externalId
                |> brokerContext.FetchBrokerByExternalId 
                |> AsyncResult.map BrokerDto.ofDomain
                |> AsyncResult.map(fun n -> Results.Ok(n))
                |> AsyncResult.mapError(fun e -> Results.BadRequest(e))
                |> IResults.ofAsyncResult
        }
    
    let registerEndpoint (app : WebApplication) (brokerContext : BrokerContext) =
        app.MapPost("/brokers", Func<BrokerDto,Task<IResult>>(createBroker brokerContext))
            .WithTags("Brokers") |> ignore
        app.MapGet("/brokers", Func<Task<IResult>>(getBrokers brokerContext))
            .WithTags("Brokers") |> ignore
        app.MapGet("/brokers/{id}", Func<Guid, Task<IResult>>(getBrokers brokerContext))
            .WithTags("Brokers")