namespace Finance.Application.FileImporter

open System
open System.Globalization
open System.IO
open FSharpPlus
open Finance.Application.FileImporter
open Finance.FSharp
open Finance.Model.Investment

[<RequireQualifiedAccess>]
module Revolut =
    type private RevolutTransaction =
        { ISIN : ISIN
          Date : DateTimeOffset
          Units : decimal
          Price : decimal
          LocalPrice : Option<decimal>
          ExchangeRate : Option<decimal> }

    type private ActionType =
        | Sell
        | Buy
    let private (|ActionType|_|) (str : string) =
        match str with
        | "SELL" -> Some ActionType.Sell
        | "BUY" -> Some ActionType.Buy
        | _ -> None

    let importCSV (context : FileImporterContext) (broker : Broker) (stream : Stream) =
        let readLines (stream : Stream) = seq {
            use sr = new StreamReader (stream)
            let _ = sr.ReadLine ()
            while not sr.EndOfStream do
                yield sr.ReadLine ()
        }

        let (|Transaction|_|) (parts : string[]) =
            if parts.Length <> 19 then
                None
            else
                match parts with
                | [| date; isin; exchange; ActionType actionType; IsDecimal units; IsDecimal price; _; _; IsDecimalOptional exchangeRate; |] ->

                    let date = DateTimeOffset.ParseExact($"{date}", "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
                    let localPrice =
                        exchangeRate
                        |> Option.map (fun exchangeRate -> price/exchangeRate)

                    let units =
                        match actionType with
                        | Buy -> units
                        | Sell -> units * -1m

                    { RevolutTransaction.ISIN = isin |> ISIN
                      Date = date
                      Units = units
                      Price = price
                      LocalPrice = localPrice
                      ExchangeRate = exchangeRate }
                    |> Some
                | _ -> None

        let parseLine (line : string[]) =
            match line with
            | Transaction t ->
                t |> AsyncResult.retn
            | _ -> "Invalid line" |> exn |> AsyncResult.error

        let convertTransaction (broker : Broker) (transaction : RevolutTransaction) =
            let ticker =
                context.FetchTicker transaction.ISIN

            let mk (ticker : Ticker) =
                { Transaction.TransactionId = TransactionId.empty
                  BrokerTransactionId = None
                  TickerId = ticker.TickerId
                  Date = transaction.Date
                  Units = transaction.Units
                  Price = transaction.Price
                  LocalPrice = None
                  Fee = None
                  ExchangeRate = transaction.ExchangeRate
                  BrokerId = broker.BrokerId
                  Note = None }

            ticker
            |> AsyncResult.map mk


        let parser broker lines =
            let parseDegiroTransaction degiroTransactions =
                degiroTransactions
                |> Seq.map (convertTransaction broker)
                |> AsyncResult.sequence

            lines
            |> Seq.map parseLine
            |> AsyncResult.sequence
            |> AsyncResult.bind parseDegiroTransaction

        let lines =
            readLines stream
            |> Seq.map (split ",")
            |> AsyncResult.retn


        lines
        |> AsyncResult.bind (parser broker)
        |> AsyncResult.bind context.SaveTransactions