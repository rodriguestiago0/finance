namespace Finance.Application.FileImporter

open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository

type FetchTicker = ISIN -> AsyncResult<Ticker, exn>
type FetchBroker =  BrokerId -> AsyncResult<Broker, exn>
type SaveTransactions = seq<Transaction> -> AsyncResult<int, exn>

type FileImporterContext =
    { FetchTicker : FetchTicker
      FetchBroker : FetchBroker
      SaveTransactions : SaveTransactions }
with
    static member create sqlConnectionString =
            
        let fetchTicker =
            TickersRepository.getByISIN sqlConnectionString
            
        let fetchBroker =
           BrokersRepository.getById sqlConnectionString
            
        let saveTransactions =
            TransactionsRepository.createTransactions sqlConnectionString 
                                
        { FetchTicker = fetchTicker
          FetchBroker = fetchBroker
          SaveTransactions = saveTransactions }
