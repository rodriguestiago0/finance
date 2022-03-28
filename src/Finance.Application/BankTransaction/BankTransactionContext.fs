namespace Finance.Application.BankTransaction

open System

type BankTransactionContext =
    { SecretId : Guid
      SecretKey : string }

with
    static member create sqlConnectionString (secretId : string) (secretKey : string) log =
        { BankTransactionContext.SecretId = secretId |> Guid
          SecretKey = secretKey }
