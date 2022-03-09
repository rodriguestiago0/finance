namespace Finance.Api.Models

open System
open Finance.FSharp
open Finance.Model.Investment
open Microsoft.FSharp.Core

type BrokerDto =
    { BrokerId : Guid
      Name : string
      Country : string }
    
    with
        static member toDomain (dto : BrokerDto) : Broker = 
            { Broker.BrokerId = BrokerId.empty
              Name = dto.Name
              CountryId = CountryMapping.CountryToCode[dto.Country] }
        
        static member ofDomain (domain : Broker) : BrokerDto = 
            { BrokerDto.BrokerId = deconstruct domain.BrokerId
              Name = domain.Name
              Country = CountryMapping.CodeToCountry[domain.CountryId] }