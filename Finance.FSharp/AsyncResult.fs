namespace Finance.FSharp

open System.Threading.Tasks

type AsyncResult<'a, 'b> = Async<Result<'a, 'b>>

[<RequireQualifiedAccess>]
module AsyncResult =
    let inline ofChoice res =
        Async.map Result.ofChoice res
        
    let retn x = async {
        return Result.Ok x
    }

    let error (x : exn) =
        x
        |> Result.Error
        |> Async.retn
        
    let bind (f: 'a->AsyncResult<'b,'c>) (res:AsyncResult<'a, 'c>) : AsyncResult<'b, 'c> = async {
        match! res with
        | Ok res' ->
            return! (f res')
        | Error e ->
            return Error e
    }
    
    let map (f: 'a -> 'b) (res:AsyncResult<'a, 'c>) : AsyncResult<'b, 'c> =
        bind (f >> retn) res

    let apply f a =
        bind (fun f' -> bind (f' >> retn) a) f
        
    let lift2 f a b =
        apply (map f a) b
        
    let bindError f res = async {
        match! res with
        | Ok res' ->
            return Ok res'
        | Error e ->
            let! e' = f e
            return Error e'
    }

    let orElse (otherCase: AsyncResult<'a, 'b>) (res: AsyncResult<'a, 'b>) = async {
        match! res with
        | Ok res' ->
            return Ok res'
        | Error _ ->
            return! otherCase
    }

    let mapError f res = async {
        match! res with
        | Ok x -> return Ok x
        | Error e -> return Error <| f e
    }
        
    let bimap f g res =
        res |> map f |> mapError g
        
    let ofPlainTask (task: Task) : AsyncResult<unit,_> =
       task
        |> Async.AwaitTask
        |> Async.Catch
        |> ofChoice
        
    let ofTask (task: Task<_>) : AsyncResult<_,_> =
        task
        |> Async.AwaitTask
        |> Async.Catch
        |> ofChoice

    let traverse f xs =
        let folder x xs =
            bind (fun h ->
                    bind (fun t -> seq { yield h; yield! t } |> retn ) xs) (f x) 
        Seq.foldBack folder xs (retn Seq.empty)
        
    let sequence xs = traverse id xs
    
    module ComputationExpressions =
        type AsyncResultBuilder() =
            member _.Return(x) = Async.retn (Ok x)

            member _.Bind(m, f) = bind f m

        let asyncResult = AsyncResultBuilder()

    module Operators =
        let (<!>) = map
        let (<*>) = apply

