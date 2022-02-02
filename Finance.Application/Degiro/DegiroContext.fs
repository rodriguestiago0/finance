namespace Finance.Application.Degiro

open Npgsql.FSharp
open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository

type FetchTicker = ISIN -> string -> AsyncResult<Option<Ticker>, exn>
type SaveTransaction = Transaction -> AsyncResult<unit, exn>

type DegiroContext =
    { FetchTicker : FetchTicker
      SaveTransaction : SaveTransaction }
with
    static member Create sqlConnectionString =
        let sql =
            sqlConnectionString
            |> Sql.connect
            
        let fetchTicker =
            TickersRepository.getByISINAndExchange sql
            
        let saveTransaction =
            TransactionsRepository.create sql
                        
        { FetchTicker = fetchTicker
          SaveTransaction = saveTransaction }
