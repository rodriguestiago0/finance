namespace Finance.HttpClient.Nordigen.Records.Response

open System

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
      AccountSelection : bool
    }

