namespace Finance.Application.Degiro

open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository

type FetchTicker = ISIN -> string -> AsyncResult<Ticker, exn>
type FetchBroker =  ExternalBrokerId -> AsyncResult<Broker, exn>
type SaveTransactions = seq<Transaction> -> AsyncResult<int, exn>

type DegiroContext =
    { FetchTicker : FetchTicker
      FetchBroker : FetchBroker
      SaveTransactions : SaveTransactions }
with
    static member Create sqlConnectionString =
            
        let fetchTicker =
            TickersRepository.getByISINAndExchange sqlConnectionString
            
        let fetchBroker =
           BrokersRepository.getByExternalId sqlConnectionString
            
        let saveTransactions =
            TransactionsRepository.createTransactions sqlConnectionString 
                                
        { FetchTicker = fetchTicker
          FetchBroker = fetchBroker
          SaveTransactions = saveTransactions }
