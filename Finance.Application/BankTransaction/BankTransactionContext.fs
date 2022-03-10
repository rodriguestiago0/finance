namespace Finance.Application.BankTransaction

type BankTransactionContext =
    { SecretId : string
      SecretKey : string }

with
    static member create sqlConnectionString secretId secretKey log =
        { BankTransactionContext.SecretId = secretId
          SecretKey = secretKey }
