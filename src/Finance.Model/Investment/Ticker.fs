namespace Finance.Model.Investment

open System
open Finance.Model

type TickerId =
    TickerId of Guid
    with
        member this.Deconstruct() =
            let (TickerId id) = this
            id

        static member empty =
            Guid.NewGuid() |> TickerId

type ShortId =
    ShortId of string
    with
        member this.Deconstruct() =
            let (ShortId id) = this
            id

type ISIN =
    ISIN of string
    with
        member this.Deconstruct() =
            let (ISIN id) = this
            id

type TickerType =
    | Bounds
    | ETF
    | Stock
    with
    static member fromInt value =
        match value with
        | 1 -> Ok Bounds
        | 2 -> Ok ETF
        | 3 -> Ok Stock
        | _ -> sprintf "Invalid TickerType - %O" value |> exn |> Result.Error
        
    static member toInt =
        function
        | Bounds -> 1 
        | ETF -> 2 
        | Stock -> 3 

type Ticker =
    { TickerId : TickerId
      ShortId : ShortId
      TickerType : TickerType
      ISIN : ISIN
      Name : string
      Exchange : string
      Currency : Currency }
