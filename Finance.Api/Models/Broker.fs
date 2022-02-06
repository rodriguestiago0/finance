namespace Finance.Api.Models

open System
open Finance.FSharp
open Finance.Model.Investment
open Microsoft.FSharp.Core

type BrokerDto =
    { BrokerId : Guid
      Name : string }
    
    with
        static member toDomain (dto : BrokerDto) : Broker = 
            { Broker.BrokerId = BrokerId.empty
              ExternalBrokerId = dto.BrokerId |> ExternalBrokerId
              Name = dto.Name }
        
        static member ofDomain (domain : Broker) : BrokerDto = 
            { BrokerDto.BrokerId = deconstruct domain.ExternalBrokerId
              Name = domain.Name }