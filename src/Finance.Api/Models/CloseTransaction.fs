namespace Finance.Api.Models

open Finance.Api.Models
open Finance.Model.Investment

type CloseTransactionDto =
    { BuyTransaction : TransactionDto
      SellTransaction : TransactionDto
      Units : decimal }
    with
        static member ofDomain (model : CloseTransaction) : CloseTransactionDto =
            { CloseTransactionDto.BuyTransaction = TransactionDto.ofDomain model.BuyTransaction
              SellTransaction = TransactionDto.ofDomain model.SellTransaction
              Units = model.Units }

