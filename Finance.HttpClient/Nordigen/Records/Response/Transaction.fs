namespace Finance.HttpClient.Nordigen.Records.Response

open System

type Currency =
    | EUR
    | USD

type TransactionAmount =
    { Amount : decimal
      Currency : Currency }

type Transaction =
    { BookingDate : DateOnly
      RemittanceInformationUnstructured : string
      TransactionAmount : TransactionAmount
      TransactionId : string
      ValueDate : DateOnly }

type Transactions =
    { Transactions : Transactions}


