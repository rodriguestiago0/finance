namespace Finance.Application.Broker

open Finance.FSharp
open Finance.Model.Investment
open Finance.Repository

type FetchBrokers = unit -> AsyncResult<List<Broker>, exn>
type FetchBrokerByExternalId = ExternalBrokerId -> AsyncResult<Broker, exn>
type FetchBrokerByName = string -> AsyncResult<Broker, exn>
type FetchTransactionByExternalBrokerId = ExternalBrokerId -> AsyncResult<List<Transaction>, exn>
type SaveBroker = Broker -> AsyncResult<Broker, exn>

type BrokerContext =
    { FetchBrokers : FetchBrokers
      FetchBrokerByExternalId : FetchBrokerByExternalId
      FetchBrokerByName : FetchBrokerByName
      FetchTransactionByExternalBrokerId : FetchTransactionByExternalBrokerId
      SaveBroker : SaveBroker }
with
    static member Create sqlConnectionString =
        let fetchBrokers _ =
            BrokersRepository.getBrokers sqlConnectionString
            
        let fetchBrokerByExternalId =
            BrokersRepository.getByExternalId sqlConnectionString
            
        let fetchBrokerByName =
            BrokersRepository.getByName sqlConnectionString
        
        let fetchTransactionByExternalBrokerId =
            TransactionsRepository.getByBrokerExternalId sqlConnectionString
            
        let saveBroker =
            BrokersRepository.createBroker sqlConnectionString 
                        
        { FetchBrokers = fetchBrokers
          FetchBrokerByExternalId = fetchBrokerByExternalId
          FetchBrokerByName = fetchBrokerByName
          FetchTransactionByExternalBrokerId = fetchTransactionByExternalBrokerId
          SaveBroker = saveBroker }
