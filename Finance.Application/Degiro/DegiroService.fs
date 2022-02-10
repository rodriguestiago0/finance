namespace Finance.Application.Degiro

open System
open System.Globalization
open System.IO
open FSharpPlus
open Finance.FSharp
open Finance.FSharp.AsyncResult.Operators
open Finance.Model.Investment

[<RequireQualifiedAccess>]
module Degiro =
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
                | [| date; hour; _; isin; exchange; _; IsDecimal units; IsDecimal price; _; _; _; _; _ ; IsDecimalOptional exchangeRate; IsDecimal fee; _; _; _; externalTransactionId |] ->
                    
                    let date = DateTimeOffset.ParseExact($"{date} {hour}", "DD-MM-YYYY HH:MM", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
                    Some(isin |> ISIN, exchange, (externalTransactionId |> Some), date, units, price, fee, exchangeRate)
                | _ -> None
                
        let parseLine (broker : Broker) (line : string[]) =
            match line with
            | Transaction (isin, exchange, brokerTransactionId, date, units, price, fee, exchangeRate) ->
                let ticker =
                    context.FetchTicker isin exchange
                    
                let mk (ticker : Ticker) =
                    { Transaction.TransactionId = TransactionId.empty
                      ExternalTransactionId = ExternalTransactionId.newExternalTransactionId
                      BrokerTransactionId = brokerTransactionId
                      TickerId = ticker.TickerId
                      ExternalTickerId = ticker.ExternalTickerId
                      Date = date
                      Units = units
                      Price = price
                      LocalPrice = None
                      Fee = fee
                      ExchangeRate = exchangeRate
                      BrokerId = broker.BrokerId
                      ExternalBrokerId = broker.ExternalBrokerId
                      Note = None }
                
                mk
                <!> ticker
            | _ -> "Invalid line" |> exn |> AsyncResult.error
        
        
        let parser (broker, lines)  =
            lines
            |> Seq.map (parseLine broker)
            |> AsyncResult.sequence
        
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