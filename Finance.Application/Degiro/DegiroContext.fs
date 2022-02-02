namespace Finance.Application.Degiro

open Npgsql.FSharp
open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository

type FetchTicker = ISIN -> string -> AsyncResult<Ticker, exn>
type SaveTransactions = Transaction[] -> AsyncResult<unit, exn>

type DegiroContext =
    { FetchTicker : FetchTicker
      SaveTransactions : SaveTransactions }
with
    static member Create sqlConnectionString =
            
        let fetchTicker =
            TickersRepository.getByISINAndExchange sqlConnectionString
            
        let saveTransactions =
            TransactionsRepository.createTransactions sqlConnectionString 
                        
        { FetchTicker = fetchTicker
          SaveTransactions = saveTransactions }
