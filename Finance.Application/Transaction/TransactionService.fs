namespace Finance.Application.Transaction

open Finance.Application.Broker
open Finance.FSharp
open Finance.Model.Investment

[<RequireQualifiedAccess>]
module Transaction =
    
    let processCloseTransactionForTicker (context : TransactionContext) (ticker : Ticker) : AsyncResult<unit, exn> =
        let transactions = context.FetchTransactionsByTicker ticker.TickerId
        failwith ""
        
    let processCloseTransaction (context : TransactionContext) =
        let tickers = context.FetchTaxableTickers()
                
        tickers
        |> AsyncResult.bind (fun tickers ->
             tickers
             |> Seq.map (processCloseTransactionForTicker context)
             |> AsyncResult.sequence)