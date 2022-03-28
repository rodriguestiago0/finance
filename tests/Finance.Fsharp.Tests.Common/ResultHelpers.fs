namespace Finance.Fsharp.Tests.Common

[<AutoOpen>]
module ResultHelper =
    let isOk res =
        match res with
        | Ok _ -> true
        | Error _ -> false

    let getOk res =
        match res with
        | Ok o -> o
        | Error _ -> failwith "Is not Ok"

    let getError res =
        match res with
        | Ok _ -> failwith "Is not Error"
        | Error e -> e