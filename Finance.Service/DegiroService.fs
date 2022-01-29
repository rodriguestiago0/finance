namespace Finance.Service

open System
open System.Globalization
open System.IO
open Finance.FSharp
open Finance.FSharp.AsyncResult.Operators
open Finance.Model.Investment

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
                | [| date; hour; _; isin; exchange; _; IsDecimal units; IsDecimal price; _; _; _; _; _ ; IsDecimal exchangeRate; IsDecimal fee; _; _; _; externalTransactionId |] ->
                    
                    let date = DateTimeOffset.ParseExact($"{date} {hour}", "DD-MM-YYYY HH:MM", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)
                    Some(isin, exchange, (externalTransactionId |> ExternalTransactionId |> Some), date, units, price, fee, exchangeRate)
                | _ -> None
                
        let parseLine (line : string[]) =
            match line with
            | Transaction (isin, exchange, externalTransactionId, date, units, price, fee, exchangeRate) ->
                let ticker = context.FetchTicker isin exchange
                let mk ticker =
                    { Transaction.TransactionId = Guid.NewGuid() |> TransactionId
                      ExternalTransactionId = externalTransactionId
                      Ticker = ticker
                      Date = date
                      Units = units
                      Price = price
                      LocalPrice = None
                      Fee = fee
                      ExchangeRate = exchangeRate }
                
                mk
                <!> ticker
                |> AsyncResult.bind context.SaveTransaction
            | _ -> "Invalid line" |> exn |> AsyncResult.error
        
        let x =
            readLines stream
            |> Seq.map (split ",")
            |> Seq.map parseLine
            |> AsyncResult.sequence
        
        failwith ""