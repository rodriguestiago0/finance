namespace Finance.Api.Endpoints

open System
open System.Threading.Tasks
open FSharp.Core
open Finance.Api.Helpers
open Finance.Application.BankTransaction
open Finance.Application.Degiro
open Finance.Application.Transaction
open Finance.FSharp
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
module Bank =

    let private getBanks (bankContext : ApiBankTransactionContext) (countryISO : Option<string>) =
        task{
            return!
                bankContext.FetchBanks countryISO
                //|> AsyncResult.map (List.map TransactionDto.ofDomain)
                |> IResults.ok
        }
        
    let private uploadFile (degiroContext : DegiroContext) (externalBrokerId : Guid) (request : HttpRequest) : Task<IResult> =
        task{
            if not request.HasFormContentType then
                return Results.BadRequest()
            else
                let form =
                    request.ReadFormAsync()
                    |> AsyncResult.ofTask
                    |> AsyncResult.bind (fun formCollection ->
                        match formCollection.Files.Count with
                        | 1 -> Ok formCollection.Files
                        | _ -> sprintf "No Files Found" |> exn |> Error
                        |> Async.retn)

                let validateForms (forms : IFormFileCollection) =
                    let validateForm (form : IFormFile) =
                        if form.Length = 0 then
                            "Empty file" |> exn |> AsyncResult.error
                        else
                            AsyncResult.retn form

                    forms
                    |> Seq.map validateForm
                    |> AsyncResult.sequence

                let processForms (forms : seq<IFormFile>) =
                    forms
                    |> Seq.map(fun form ->
                        form.OpenReadStream()
                        |> Degiro.importCSV degiroContext externalBrokerId)
                    |> AsyncResult.sequence

                return!
                    form
                    |> AsyncResult.bind validateForms
                    |> AsyncResult.bind processForms
                    |> IResults.ok
        }
    
    let registerEndpoint (app : WebApplication) (bankContext : ApiBankTransactionContext) =

        app.MapGet("/api/banks", Func<string, Task<IResult>>(fun countryISO -> getBanks bankContext (countryISO |> Option.ofString)))
           .WithTags("Banks") |> ignore