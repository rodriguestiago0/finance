namespace Finance.Application.Broker

open System
open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository

type FetchBrokers = unit -> AsyncResult<List<Broker>, exn>
type FetchBrokerByExternalId = ExternalBrokerId -> AsyncResult<Broker, exn>
type FetchBrokerByName = string -> AsyncResult<Broker, exn>
type SaveBroker = Broker -> AsyncResult<int, exn>

type BrokerContext =
    { FetchBrokers : FetchBrokers
      FetchBrokerByExternalId : FetchBrokerByExternalId
      FetchBrokerByName : FetchBrokerByName
      SaveBroker : SaveBroker }
with
    static member Create sqlConnectionString =
        let fetchBrokers _ =
            BrokersRepository.getBrokers sqlConnectionString
            
        let fetchBrokerByExternalId =
            BrokersRepository.getByExternalId sqlConnectionString
            
        let fetchBrokerByName =
            BrokersRepository.getByName sqlConnectionString
            
        let saveBroker =
            BrokersRepository.createBroker sqlConnectionString 
                        
        { FetchBrokers = fetchBrokers
          FetchBrokerByExternalId = fetchBrokerByExternalId
          FetchBrokerByName = fetchBrokerByName
          SaveBroker = saveBroker }
