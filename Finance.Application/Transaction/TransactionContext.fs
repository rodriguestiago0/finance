namespace Finance.Application.Transaction

open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository
open Microsoft.Extensions.Logging
     
type FetchTaxableTickers = unit -> AsyncResult<List<Ticker>, exn>
type FetchOpenTransactionsByTicker = TickerId -> AsyncResult<List<Transaction>, exn>
type SaveClosedTransactions = List<CloseTransaction> -> AsyncResult<int, exn>

type TransactionContext =
    { FetchTaxableTickers : FetchTaxableTickers
      FetchOpenTransactionsByTicker : FetchOpenTransactionsByTicker
      SaveClosedTransactions : SaveClosedTransactions
      Log : ILogger }
with
    static member Create sqlConnectionString log =
        let fetchTaxableTickers _ =
            TickersRepository.getTaxableTickers sqlConnectionString
            
        let fetchOpenTransactionsByTicker =
            TransactionsRepository.getOpenTransactionByTickerId sqlConnectionString

        let saveClosedTransactions =
            CloseTransactionsRepository.createCloseTransactions sqlConnectionString

        { FetchTaxableTickers = fetchTaxableTickers
          FetchOpenTransactionsByTicker = fetchOpenTransactionsByTicker
          SaveClosedTransactions = saveClosedTransactions
          Log = log }
