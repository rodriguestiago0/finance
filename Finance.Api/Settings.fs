namespace Finance.Api

module Settings =
    [<CLIMutable>]
    type Settings = {
        SqlConnectionString: string
    }