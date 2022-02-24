namespace Finance.Repository

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
        |> AsyncResult.mapError handleExceptions
        
    let getByExternalId connectionString (externalId : ExternalBrokerId) : AsyncResult<Broker, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM broker WHERE external_broker_id = @BrokerExternalId"
        |> Sql.parameters [ "@BrokerExternalId", Sql.uuid (deconstruct externalId)]
        |> Sql.executeRowAsync mapToDto
        |> AsyncResult.ofTask
        |> AsyncResult.map BrokerDto.toDomain
        |> AsyncResult.mapError handleExceptions
    
    let createBroker connectionString (broker : Broker) : AsyncResult<Broker, exn> =
        async {
            try                    
                let brokerDto =
                    broker
                    |> BrokerDto.ofDomain
                
                return!
                    connectionString
                    |> Sql.connect
                    |> Sql.query "INSERT INTO
                            broker (external_broker_id, name)
                            VALUES (@ExternalBrokerId, @Name)
                            RETURNING *"
                    |> Sql.parameters [ ("@ExternalBrokerId", Sql.uuid brokerDto.ExternalBrokerId)
                                        ("@Name", Sql.string brokerDto.Name) ]
                    |> Sql.executeRowAsync mapToDto
                    |> AsyncResult.ofTask
                    |> AsyncResult.map BrokerDto.toDomain
            with ex ->
                return Error ex
        }
        |> AsyncResult.mapError handleExceptions