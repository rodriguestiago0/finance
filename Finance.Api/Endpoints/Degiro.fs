namespace Finance.Api.Endpoints

open System
open System.IO
open System.Threading.Tasks
open Finance.Application.Degiro
open Finance.FSharp
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open FSharp.Core

[<RequireQualifiedAccess>]
module Degiro =
    let uploadFile (degiroContext : DegiroContext) (request : HttpRequest) : Task<IResult> =
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
                
                let x =
                    form
                    |> AsyncResult.bind validateForm
                    |> AsyncResult.map(fun form -> form.OpenReadStream())
                    |> AsyncResult.map (Degiro.importCSV degiroContext)
                    |> AsyncResult.map (fun _ -> Results.Ok(None))
                    |> AsyncResult.mapError (fun _ -> Results.BadRequest(None))
                //TODO: fix this
                return Results.Ok(None)
        }

    
    let registerEndpoint (app : WebApplication) (degiroContext : DegiroContext) =
        app.MapPost("/degiro/transaction", Func<HttpRequest,Task<IResult>>(uploadFile degiroContext))
            .Accepts<IFormFile>("multipart/form-data")
            .WithName("Upload Transaction")
            .WithTags("Degiro")



