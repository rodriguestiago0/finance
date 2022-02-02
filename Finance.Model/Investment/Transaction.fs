namespace Finance.Model.Investment

open System

type TransactionId =
    TransactionId of Guid
    with
        member this.Deconstruct() =
            let (TransactionId id) = this
            id
            
type ExternalTransactionId =
    ExternalTransactionId of string
    with
        member this.Deconstruct() =
            let (ExternalTransactionId id) = this
            id
            
type Broker =
    | Degiro
    with
    static member toInt =
        function
        | Degiro -> 1
    static member fromInt value =
        match value with
        | 1 -> Ok Degiro
        | _ -> sprintf "Invalid Broker - %O" value |> Error 

type Transaction =
    { TransactionId : TransactionId
      ExternalTransactionId : Option<ExternalTransactionId>
      Ticker : Ticker
      Date : DateTimeOffset
      Units : decimal
      Price : decimal
      LocalPrice : Option<decimal>
      Fee : decimal
      ExchangeRate : Option<decimal> 
      Broker : Broker
      Note : Option<string> }