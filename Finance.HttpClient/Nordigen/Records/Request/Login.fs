namespace Finance.HttpClient.Nordigen.Records.Request

open System

type Login =
    { SecretId : Guid
      SecretKey : Guid } 