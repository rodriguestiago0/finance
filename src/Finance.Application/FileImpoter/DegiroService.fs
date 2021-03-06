namespace Finance.Application.FileImporter

open System
open System.Globalization
open System.IO
open FSharpPlus
open Finance.Application.FileImporter
open Finance.FSharp
open Finance.FSharp.AsyncResult.Operators
open Finance.Model.Investment

[<RequireQualifiedAccess>]
module Degiro =
    type private DegiroTransaction =
        { ISIN : ISIN
          BrokerTransactionId : string
          Date : DateTimeOffset
          Units : decimal
          Price : decimal
          LocalPrice : Option<decimal>
          Fee : Option<decimal>
          ExchangeRate : Option<decimal> }

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
                | [| date; hour; _; isin; _; _; IsDecimal units; IsDecimal price; _; _; _; _; _ ; IsDecimalOptional exchangeRate; IsDecimalOptional fee; _; _; _; brokerTransactionId |] ->
                    
                    let date = DateTimeOffset.ParseExact($"{date} {hour}", "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
                    let localPrice =
                        exchangeRate
                        |> Option.map (fun exchangeRate -> price/exchangeRate)
                    { DegiroTransaction.ISIN = isin |> ISIN
                      BrokerTransactionId = brokerTransactionId
                      Date = date
                      Units = units
                      Price = price
                      LocalPrice = localPrice
                      Fee = fee
                      ExchangeRate = exchangeRate }
                    |> Some
                | _ -> None
                
        let parseLine (line : string[]) =
            match line with
            | Transaction t ->
                t |> AsyncResult.retn
            | _ -> "Invalid line" |> exn |> AsyncResult.error

        let convertTransaction (broker : Broker) (transaction : DegiroTransaction) =
            let ticker =
                context.FetchTicker transaction.ISIN

            let mk (ticker : Ticker) =
                { Transaction.TransactionId = TransactionId.empty
                  BrokerTransactionId = transaction.BrokerTransactionId |> Some
                  TickerId = ticker.TickerId
                  Date = transaction.Date
                  Units = transaction.Units
                  Price = transaction.Price
                  LocalPrice = transaction.LocalPrice
                  Fee = transaction.Fee
                  ExchangeRate = transaction.ExchangeRate
                  BrokerId = broker.BrokerId
                  Note = None }

            mk
            <!> ticker

        let groupByExternalTransactionId (transactions : seq<DegiroTransaction>) =
            transactions
            |> Seq.groupBy(fun t -> t.BrokerTransactionId)
            |> Seq.map(fun (_, transactions) ->
                let mergeTransactions (t1 : DegiroTransaction) (t2 : DegiroTransaction) =
                    { t1 with
                        Units = t1.Units + t2.Units
                        Fee = addOptional t1.Fee t2.Fee }
                let first =
                    transactions
                    |> Seq.head

                transactions
                |> Seq.tail
                |> Seq.fold mergeTransactions first)

        
        let parser broker lines =
            let parseDegiroTransaction degiroTransactions =
                degiroTransactions
                |> Seq.map (convertTransaction broker)
                |> AsyncResult.sequence

            lines
            |> Seq.map parseLine
            |> AsyncResult.sequence
            |> AsyncResult.map groupByExternalTransactionId
            |> AsyncResult.bind parseDegiroTransaction

        let lines =
            readLines stream
            |> Seq.map (split ",")
            |> AsyncResult.retn

        lines
        |> AsyncResult.bind (parser broker)
        |> AsyncResult.bind context.SaveTransactions