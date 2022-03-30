namespace Finance.Api

open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Finance.FSharp

module ApiBinder =

    type FormFiles =
        { Items : seq<IFormFile> }
        with
        static member BindAsync (context : HttpContext) =
            let form =
                context.Request.ReadFormAsync()
                |> AsyncResult.ofTask
                |> AsyncResult.bind (fun formCollection ->
                    match formCollection.Files.Count with
                    | 0 -> sprintf "No Files Found" |> exn |> Error
                    | _ -> Ok formCollection.Files
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

            let result =
                form
                |> AsyncResult.bind validateForms
                |> AsyncResult.map (fun items -> { FormFiles.Items = items })
                |> Async.RunSynchronously

            match result with
            | Ok f ->
                ValueTask.FromResult<FormFiles>(f)
            | Error e -> failwith e.Message
