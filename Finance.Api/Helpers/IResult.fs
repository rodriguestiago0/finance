namespace Finance.Api.Helpers

open Finance.FSharp
open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module IResults = 
    let private ofResult (res: Result<IResult, IResult>) =
        match res with
        | Ok o -> o
        | Error e -> e
        
    let private ofAsyncResult (res: AsyncResult<IResult, IResult>) =
        res
        |> Async.map ofResult
        
    let created (buildUri : 'a -> string) (result : AsyncResult<'a, exn>) =
        result
        |> AsyncResult.map (fun res -> Results.Created(buildUri res, res))
        |> AsyncResult.mapError (fun e -> Results.BadRequest(e))
        |> ofAsyncResult
        
    let ok (result : AsyncResult<'a, exn>) =
        result
        |> AsyncResult.map (fun res -> Results.Ok(res))
        |> AsyncResult.mapError (fun e -> Results.BadRequest(e))
        |> ofAsyncResult