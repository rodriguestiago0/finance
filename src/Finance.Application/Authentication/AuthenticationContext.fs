namespace Finance.Application.Authentication

open Finance.FSharp
open Finance.Model
open Finance.Repository

type Authenticate = Username -> string -> AsyncResult<User, exn>

type AuthenticationContext =
    { Authenticate : Authenticate
      Issuer : string
      Audience : string
      Key : string }
    with
    static member create sqlConnectionString issuer audience key =
        let authenticate username password =
            UserRepository.getUser sqlConnectionString username
            |> AsyncResult.bind (AuthenticationService.authenticate username password)

        { AuthenticationContext.Authenticate = authenticate
          Issuer = issuer
          Audience = audience
          Key = key }

