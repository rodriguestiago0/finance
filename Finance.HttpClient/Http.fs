﻿namespace Finance.HttpClient

open System
open System.Net.Http
open System.Text
open Finance.FSharp
open Fleece.SystemTextJson

[<RequireQualifiedAccess>]
module Http =
    
    type RequestMsg = HttpRequestMessage
    type ResponseMsg = HttpResponseMessage
    
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
            | Get -> HttpMethod.Get
            | Post -> HttpMethod.Post
            | Put -> HttpMethod.Put
            | Patch -> HttpMethod.Patch
            | Delete -> HttpMethod.Delete
            | Head -> HttpMethod.Head
            | Options -> HttpMethod.Options
            | Trace -> HttpMethod.Trace
    
    [<AutoOpen>]
    module Request =
        let fromPath (path: string) =
            let request = new RequestMsg()
            request.RequestUri <- Uri(path, UriKind.Relative)
            request

        let inline withFullUri uri (request: RequestMsg) =
            request.RequestUri <- uri
            request
            
        let inline withMethod method (request: RequestMsg) =
            request.Method <- Method.toNetMethod method
            request
          
        let inline withJsonBody body (request: RequestMsg) =
            let json = toJson body |> string
            request.Content <- new StringContent(json, Encoding.UTF8, "application/json")
            request
            
        let inline withJson body (request: RequestMsg) =
            request.Content <- new StringContent(body, Encoding.UTF8, "application/json")
            request

    
    module Response =
        let inline toJson (response: ResponseMsg) =
            response.Content.ReadAsStringAsync()
            |> AsyncResult.ofTask
            |> AsyncResult.bind JsonRes.ofString
            
        let inline asString (response: ResponseMsg) =
            response.Content.ReadAsStringAsync()
            |> AsyncResult.ofTask
        
        let inline toResult (response: ResponseMsg) =
            let body = asString response
            if response.IsSuccessStatusCode then
                body
                |> AsyncResult.bind JsonRes.ofString
            else
                body
                |> AsyncResult.map exn
                |> Async.map (function | Ok e -> e :> exn |> Error | Error e -> Error e)

    let inline toRequest x =
        (^T : (static member ToRequest: ^T -> Result<RequestMsg, exn>) x)
      