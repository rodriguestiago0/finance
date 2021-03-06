namespace Finance.Application.BankTransaction

open Finance.FSharp
open Finance.HttpClient.Model.Request.Nordigen

[<RequireQualifiedAccess>]
module BankTransactionService =
    let importTransactions (context : BankTransactionContext) =
        let authorization =
            { LoginRequest.SecretId = context.SecretId
              SecretKey = context.SecretKey }
            //NordigenClient.login request

        AsyncResult.retn()
