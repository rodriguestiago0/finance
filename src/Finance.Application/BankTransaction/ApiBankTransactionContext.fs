namespace Finance.Application.BankTransaction

open System
open Finance.FSharp
open Finance.HttpClient.Client
open Finance.HttpClient.Model.Response.Nordigen
open Finance.HttpClient.Model.Request.Nordigen

type FetchBanks = Option<string> -> AsyncResult<Institution[], exn>

type ApiBankTransactionContext =
    { FetchBanks : FetchBanks }

with
    static member create sqlConnectionString (secretId : Guid) (secretKey : string) =
        let login = { LoginRequest.SecretId = secretId
                      SecretKey = secretKey }

        let fetchBanks country =
            NordigenClient.getInstitutions login country

        { ApiBankTransactionContext.FetchBanks = fetchBanks }
