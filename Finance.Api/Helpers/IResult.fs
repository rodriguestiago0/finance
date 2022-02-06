namespace Finance.Api.Helpers

open Finance.FSharp
open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module IResults = 
    let inline ofResult (res: Result<IResult, IResult>) =
        match res with
        | Ok o -> o
        | Error e -> e
        
    let inline ofAsyncResult (res: AsyncResult<IResult, IResult>) =
        res
        |> Async.map ofResult