namespace Finance.Repository.Models

open System
open Finance.FSharp
open Finance.Model.Investment

type CloseTransactionDto =
    { BuyTransactionId : int
      SellTransactionId : int
      Units : decimal }
    
    with
        static member ofDomain (model : CloseTransaction) : CloseTransactionDto =
            { CloseTransactionDto.BuyTransactionId = deconstruct model.BuyTransaction.TransactionId
              SellTransactionId = deconstruct model.SellTransaction.TransactionId
              Units = model.Units }
