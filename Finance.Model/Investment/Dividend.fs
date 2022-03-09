namespace Finance.Model.Investment

open System
open Finance.Model.Investment


type DividendId =
    DividendId of Guid
    with
        member this.Deconstruct() =
            let (DividendId id) = this
            id

        static member empty =
            Guid.NewGuid() |> DividendId


type Dividend =
    { DividendId : DividendId
      TickerId : TickerId
      Value : decimal
      Taxes : Option<decimal> }