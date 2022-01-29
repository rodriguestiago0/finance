namespace Finance.Model.Investment

open Finance.Model

type TickerId = TickerId of string

type TickerType =
    | Bounds
    | ETF
    | Stock

type Ticker =
    { TickerId : TickerId
      TickerType : TickerType
      Name : string
      Exchange : string
      Currency : Currency }
