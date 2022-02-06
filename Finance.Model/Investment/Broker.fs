namespace Finance.Model.Investment

open System

type BrokerId =
    BrokerId of int
    with
        member this.Deconstruct() =
            let (BrokerId id) = this
            id
                       
        static member empty =
            0 |> BrokerId
            
type ExternalBrokerId =
    ExternalBrokerId of Guid
    with
        member this.Deconstruct() =
            let (ExternalBrokerId id) = this
            id
            
        static member newExternalBrokerId =
            Guid.NewGuid() |> ExternalBrokerId

type Broker =
    { BrokerId : BrokerId
      ExternalBrokerId : ExternalBrokerId
      Name : string}