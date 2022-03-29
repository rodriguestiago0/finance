namespace Finance.HttpClient.Model.Request

open System
open Fleece
open Finance.HttpClient.Model.Nordigen

module Nordigen =
    type LoginRequest =
        { SecretId : Guid
          SecretKey : string }
        with
        static member ToJson (value : LoginRequest) =
            jobj [ "secret_id" .= value.SecretId
                   "secret_key" .= value.SecretKey ]

    type RefreshRequest =
        { Refresh : string }
        with
        static member ToJson (value : RefreshRequest) =
            jobj [ "refresh" .= value.Refresh ]

    type EndUserAgreementRequest =
        { InstitutionId : string
          MaxHistoricalDays : Option<int>
          AccessValidForDays : Option<int>
          AccessScope : Option<AccessScope[]> }
        with
        static member ToJson (value : EndUserAgreementRequest) =
            jobj [ "institution_id" .= value.InstitutionId
                   "max_historical_days" .= value.MaxHistoricalDays
                   "access_valid_for_days" .= value.AccessValidForDays
                   "access_scope" .= (value.AccessScope |> Option.map (Array.map AccessScope.toString)) ]

    type RequisitionRequest =
        { Redirect : Uri
          InstitutionId : string
          Reference : string
          Agreement : Option<Guid>
          UserLanguage : string }
        with
        static member ToJson (value : RequisitionRequest) =
            jobj [ "redirect" .= value.Redirect.ToString()
                   "institution_id" .= value.InstitutionId
                   "reference" .= value.Reference
                   "agreement" .= value.Agreement
                   "user_language" .= value.UserLanguage ]

