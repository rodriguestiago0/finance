namespace Finance.Repository.Models

open System
open FSharpPlus
open Finance.FSharp
open Finance.Model
open Finance.Model.Investment

type TickerDto =
    { TickerId : int
      ExternalTickerId : Guid
      ShortId : string
      TickerType : int
      ISIN : string
      Name : string
      Exchange : string
      Currency : int
      TaxationRequired : bool }
    
    with
        static member toDomain (dto : TickerDto) : Result<Ticker, exn> =
            let tickerType = TickerType.fromInt dto.TickerType
            let currency = Currency.fromInt dto.Currency
            let mk tickerType currency = 
                { Ticker.TickerId = dto.TickerId |> TickerId
                  ExternalTickerId = dto.ExternalTickerId |> ExternalTickerId
                  ShortId = dto.ShortId |> ShortId
                  TickerType = tickerType
                  ISIN = dto.ISIN |> ISIN
                  Name = dto.Name
                  Exchange = dto.Exchange
                  Currency = currency
                  TaxationRequired = dto.TaxationRequired }
            mk
            <!> tickerType
            <*> currency
            
        static member ofDomain (domain : Ticker) :  TickerDto =
            { TickerDto.TickerId = deconstruct domain.TickerId
              ExternalTickerId = deconstruct domain.ExternalTickerId
              ShortId = deconstruct domain.ShortId
              TickerType = TickerType.toInt domain.TickerType
              ISIN = deconstruct domain.ISIN
              Name = domain.Name
              Exchange = domain.Exchange
              Currency = Currency.toInt domain.Currency
              TaxationRequired = domain.TaxationRequired }            