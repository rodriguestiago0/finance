namespace Finance.Repository

open FSharpPlus
open Microsoft.FSharp.Core
open Npgsql.FSharp
open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

module BrokersRepository =
    type BrokerDto
    with
        static member ofRowReader (read : RowReader) =
            { BrokerDto.BrokerId = read.uuid "broker_id"
              Name = read.string "name"
              CountryId = read.int "country_id" }
    
    let getBrokers connectionString : AsyncResult<List<Broker>, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM finance.broker"
        |> Sql.executeAsync BrokerDto.ofRowReader
        |> AsyncResult.ofTask 
        |> AsyncResult.map (List.map BrokerDto.toDomain)
        
    let getByName connectionString (name : string) : AsyncResult<Broker, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM finance.broker WHERE name = @name"
        |> Sql.parameters [ "@name", Sql.string name]
        |> Sql.executeRowAsync BrokerDto.ofRowReader
        |> AsyncResult.ofTask 
        |> AsyncResult.map BrokerDto.toDomain
        |> AsyncResult.mapError handleExceptions
        
    let getById connectionString (id : BrokerId) : AsyncResult<Broker, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM finance.broker WHERE broker_id = @brokerId"
        |> Sql.parameters [ "@brokerId", Sql.uuid (deconstruct id)]
        |> Sql.executeRowAsync BrokerDto.ofRowReader
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
                            finance.broker (name, country_id)
                            VALUES (@name, @countryId)
                            RETURNING *"
                    |> Sql.parameters [ ("@name", Sql.string brokerDto.Name)
                                        ("@countryId", Sql.int brokerDto.CountryId)]
                    |> Sql.executeRowAsync BrokerDto.ofRowReader
                    |> AsyncResult.ofTask
                    |> AsyncResult.map BrokerDto.toDomain
            with ex ->
                return Error ex
        }
        |> AsyncResult.mapError handleExceptions