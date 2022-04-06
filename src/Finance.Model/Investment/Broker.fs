namespace Finance.Model.Investment

open System

type BrokerId =
    BrokerId of Guid
    with
        member this.Deconstruct() =
            let (BrokerId id) = this
            id

        static member empty =
            Guid.NewGuid() |> BrokerId

type Broker =
    { BrokerId : BrokerId
      Name : string
      CountryId : int
      FileImporter : Option<FileImporter> }