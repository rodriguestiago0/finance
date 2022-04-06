namespace Finance.Application.Transaction

open System
open Finance.FSharp
open Finance.Model.Investment
open Microsoft.Extensions.Logging

[<RequireQualifiedAccess>]
module Transaction =
    
    let private processCloseTransactionForTicker (context : TransactionContext) (ticker : Ticker) : AsyncResult<int, exn> =
        context.Log.LogInformation $"Start calculating close transaction for Ticket {deconstruct ticker.TickerId}"
        let transactions =
            context.FetchOpenTransactionsByTicker ticker.TickerId
            |> AsyncResult.map(fun transactions ->
                let sellTransactions =
                    transactions
                    |> List.filter (fun t -> t.Units < 0m)
                    |> List.sortBy (fun t -> t.Date)

                let buyTransactions =
                    transactions
                    |> List.filter (fun t -> t.Units > 0m)
                    |> List.sortBy (fun t -> t.Date)
                sellTransactions, buyTransactions
            )
            |> Async.map(Result.bind(fun (sell, buy) ->
                let sellUnits = sell |> List.sumBy(fun s -> s.Units)
                let buyUnits = buy |> List.sumBy(fun s -> s.Units)
                if sellUnits > buyUnits then
                    sprintf "More sell unit than buy units" |> exn |> Error
                else
                    (sell, buy) |> Ok
                ))

        let mkClosedTransaction (buyTransaction : Transaction) (sellTransaction : Transaction) (units : decimal) =
            { CloseTransaction.Units = units
              BuyTransaction = buyTransaction
              SellTransaction = sellTransaction }

        let rec loop
            (closedTransactions : List<CloseTransaction>)
            (sellTransactions : List<Transaction>, buyTransactions : List<Transaction>) : List<CloseTransaction> =

            if sellTransactions.Length = 0 ||  buyTransactions.Length = 0 then
                closedTransactions
            else
                let buy = buyTransactions.Head
                let sell = sellTransactions.Head
                let minUnits = Math.Min(buy.Units, Math.Abs(sell.Units))
                let closedTransaction = mkClosedTransaction buy sell minUnits
                let closedTransactions = closedTransaction :: closedTransactions

                let buy = { buy with Units = buy.Units - minUnits }
                let sell = { sell with Units = sell.Units + minUnits }

                let buyTransactions =
                    if buy.Units = 0m then
                        buyTransactions.Tail
                    else
                        buy :: buyTransactions.Tail

                let sellTransactions =
                    if sell.Units = 0m then
                        sellTransactions.Tail
                    else
                        sell :: sellTransactions.Tail

                loop closedTransactions (sellTransactions, buyTransactions)
        transactions
        |> AsyncResult.map (loop [])
        |> AsyncResult.bind context.SaveClosedTransactions
        |> AsyncResult.tee(fun _ -> context.Log.LogInformation $"Done calculating close transaction for Ticket {deconstruct ticker.TickerId}")
        |> AsyncResult.teeError(fun e -> context.Log.LogInformation $"Failed calculating close transaction for Ticket {deconstruct ticker.TickerId} - {e.Message}")

    let processCloseTransaction (context : TransactionContext) =
        let tickers = context.FetchTickers()
                
        tickers
        |> AsyncResult.teeError(fun e -> context.Log.LogInformation $"Failed to load the taxable ticker = {e}")
        |> AsyncResult.bind (fun tickers ->
             tickers
             |> Seq.map (processCloseTransactionForTicker context)
             |> AsyncResult.sequence)
        |> AsyncResult.tee(fun r -> context.Log.LogInformation $"Added {r |> Seq.sum} closed transactions")
