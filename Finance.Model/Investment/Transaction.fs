namespace Finance.Model.Investment

open System

type TransactionId =
    TransactionId of Guid
    with
        member this.Deconstruct() =
            let (TransactionId id) = this
            id

        static member empty =
            Guid.NewGuid() |> TransactionId

type Transaction =
    { TransactionId : TransactionId
      BrokerTransactionId : Option<string>
      TickerId : TickerId
      Date : DateTimeOffset
      Units : decimal
      Price : decimal
      LocalPrice : Option<decimal>
      Fee : Option<decimal>
      ExchangeRate : Option<decimal> 
      BrokerId : BrokerId
      Note : Option<string> }