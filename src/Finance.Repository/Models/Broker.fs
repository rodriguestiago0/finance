namespace Finance.Repository.Models

open System
open Finance.FSharp
open Finance.Model.Investment

type BrokerDto =
    { BrokerId : Guid
      Name : string
      CountryId : int }
    with
        static member ofDomain (model : Broker) : BrokerDto =
            { BrokerDto.BrokerId = deconstruct model.BrokerId
              Name = model.Name
              CountryId = model.CountryId }
            
        static member toDomain (dto : BrokerDto) : Broker =
            { Broker.BrokerId = dto.BrokerId |> BrokerId
              Name = dto.Name
              CountryId = dto.CountryId }