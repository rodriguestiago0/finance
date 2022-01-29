namespace Finance.Model.Investment

open System

type TransactionId = TransactionId of Guid
type ExternalTransactionId = ExternalTransactionId of string

type Transaction =
    { TransactionId : TransactionId
      ExternalTransactionId : Option<ExternalTransactionId>
      Ticker : Ticker
      Date : DateTimeOffset
      Units : decimal
      Price : decimal
      LocalPrice : Option<decimal>
      Fee : decimal
      ExchangeRate : decimal }