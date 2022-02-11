namespace Finance.Application.Transaction

open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository
     
type FetchTaxableTickers = unit -> AsyncResult<List<Ticker>, exn>
type FetchTransactionsByTicker = TickerId -> AsyncResult<List<Transaction>, exn>

type TransactionContext =
    { FetchTaxableTickers : FetchTaxableTickers
      FetchTransactionsByTicker : FetchTransactionsByTicker }
with
    static member Create sqlConnectionString =
        let fetchTaxableTickers _ =
            TickersRepository.getTaxableTickers sqlConnectionString
            
        let fetchTransactionsByTicker =
            TransactionsRepository.getByTickerId sqlConnectionString
            
        { FetchTaxableTickers = fetchTaxableTickers
          FetchTransactionsByTicker = fetchTransactionsByTicker }
