namespace Finance.HttpClient.Model

module Nordigen =
    type AccessScope =
    | Balances
    | Details
    | Transactions
        with
        static member toString value =
            match value with
            | Balances -> "balances"
            | Details -> "details"
            | Transactions -> "transactions"

        static member ofString value =
            match value with
            | "balances" -> Ok Balances
            | "details" -> Ok Details
            | "transactions" -> Ok Transactions
            | _ -> $"Invalid AccessScope %s{value}" |> exn |> Error
