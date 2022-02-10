namespace Finance.Repository

open System
open FSharpPlus
open Finance.FSharp
open Microsoft.FSharp.Core
open Npgsql.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

module BrokersRepository =
    
    let mapToDto (read : RowReader) =
        { BrokerDto.BrokerId = read.int "broker_id"
          ExternalBrokerId = read.uuid "external_broker_id"
          Name = read.string "name" }
    
    let getBrokers connectionString : AsyncResult<List<Broker>, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM broker"
        |> Sql.executeAsync mapToDto
        |> AsyncResult.ofTask 
        |> AsyncResult.map (List.map BrokerDto.toDomain)
        
    let getByName connectionString (name : string) : AsyncResult<Broker, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM broker WHERE name = @Name"
        |> Sql.parameters [ "@Name", Sql.string name]
        |> Sql.executeRowAsync mapToDto
        |> AsyncResult.ofTask 
        |> AsyncResult.map BrokerDto.toDomain
        
    let getByExternalId connectionString (externalId : ExternalBrokerId) : AsyncResult<Broker, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM broker WHERE external_broker_id = @BrokerExternalId"
        |> Sql.parameters [ "@BrokerExternalId", Sql.uuid (deconstruct externalId)]
        |> Sql.executeRow mapToDto
        //|> AsyncResult.ofTask
        |> AsyncResult.retn
        |> AsyncResult.map BrokerDto.toDomain
    
    let createBroker connectionString (broker : Broker) =
        async {
            try                    
                let brokerDto =
                    broker
                    |> BrokerDto.ofDomain
                
                let! result =
                    connectionString
                    |> Sql.connect
                    |> Sql.query "INSERT INTO
                            broker (external_broker_id, name)
                            VALUES (@ExternalBrokerId, @Name)"
                    |> Sql.parameters [ ("@ExternalBrokerId", Sql.uuid brokerDto.ExternalBrokerId)
                                        ("@Name", Sql.string brokerDto.Name) ]
                    |> Sql.executeNonQueryAsync
                    |> Async.AwaitTask
                return Ok (result)
            with ex ->
                return Error ex
        }