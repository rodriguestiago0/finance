namespace Finance.Repository.Models

open System
open FSharpPlus
open Finance.Model
open Finance.Model.Investment

type TickerDto =
    { TickerId : Guid
      ShortId : string
      TickerType : int
      ISIN : string
      Name : string
      Exchange : string
      Currency : int }
    
    with
        static member toDomain (dto : TickerDto) : Result<Ticker, exn> =
            let tickerType = TickerType.fromInt dto.TickerType
            let currency = Currency.fromInt dto.Currency
            let mk tickerType currency = 
                { Ticker.TickerId = dto.TickerId |> TickerId
                  ShortId = dto.ShortId |> ShortId
                  TickerType = tickerType
                  ISIN = dto.ISIN |> ISIN
                  Name = dto.Name
                  Exchange = dto.Exchange
                  Currency = currency }
            mk
            <!> tickerType
            <*> currency
            