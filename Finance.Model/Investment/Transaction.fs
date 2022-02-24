namespace Finance.Model.Investment

open System

type TransactionId =
    TransactionId of int
    with
        member this.Deconstruct() =
            let (TransactionId id) = this
            id
            
        static member empty =
            0 |> TransactionId
          
type ExternalTransactionId =
    ExternalTransactionId of Guid
    with
        member this.Deconstruct() =
            let (ExternalTransactionId id) = this
            id
            
        static member newExternalTransactionId =
            Guid.NewGuid() |> ExternalTransactionId

type Transaction =
    { TransactionId : TransactionId
      ExternalTransactionId : ExternalTransactionId
      BrokerTransactionId : Option<string>
      TickerId : TickerId
      ExternalTickerId : ExternalTickerId
      Date : DateTimeOffset
      Units : decimal
      Price : decimal
      LocalPrice : Option<decimal>
      Fee : Option<decimal>
      ExchangeRate : Option<decimal> 
      BrokerId : BrokerId
      ExternalBrokerId : ExternalBrokerId
      Note : Option<string> }