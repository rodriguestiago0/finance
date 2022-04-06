namespace Finance.Api.Models

open System
open Finance.FSharp
open Finance.Model.Investment
open Microsoft.FSharp.Core

type BrokerDto =
    { BrokerId : Guid
      Name : string
      Country : string
      FileImporter : Option<string> }
    
    with
        static member toDomain (dto : BrokerDto) : Result<Broker, exn> =
            let fileImporter =
                dto.FileImporter
                |> Option.map (fun f ->
                    fromString<FileImporter> f
                    |> Result.map Some)
                |> Option.defaultValue (Ok None)
            let mk fileImporter =
                { Broker.BrokerId = BrokerId.empty
                  Name = dto.Name
                  CountryId = CountryMapping.CountryToCode[dto.Country]
                  FileImporter = fileImporter }
            fileImporter
            |> Result.map mk
        
        static member ofDomain (domain : Broker) : BrokerDto = 
            { BrokerDto.BrokerId = deconstruct domain.BrokerId
              Name = domain.Name
              Country = CountryMapping.CodeToCountry[domain.CountryId]
              FileImporter = domain.FileImporter |> Option.map getCaseName }