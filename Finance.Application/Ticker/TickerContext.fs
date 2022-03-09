namespace Finance.Application.Ticker

open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository

type FetchTickers = unit -> AsyncResult<List<Ticker>, exn>
type FetchTickerById = TickerId -> AsyncResult<Ticker, exn>
type SaveTicker = Ticker -> AsyncResult<Ticker, exn>

type TickerContext =
    { FetchTickers : FetchTickers
      FetchTickerById : FetchTickerById
      SaveTicker : SaveTicker }
with
    static member Create sqlConnectionString =
        let fetchTickers _ =
            TickersRepository.getAll sqlConnectionString
            
        let fetchTickerById =
            TickersRepository.getById sqlConnectionString
            
        let saveTicker =
            TickersRepository.createTicker sqlConnectionString 
                        
        { FetchTickerById = fetchTickerById
          SaveTicker = saveTicker
          FetchTickers = fetchTickers }
