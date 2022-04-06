namespace Finance.Repository.Models

open System
open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository.Models

type CloseTransactionDto =
    { BuyTransactionId : Guid
      SellTransactionId : Guid
      Units : decimal }
    with
        static member ofDomain (model : CloseTransaction) : CloseTransactionDto =
            { CloseTransactionDto.BuyTransactionId = deconstruct model.BuyTransaction.TransactionId
              SellTransactionId = deconstruct model.SellTransaction.TransactionId
              Units = model.Units }

type CompleteCloseTransactionDto =
    { BuyTransaction : TransactionDto
      SellTransaction : TransactionDto
      Units : decimal }
    with
        static member toDomain (model : CompleteCloseTransactionDto) : CloseTransaction =
            { SellTransaction = TransactionDto.toDomain model.SellTransaction
              BuyTransaction = TransactionDto.toDomain model.BuyTransaction
              CloseTransaction.Units = model.Units }
