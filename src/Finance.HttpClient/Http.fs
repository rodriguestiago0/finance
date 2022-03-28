namespace Finance.HttpClient

open System
open System.Net.Http
open System.Text
open Finance.FSharp
open Microsoft.FSharp.Control

[<AutoOpen>]
module Http =
    type Method =
    | Get
    | Post
    | Put
    | Patch
    | Delete
    | Head
    | Options
    | Trace
    with
        static member toNetMethod = function
            | Get _ -> HttpMethod.Get
            | Post _ -> HttpMethod.Post
            | Put _ -> HttpMethod.Put
            | Patch _ -> HttpMethod.Patch
            | Delete _ -> HttpMethod.Delete
            | Head _ -> HttpMethod.Head
            | Options _ -> HttpMethod.Options
            | Trace _ -> HttpMethod.Trace

    type ContentType =
        { ``type`` : string
          subtype : string
          charset : option<Encoding>
          boundary : option<string> }
    with
        override x.ToString() =
          String.Concat [
            yield x.``type``
            yield "/"
            yield x.subtype
            match x.charset with
            | None -> ()
            | Some enc -> yield! [ ";"; " charset="; enc.WebName ]
            match x.boundary with
            | None -> ()
            | Some b -> yield! [ ";"; " boundary="; sprintf "\"%s\"" b ]
        ]

    type RequestHeader =
    | Accept of string
    | Authorization of string
    | ContentType of ContentType
    | Custom of string * string

    type RequestBody =
        | BodyString of string
        | BodyRaw of byte []

    [<AutoOpen>]
    module private HttpRequestMessageExtension =
        type HttpRequestMessage
        with
            static member setHeaders (headers : seq<RequestHeader>) (request : HttpRequestMessage) =
                let add (k: string) (v: string) = request.Headers.Add (k, v)
                let addf (k: string) (v: string) = request.Headers.TryAddWithoutValidation(k, v) |> ignore
                let addContent (add: HttpContent -> unit) = if not <| isNull request.Content then add request.Content

                headers
                |> Seq.iter(
                    function
                    | RequestHeader.Accept a -> add "Accept" a
                    | Authorization a -> addf "Authorization" a
                    | ContentType ct -> addContent (fun c ->
                                            c.Headers.Remove("Content-Type") |> ignore
                                            c.Headers.TryAddWithoutValidation("Content-Type", ct.ToString()) |> ignore)
                    | Custom (customName, customValue) -> add customName customValue)
                request

            static member setMethod (method : Method) (request : HttpRequestMessage) =
                request.Method <- Method.toNetMethod method
                request

            static member setRequestUri (path : Option<string>) (queries : seq<string*string>) (request : HttpRequestMessage) =
                let requestUri =
                    let builder = StringBuilder()

                    path
                    |> Option.defaultValue String.Empty
                    |> builder.Append |> ignore

                    match queries with
                    | NotEmptySeq _ -> builder.Append("?")
                    | _ -> builder
                    |> ignore

                    queries
                    |> Seq.fold (fun (builder  :StringBuilder) (key, value) ->
                         builder
                             .Append(key)
                             .Append("=")
                             .Append(value)) builder
                    |> ignore

                    Uri(builder.ToString(), UriKind.Relative)

                request.RequestUri <- requestUri
                request

            static member setBody body (request: HttpRequestMessage) =
                match body with
                | Some (BodyString s) ->
                    request.Content <- new StringContent(s, Encoding.UTF8)
                | Some (BodyRaw s) ->
                    request.Content <- new ByteArrayContent(s)
                | None -> ()
                request

    type ResponseHandler<'a> = HttpResponseMessage -> AsyncResult<'a, exn>

    type HttpRequest<'a> =
        { HttpClient : HttpClient
          BaseAddress : Option<Uri>
          RequestHeaders : seq<RequestHeader>
          Method : Method
          Path : Option<string>
          Queries : seq<string*string>
          Body : Option<RequestBody>
          ResponseHandler : ResponseHandler<'a> }

    type HttpResponseBuilder() =
        let defaultResponseHandler _ =
            Unchecked.defaultof<'a> |> AsyncResult.retn
        member _.Yield _ =
            { HttpRequest.HttpClient =  new HttpClient()
              RequestHeaders = Seq.empty
              BaseAddress = None
              Method = Method.Get
              Path = None
              Queries = Seq.empty
              Body = None
              ResponseHandler = defaultResponseHandler }

        [<CustomOperation("requestHeaders")>]
        member __.RequestHeaders (state, headers) =
            { state with RequestHeaders = headers }

        [<CustomOperation("baseAddress")>]
        member __.BaseAddress(state, baseAddress) =
            { state with BaseAddress = (baseAddress |> Some) }

        [<CustomOperation("method")>]
        member __.Method(state, method) =
            { state with Method = method }

        [<CustomOperation("path")>]
        member __.Path(state, path) =
            { state with Path = (path |> Some) }

        [<CustomOperation("queries")>]
        member __.Queries(state, queries) =
            { state with Queries = queries }

        [<CustomOperation("responseHandler")>]
        member __. ResponseHandler(state, responseHandler) =
            { state with ResponseHandler = responseHandler }

        [<CustomOperation("body")>]
        member __.Body(state, body) =
            { state with Body = (body |> Some) }

        member __.Run(state) =
            let client = state.HttpClient

            match state.BaseAddress with
            | Some b ->
                client.BaseAddress <- b
            | _ -> ()
            let request =
                new HttpRequestMessage()
                |> HttpRequestMessage.setRequestUri state.Path state.Queries
                |> HttpRequestMessage.setMethod state.Method
                |> HttpRequestMessage.setBody state.Body
                |> HttpRequestMessage.setHeaders state.RequestHeaders


            client.SendAsync(request)
            |> Async.AwaitTask
            |> Async.catchResult
            |> AsyncResult.bind state.ResponseHandler

    let httpResponse = HttpResponseBuilder()

    module Response =
        let inline asString (response: HttpResponseMessage) =
            response.Content.ReadAsStringAsync()
            |> AsyncResult.ofTask
        
        let inline toResult (response: HttpResponseMessage) =
            let body = asString response
            if response.IsSuccessStatusCode then
                body
                |> AsyncResult.bind JsonRes.ofString
            else
                body
                |> AsyncResult.map exn
                |> Async.map (function
                    | Ok e -> e |> Error
                      | Error e -> Error e)