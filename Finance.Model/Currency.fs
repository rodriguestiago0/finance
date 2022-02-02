namespace Finance.Model

type Currency =
    | EUR
    | USD
    with
    static member fromInt value =
        match value with
        | 1 -> Ok EUR
        | 2 -> Ok USD
        | _ -> sprintf "Invalid Currency - %O" value |> exn |> Result.Error
        
    static member toInt =
        function
        | EUR -> 1 
        | USD -> 2
