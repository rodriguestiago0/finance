namespace Finance.FSharp

[<RequireQualifiedAccess>]
module Async =
    let inline retn x = async { return x }
    let inline bind f a = async.Bind(a, f)
    let inline map f = bind (f >> retn)

    let sequenceOption (x:option<Async<'t>>) = async {
        match x with
        | Some a ->
            let! a' = a
            return Some (a')
        | None ->
            return None
    }

    let catchResult (comp: Async<'a>) =
        comp
        |> Async.Catch
        |> map Result.ofChoice