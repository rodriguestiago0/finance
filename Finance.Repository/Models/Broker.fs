namespace Finance.Repository.Models

open System
open Finance.FSharp
open Finance.Model.Investment

type BrokerDto =
    { BrokerId : int
      ExternalBrokerId : Guid
      Name : string }
    
    with
        static member ofDomain (model : Broker) : BrokerDto =
            { BrokerDto.BrokerId = deconstruct model.BrokerId
              ExternalBrokerId = deconstruct model.ExternalBrokerId
              Name = model.Name }
            
        static member toDomain (dto : BrokerDto) : Broker =
            { Broker.BrokerId = dto.BrokerId |> BrokerId
              ExternalBrokerId = dto.ExternalBrokerId |> ExternalBrokerId
              Name = dto.Name }