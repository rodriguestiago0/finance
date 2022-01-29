namespace Finance.FSharp

[<RequireQualifiedAccess>]
module Result =
    let ofChoice : (Choice<'a,'b> -> Result<'a, 'b>)= function
        | Choice1Of2 x -> Result.Ok x
        | Choice2Of2 e -> Result.Error e