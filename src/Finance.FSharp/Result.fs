namespace Finance.FSharp

open Microsoft.FSharp.Core
open Fleece

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
        
    let tee (f: 'a->unit) (res: Result<'a, _>) : Result<'a, _> =
        match res with
        | Ok x -> f x
        | Error _ -> ()
        res
        
    let teeError (f: 'b->unit) (res: Result<'a, 'b>) : Result<'a, 'b> =
        match res with
        | Ok _ -> ()
        | Error e -> f e
        res

    let decodeError (f: 'b -> Result<'a, DecodeError>) (res: Result<'a, 'b>) : Result<'a, DecodeError> =
        match res with
        | Ok x -> x |> Ok
        | Error e -> f e