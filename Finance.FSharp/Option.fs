namespace Finance.FSharp

[<RequireQualifiedAccess>]
module Option =
    let apply f a =
        Option.bind (fun f' -> Option.bind (f' >> Some ) a) f

    let lift2 f a b =
        apply (Option.map f a) b

    module Operators =
        let (<!>) = Option.map
        let (<*>) = apply