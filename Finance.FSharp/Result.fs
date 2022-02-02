namespace Finance.FSharp

[<RequireQualifiedAccess>]
module Result =
    let traverse f xs =
        let folder x xs =
            Result.bind (fun h ->
                    Result.bind (fun t -> seq { yield h; yield! t } |> Result.Ok ) xs) (f x) 
        Seq.foldBack folder xs (Result.Ok Seq.empty)
        
    let sequence xs = traverse id xs
    
    let ofChoice : (Choice<'a,'b> -> Result<'a, 'b>)= function
        | Choice1Of2 x -> Result.Ok x
        | Choice2Of2 e -> Result.Error e
        
    let inline ofOption errorCase x =
        match x with
        | Some s -> Result.Ok s
        | None -> Result.Error errorCase