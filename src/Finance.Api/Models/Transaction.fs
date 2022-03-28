namespace Finance.Api.Models

open System
open Finance.FSharp
open Finance.Model.Investment

type TransactionDto =
    { TransactionId : Guid
      BrokerTransactionId : Option<string>
      TickerId : Guid
      Date : DateTimeOffset
      Units : decimal
      Price : decimal
      LocalPrice : Option<decimal>
      Fee : Option<decimal>
      ExchangeRate : Option<decimal>
      BrokerId : Guid
      Note : Option<string> }
    
    with
        static member ofDomain (model : Transaction) : TransactionDto =
            { TransactionDto.TransactionId = deconstruct model.TransactionId
              BrokerTransactionId = model.BrokerTransactionId 
              TickerId = deconstruct model.TickerId
              Date = model.Date
              Units = model.Units
              Price = model.Price
              LocalPrice = model.LocalPrice
              Fee = model.Fee
              ExchangeRate = model.ExchangeRate
              BrokerId = deconstruct model.BrokerId
              Note = model.Note }
