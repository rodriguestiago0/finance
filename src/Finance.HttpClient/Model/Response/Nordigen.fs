namespace Finance.HttpClient.Model.Response

open System
open FSharpPlus
open Fleece
open Finance.FSharp
open Finance.HttpClient.Model.Nordigen

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
        with
        static member ofString value =
            match value with
            | "CR" -> Ok CR
            | "EX" -> Ok EX
            | "GA" -> Ok GA
            | "GC" -> Ok GC
            | "LN" -> Ok LN
            | "RJ" -> Ok RJ
            | "SA" -> Ok SA
            | "SU" -> Ok SU
            | "UA" -> Ok UA
            | _ -> $"Invalid RequisitionStatus - {value}" |> exn |> Error

    type Requisition =
        { Id : Guid
          CreatedAt : DateTimeOffset
          Redirect : Uri
          Status: RequisitionStatus
          InstitutionId : string
          Agreement : string
          Reference : string
          Accounts : Guid[]
          UserLanguage  :string
          Link : Uri }
        with
        static member OfJson json =
                match json with
                | JObject o ->
                    monad {
                        let! id = o .@ "id"
                        let! createdAt = o .@ "created"
                        let! redirect = o .@ "redirect"
                        let! status = o .@ "status"
                        let! institutionId = o .@ "institution_id"
                        let! agreement = o .@ "agreement"
                        let! reference = o .@ "reference"
                        let! accounts = o .@ "accounts"
                        let! userLanguage = o .@ "user_language"
                        let! link = o .@ "link"

                        let! status =
                            RequisitionStatus.ofString status
                            |> Result.decodeError (fun x -> Decode.Fail.parseError x "Invalid Access Scope")

                        return
                            { Id = id
                              CreatedAt = createdAt
                              Redirect = Uri(redirect)
                              Status = status
                              InstitutionId = institutionId
                              Agreement  = agreement
                              Reference = reference
                              Accounts = accounts
                              UserLanguage = userLanguage
                              Link = Uri(link) }
                    }
                | x -> Decode.Fail.objExpected x

    type EndUserAgreement =
        { Id : Guid
          CreatedAt : DateTimeOffset
          MaxHistoricalDays : int
          AccessValidForDays : int
          AccessScope: AccessScope[]
          InstitutionId : string }
        with
        static member OfJson json =
                match json with
                | JObject o ->
                    monad {
                        let! id = o .@ "id"
                        let! createdAt = o .@ "created"
                        let! maxHistoricalDays = o .@ "max_historical_days"
                        let! accessValidForDays = o .@ "access_valid_for_days"
                        let! accessScope = o .@ "access_scope"
                        let! institutionId = o .@ "institution_id"
                        let! accessScope =
                            accessScope
                            |> Array.map AccessScope.ofString
                            |> Result.sequence
                            |> Result.map Array.ofSeq
                            |> Result.decodeError (fun x -> Decode.Fail.parseError x "Invalid Access Scope")

                        return
                            { Id = id
                              CreatedAt = createdAt
                              MaxHistoricalDays = maxHistoricalDays
                              AccessValidForDays = accessValidForDays
                              AccessScope = accessScope
                              InstitutionId  = institutionId }
                    }
                | x -> Decode.Fail.objExpected x

    type Institution = {
            Id : string
            Name : string
            BIC : string
            TransactionTotalDays : int
            Countries : string[]
            Logo : string }
        with
        static member OfJson json =
                match json with
                | JObject o ->
                    monad {
                        let! id = o .@ "id"
                        let! name = o .@ "name"
                        let! bic = o .@ "bic"
                        let! transactionTotalDays = o .@ "transaction_total_days"
                        let! countries = o .@ "countries"
                        let! logo = o .@ "logo"

                        return
                            { Id = id
                              Name = name
                              BIC = bic
                              TransactionTotalDays = transactionTotalDays
                              Countries = countries
                              Logo  = logo }
                    }
                | x -> Decode.Fail.objExpected x

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