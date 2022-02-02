namespace Finance.Application.Degiro
open System.Threading
open Finance.FSharp
open Finance.Model.Investment

type FetchTicker = string -> string -> AsyncResult<Ticker, exn>
type SaveTransaction = Transaction -> AsyncResult<unit, exn>

type DegiroContext =
    { FetchTicker : FetchTicker
      SaveTransaction : SaveTransaction
      CancellationToken : CancellationToken }
with
    static member Create(I) =
        failwith ""
