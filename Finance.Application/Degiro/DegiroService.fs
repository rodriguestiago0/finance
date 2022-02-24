namespace Finance.Application.Degiro

open System
open System.Globalization
open System.IO
open FSharpPlus
open Finance.Application.Degiro
open Finance.FSharp
open Finance.FSharp.AsyncResult.Operators
open Finance.Model.Investment

[<RequireQualifiedAccess>]
module Degiro =
    type private DegiroTransaction =
        { ISIN : ISIN
          Exchange : string
          BrokerTransactionId : string
          Date : DateTimeOffset
          Units : decimal
          Price : decimal
          LocalPrice : Option<decimal>
          Fee : Option<decimal>
          ExchangeRate : Option<decimal> }

    let importCSV (context : DegiroContext) (externalBrokerId : Guid) (stream : Stream) =
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
                | [| date; hour; _; isin; exchange; _; IsDecimal units; IsDecimal price; _; _; _; _; _ ; IsDecimalOptional exchangeRate; IsDecimalOptional fee; _; _; _; brokerTransactionId |] ->
                    
                    let date = DateTimeOffset.ParseExact($"{date} {hour}", "dd-MM-yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
                    let localPrice =
                        exchangeRate
                        |> Option.map (fun exchangeRate -> price/exchangeRate)
                    { DegiroTransaction.ISIN = isin |> ISIN
                      Exchange = exchange
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
                context.FetchTicker transaction.ISIN transaction.Exchange

            let mk (ticker : Ticker) =
                { Transaction.TransactionId = TransactionId.empty
                  ExternalTransactionId = ExternalTransactionId.newExternalTransactionId
                  BrokerTransactionId = transaction.BrokerTransactionId |> Some
                  TickerId = ticker.TickerId
                  ExternalTickerId = ticker.ExternalTickerId
                  Date = transaction.Date
                  Units = transaction.Units
                  Price = transaction.Price
                  LocalPrice = transaction.LocalPrice
                  Fee = transaction.Fee
                  ExchangeRate = transaction.ExchangeRate
                  BrokerId = broker.BrokerId
                  ExternalBrokerId = broker.ExternalBrokerId
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
                        Fee = Decimal.addOptional t1.Fee t2.Fee }
                let first =
                    transactions
                    |> Seq.head

                transactions
                |> Seq.tail
                |> Seq.fold mergeTransactions first)

        
        let parser (broker, lines) =
            let parseDegiroTransaction degiroTransactions =
                degiroTransactions
                |> Seq.map (convertTransaction broker)
                |> AsyncResult.sequence

            lines
            |> Seq.map parseLine
            |> AsyncResult.sequence
            |> AsyncResult.map groupByExternalTransactionId
            |> AsyncResult.bind parseDegiroTransaction
        
        let broker =
           context.FetchBroker (externalBrokerId |> ExternalBrokerId)
        
        let lines =
            readLines stream
            |> Seq.map (split ",")
            |> AsyncResult.retn
            
        let linesBroker = AsyncResult.lift2 tuple2 broker lines

        linesBroker
        |> AsyncResult.bind parser
        |> AsyncResult.bind context.SaveTransactions
        |> AsyncResult.teeError(fun e -> Console.WriteLine e)