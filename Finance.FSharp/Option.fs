namespace Finance.FSharp

[<RequireQualifiedAccess>]
module Option =
    let inline ofObjSafe x =
        match box x with
        | null -> None
        | e -> Some (unbox e)

    let ofString s =
        match s with
        | NotNullOrEmpty s -> Some s
        | _ -> None

    let apply f a =
        Option.bind (fun f' -> Option.bind (f' >> Some ) a) f

    let lift2 f a b =
        apply (Option.map f a) b

    module Operators =
        let (<!>) = Option.map
        let (<*>) = apply