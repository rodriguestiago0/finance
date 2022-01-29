namespace Finance.Service

open System
open FSharp.Data
open Finance.FSharp
open Finance.Model.Investment

type FetchTicker = string -> string -> AsyncResult<Ticker, exn>
type SaveTransaction = Transaction -> AsyncResult<unit, exn>

type DegiroContext =
    { FetchTicker : FetchTicker
      SaveTransaction : SaveTransaction }
with
    static member Create() =
        failwith ""
