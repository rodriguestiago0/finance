namespace Finance.Api.Endpoints

open System
open System.IO
open System.Threading.Tasks
open Finance.FSharp
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open FSharp.Core

[<RequireQualifiedAccess>]
module Degiro =
    let uploadFile (request : HttpRequest) : Task<IResult> =
        task{
            if not request.HasFormContentType then
                return Results.BadRequest()
            else                                            
                let form =
                    request.ReadFormAsync()
                    |> AsyncResult.ofTask
                    |> AsyncResult.map (fun formCollection -> formCollection.Files["File"])
                
                let validateForm (form : IFormFile) =
                    form
                    |> Option.ofObj
                    |> Option.map(fun formFile ->
                        if formFile.Length = 0 then
                            "Empty file" |> exn |> AsyncResult.error
                        else
                            AsyncResult.retn formFile )
                    |> Option.defaultValue ("No file found" |> exn |> AsyncResult.error)
                
                form
                |> AsyncResult.bind validateForm
                |> AsyncResult.map(fun form -> form.OpenReadStream())
                |> ignore
                
                return Results.Ok(None)
        }

    
    let registerEndpoint (app : WebApplication) =
        app.MapPost("/degiro/transaction", Func<HttpRequest , Task<IResult>>(uploadFile))
            .Accepts<IFormFile>("multipart/form-data")
            .WithName("Upload Transaction")
            .WithTags("Degiro")



