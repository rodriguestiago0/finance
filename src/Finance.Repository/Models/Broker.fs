namespace Finance.Repository.Models

open System
open Finance.FSharp
open Finance.Model.Investment

type BrokerDto =
    { BrokerId : Guid
      Name : string
      CountryId : int
      FileImporter : Option<int> }
    with
        static member ofDomain (model : Broker) : BrokerDto =
            { BrokerDto.BrokerId = deconstruct model.BrokerId
              Name = model.Name
              CountryId = model.CountryId
              FileImporter = model.FileImporter |> Option.map FileImporter.toInt }
            
        static member toDomain (dto : BrokerDto) : Result<Broker,exn> =
            let fileImporter =
                dto.FileImporter
                |> Option.map (fun f ->
                    FileImporter.fromInt f
                    |> Result.map Some)
                |> Option.defaultValue (Ok None)

            let mk fileImporter =
                { Broker.BrokerId = dto.BrokerId |> BrokerId
                  Name = dto.Name
                  CountryId = dto.CountryId
                  FileImporter = fileImporter }
            fileImporter
            |> Result.map mk