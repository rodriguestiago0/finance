namespace Finance.Application.Degiro

open System
open System.Globalization
open System.IO
open Finance.FSharp
open Finance.FSharp.AsyncResult.Operators
open Finance.Model.Investment

[<RequireQualifiedAccess>]
module Degiro =
    let importCSV (context : DegiroContext) (stream : Stream) =
        
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
                
        let parseLine (line : string[]) =
            match line with
            | Transaction (isin, exchange, brokerTransactionId, date, units, price, fee, exchangeRate) ->
                let ticker =
                    context.FetchTicker isin exchange
                    
                let broker =
                    context.FetchBroker "Degiro"
                    
                let mk ticker broker =
                    { Transaction.TransactionId = TransactionId.empty
                      ExternalTransactionId = ExternalTransactionId.newExternalTransactionId
                      BrokerTransactionId = brokerTransactionId
                      Ticker = ticker
                      Date = date
                      Units = units
                      Price = price
                      LocalPrice = None
                      Fee = fee
                      ExchangeRate = exchangeRate
                      Broker = broker
                      Note = None }
                
                mk
                <!> ticker
                <*> broker
            | _ -> "Invalid line" |> exn |> AsyncResult.error
        
        let x =
            readLines stream
            |> Seq.map (split ",")
            |> Seq.map parseLine
            |> AsyncResult.sequence
            |> AsyncResult.bind (Array.ofSeq >> context.SaveTransactions)
            
        failwith ""