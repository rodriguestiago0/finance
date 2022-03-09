namespace Finance.Application.Dividend

open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository

type FetchDividendById = DividendId -> AsyncResult<Dividend, exn>
type FetchDividendByTickerId = TickerId -> AsyncResult<List<Dividend>, exn>
type SaveDividend = Dividend -> AsyncResult<Dividend, exn>

type DividendContext =
    { FetchDividendById : FetchDividendById
      FetchDividendByTickerId : FetchDividendByTickerId
      SaveDividend : SaveDividend }
with
    static member Create sqlConnectionString =
        let fetchDividendById =
            DividendsRepository.getById sqlConnectionString
            
        let fetchDividendByTickerId =
            DividendsRepository.getDividendsByTickerId sqlConnectionString
            
        let saveTicker =
            DividendsRepository.createDividend sqlConnectionString
                        
        { FetchDividendById = fetchDividendById
          FetchDividendByTickerId = fetchDividendByTickerId
          SaveDividend = saveTicker }
