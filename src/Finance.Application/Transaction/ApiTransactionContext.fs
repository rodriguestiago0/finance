namespace Finance.Application.Transaction

open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository

type FetchTransactionByBrokerId = BrokerId -> AsyncResult<List<Transaction>, exn>
type FetchClosedTransactionsByTicker = TickerId -> AsyncResult<List<CloseTransaction>, exn>


type ApiTransactionContext =
    { FetchTransactionByBrokerId : FetchTransactionByBrokerId
      FetchClosedTransactionsByTicker : FetchClosedTransactionsByTicker }
with
    static member create sqlConnectionString =
        let fetchTransactionByBrokerId =
            TransactionsRepository.getByBrokerExternalId sqlConnectionString

        let fetchClosedTransactionsByTicker =
            CloseTransactionsRepository.getClosedTransactionByTickerId sqlConnectionString

        { FetchTransactionByBrokerId = fetchTransactionByBrokerId
          FetchClosedTransactionsByTicker = fetchClosedTransactionsByTicker }
