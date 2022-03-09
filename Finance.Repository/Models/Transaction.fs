namespace Finance.Repository.Models

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

        static member toDomain (dto : TransactionDto) : Transaction =
            { Transaction.TransactionId = dto.TransactionId |> TransactionId
              BrokerTransactionId = dto.BrokerTransactionId
              TickerId = dto.TickerId |>  TickerId
              Date = dto.Date
              Units = dto.Units
              Price = dto.Price
              LocalPrice = dto.LocalPrice
              Fee = dto.Fee
              ExchangeRate = dto.ExchangeRate
              BrokerId = dto.BrokerId |>  BrokerId
              Note = dto.Note }
