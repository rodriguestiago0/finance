namespace Finance.Api

open System
open System.Threading.Tasks
open Microsoft.AspNetCore.Http
open Finance.FSharp

module ApiBinder =

    type public FormFiles =
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
                ValueTask.FromResult(f)
            | Error _ ->
                ValueTask.FromResult(Unchecked.defaultof<FormFiles>)

    type public Pagination =
        { Page : Option<int>
          PageSize : Option<int> }
        with
        static member BindAsync (context : HttpContext) =
            let pageSizeKey = "pageSize";
            let pageKey = "page";

            let page =
                match context.Request.Query.ContainsKey pageKey, Int32.TryParse context.Request.Query[pageKey] with
                | true, (true, p) -> Ok (Some p)
                | false, _ -> Ok(None)
                | true, (false, _) -> "Cannot parse Page" |> exn |> Error

            let pageSize =
                match context.Request.Query.ContainsKey pageSizeKey, Int32.TryParse context.Request.Query[pageSizeKey] with
                | true, (true, p) -> Ok (Some p)
                | false, _ -> Ok(None)
                | true, (false, _) -> "Cannot parse Page Size" |> exn |> Error

            match page, pageSize with
            | Ok p, Ok ps ->
                let result =
                    { Pagination.Page = p
                      PageSize = ps }
                ValueTask.FromResult(result)
            | _ ->
                failwith ""
