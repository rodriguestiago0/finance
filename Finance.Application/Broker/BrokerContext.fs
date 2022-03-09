namespace Finance.Application.Broker

open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository


type FetchBrokers = unit -> AsyncResult<List<Broker>, exn>
type FetchBrokerById = BrokerId -> AsyncResult<Broker, exn>
type FetchBrokerByName = string -> AsyncResult<Broker, exn>
type SaveBroker = Broker -> AsyncResult<Broker, exn>

type BrokerContext =
    { FetchBrokers : FetchBrokers
      FetchBrokerById : FetchBrokerById
      FetchBrokerByName : FetchBrokerByName
      SaveBroker : SaveBroker }
with
    static member Create sqlConnectionString =
        let fetchBrokers _ =
            BrokersRepository.getBrokers sqlConnectionString
            
        let fetchBrokerById =
            BrokersRepository.getById sqlConnectionString

        let fetchBrokerByName =
            BrokersRepository.getByName sqlConnectionString

        let saveBroker =
            BrokersRepository.createBroker sqlConnectionString

        { FetchBrokers = fetchBrokers
          FetchBrokerById = fetchBrokerById
          FetchBrokerByName = fetchBrokerByName
          SaveBroker = saveBroker }
