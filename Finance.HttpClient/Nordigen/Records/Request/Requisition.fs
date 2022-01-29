namespace Finance.HttpClient.Nordigen.Records.Request

open System

type Requisition =
    { Redirect : Uri
      InstitutionId : string
      Reference : string
      Agreement : Guid
      UserLanguage : string
}

