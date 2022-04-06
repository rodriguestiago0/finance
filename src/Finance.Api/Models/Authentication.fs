namespace Finance.Api.Models

open System

type AuthenticationDto =
    { Token : string
      ExpirationDate : Option<DateTime> }

