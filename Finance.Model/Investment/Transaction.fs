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

type Transaction =
    { TransactionId : TransactionId
      ExternalTransactionId : Option<ExternalTransactionId>
      Ticker : Ticker
      Date : DateTimeOffset
      Units : decimal
      Price : decimal
      LocalPrice : Option<decimal>
      Fee : decimal
      ExchangeRate : Option<decimal> }