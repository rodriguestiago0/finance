namespace Finance.Api.Endpoints

open System
open System.Net
open System.Threading.Tasks
open FSharp.Core
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Finance.Api.Helpers
open Finance.Api.Models
open Finance.Application.Broker
open Finance.FSharp
open Finance.Model.Investment

[<RequireQualifiedAccess>]
module Broker =
    let private createBroker (brokerContext : BrokerContext) (brokerDto : BrokerDto) =
        task{
            let ticker = BrokerDto.toDomain brokerDto
            
            let buildUri (broker : BrokerDto) =
                $"/brokers/{broker.BrokerId}"
                
            return!
                ticker
                |> Async.retn
                |> AsyncResult.bind brokerContext.SaveBroker
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
                BrokerId id
                |> brokerContext.FetchBrokerById
                |> AsyncResult.map BrokerDto.ofDomain
                |> IResults.ok
        }

    let registerEndpoint (brokerContext : BrokerContext) (app : WebApplication) =
    
        app.MapPost("/api/brokers", Func<BrokerDto,Task<IResult>>(createBroker brokerContext))
            .WithTags("Brokers") |> ignore
        app.MapGet("/api/brokers", Func<Task<IResult>>(getBrokers brokerContext))
            .WithTags("Brokers") |> ignore
        app.MapGet("/api/brokers/{id}", Func<Guid, Task<IResult>> (fun id -> getBroker brokerContext id))
            .WithTags("Brokers") |> ignore
        app