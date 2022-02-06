﻿namespace Finance.Repository.Models

open System
open Finance.FSharp
open Finance.Model.Investment

type TransactionDto =
    { TransactionId : int
      ExternalTransactionId : Guid
      BrokerTransactionId : Option<string>
      TickerId : Guid
      Date : DateTimeOffset
      Units : decimal
      Price : decimal
      LocalPrice : Option<decimal>
      Fee : decimal
      ExchangeRate : Option<decimal>
      Broker : int
      Note : Option<string> }
    
    with
        static member ofDomain (model : Transaction) : TransactionDto =
            { TransactionDto.TransactionId = deconstruct model.TransactionId
              ExternalTransactionId = deconstruct model.ExternalTransactionId
              BrokerTransactionId = model.BrokerTransactionId 
              TickerId = deconstruct model.Ticker.ExternalTickerId
              Date = model.Date
              Units = model.Units
              Price = model.Price
              LocalPrice = model.LocalPrice
              Fee = model.Fee
              ExchangeRate = model.ExchangeRate
              Broker = failwith ""//Broker.toInt model.Broker
              Note = model.Note }
