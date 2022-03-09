namespace Finance.Application.Transaction

open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository
     

type FetchTransactionByBrokerId = BrokerId -> AsyncResult<List<Transaction>, exn>

type ApiTransactionContext =
    { FetchTransactionByBrokerId : FetchTransactionByBrokerId}
with
    static member Create sqlConnectionString =
        let fetchTransactionByBrokerId =
            TransactionsRepository.getByBrokerExternalId sqlConnectionString

        { FetchTransactionByBrokerId = fetchTransactionByBrokerId }
