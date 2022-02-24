namespace Finance.Repository.Models

open System
open Finance.FSharp
open Finance.Model.Investment

type TransactionDto =
    { TransactionId : int
      ExternalTransactionId : Guid
      BrokerTransactionId : Option<string>
      TickerId : int
      Date : DateTimeOffset
      Units : decimal
      Price : decimal
      LocalPrice : Option<decimal>
      Fee : Option<decimal>
      ExchangeRate : Option<decimal>
      BrokerId : int
      Note : Option<string> }
    
    with
        static member ofDomain (model : Transaction) : TransactionDto =
            { TransactionDto.TransactionId = deconstruct model.TransactionId
              ExternalTransactionId = deconstruct model.ExternalTransactionId
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

        static member toDomain (dto : TransactionDto, externalTickerId : Guid, externalBrokerId : Guid) : Transaction =
            { Transaction.TransactionId = dto.TransactionId |> TransactionId
              ExternalTransactionId = dto.ExternalTransactionId |> ExternalTransactionId
              BrokerTransactionId = dto.BrokerTransactionId 
              TickerId = dto.TickerId |>  TickerId
              ExternalTickerId = externalTickerId |> ExternalTickerId
              Date = dto.Date
              Units = dto.Units
              Price = dto.Price
              LocalPrice = dto.LocalPrice
              Fee = dto.Fee
              ExchangeRate = dto.ExchangeRate
              BrokerId = dto.BrokerId |>  BrokerId
              ExternalBrokerId = externalBrokerId |> ExternalBrokerId
              Note = dto.Note }
