namespace Finance.Service

open System

module Settings =
    [<CLIMutable>]
    type Settings = {
        SqlConnectionString: string
        ClientId: Guid
        SecretId: string
    }