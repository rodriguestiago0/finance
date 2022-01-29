namespace Finance.HttpClient.Nordigen.Records.Response

open System

type EndUserAgreement =
    { Id : Guid
      CreatedAt : DateTimeOffset
      MaxHistoricalDays : int
      AccessValidForDays : int
      AccessScope: string[]
      InstitutionId : string }

