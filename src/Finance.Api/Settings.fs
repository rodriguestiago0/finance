namespace Finance.Api

open System

module Settings =
    [<CLIMutable>]
    type Jwt =
        { Issuer : string
          Audience : string
          Key : string }

    [<CLIMutable>]
    type Settings =
        { SqlConnectionString : string
          SecretId : Guid
          SecretKey : string
          EncryptionKey : string
          Jwt : Jwt }