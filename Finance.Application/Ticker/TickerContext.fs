namespace Finance.Application.Ticker

open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository

type FetchTickers = unit -> AsyncResult<List<Ticker>, exn>
type FetchTickerByExternalId = ExternalTickerId -> AsyncResult<Ticker, exn>
type SaveTicker = Ticker -> AsyncResult<int, exn>

type TickerContext =
    { FetchTickers : FetchTickers
      FetchTickerByExternalId : FetchTickerByExternalId
      SaveTicker : SaveTicker }
with
    static member Create sqlConnectionString =
        let fetchTickers _ =
            TickersRepository.getAll sqlConnectionString
            
        let fetchTickerByExternalId =
            TickersRepository.getByExternalId sqlConnectionString
            
        let saveTicker =
            TickersRepository.createTicker sqlConnectionString 
                        
        { FetchTickerByExternalId = fetchTickerByExternalId
          SaveTicker = saveTicker
          FetchTickers = fetchTickers }
