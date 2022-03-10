namespace Finance.Application.Transaction

open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository
open Microsoft.Extensions.Logging
     
type FetchTickers = unit -> AsyncResult<List<Ticker>, exn>
type FetchOpenTransactionsByTicker = TickerId -> AsyncResult<List<Transaction>, exn>
type SaveClosedTransactions = List<CloseTransaction> -> AsyncResult<int, exn>

type TransactionContext =
    { FetchTickers : FetchTickers
      FetchOpenTransactionsByTicker : FetchOpenTransactionsByTicker
      SaveClosedTransactions : SaveClosedTransactions
      Log : ILogger }
with
    static member create sqlConnectionString log =
        let fetchTickers _ =
            TickersRepository.getAll sqlConnectionString
            
        let fetchOpenTransactionsByTicker =
            TransactionsRepository.getOpenTransactionByTickerId sqlConnectionString

        let saveClosedTransactions =
            CloseTransactionsRepository.createCloseTransactions sqlConnectionString

        { FetchTickers = fetchTickers
          FetchOpenTransactionsByTicker = fetchOpenTransactionsByTicker
          SaveClosedTransactions = saveClosedTransactions
          Log = log }
