namespace Finance.HttpClient.Model.Response

open System
open FSharpPlus
open Fleece

module Nordigen =
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
        { Transactions : Transactions }

    type RequisitionStatus =
        | CR
        | EX
        | GA
        | GC
        | LN
        | RJ
        | SA
        | SU
        | UA

    type Requisition =
        { Id : Guid
          CreatedAt : DateTimeOffset
          Redirect : string
          Status: RequisitionStatus
          InstitutionId : string
          Agreement : string
          Reference : string
          Accounts : Guid[]
          UserLanguage  :string
          Link : Uri
          AccountSelection : bool }

    type EndUserAgreement =
        { Id : Guid
          CreatedAt : DateTimeOffset
          MaxHistoricalDays : int
          AccessValidForDays : int
          AccessScope: string[]
          InstitutionId : string }

    type Authorization = {
        Access : string
        AccessExpires : int
        Refresh : string
        RefreshExpires : int }
    with
    static member OfJson json =
            match json with
            | JObject o ->
                monad {
                    let! access = o .@ "access"
                    let! accessExpires = o .@ "access_expires"
                    let! refresh = o .@ "refresh"
                    let! refreshExpires = o .@ "refresh_expires"

                    return
                        { Access = access
                          AccessExpires = accessExpires
                          Refresh = refresh
                          RefreshExpires = refreshExpires }
                }
            | x -> Decode.Fail.objExpected x

