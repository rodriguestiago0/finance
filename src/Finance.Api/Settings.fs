namespace Finance.Api

open System

module Settings =
    [<CLIMutable>]
    type Settings =
        { SqlConnectionString : string
          SecretId : Guid
          SecretKey : string
          EncryptionKey : string }