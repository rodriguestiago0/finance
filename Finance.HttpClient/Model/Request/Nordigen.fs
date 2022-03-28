namespace Finance.HttpClient.Model.Request

open System
open Fleece

module Nordigen =
    type Login =
        { SecretId : Guid
          SecretKey : string }
        static member ToJson (value : Login) =
            jobj [ "secret_id" .= value.SecretId
                   "secret_key" .= value.SecretKey ]

    type Refresh =
        { Refresh : string }
        static member ToJson (value : Refresh) =
            jobj [ "refresh" .= value.Refresh ]

    type Requisition =
        { Redirect : Uri
          InstitutionId : string
          Reference : string
          Agreement : Guid
          UserLanguage : string }

