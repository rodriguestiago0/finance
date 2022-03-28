namespace Finance.HttpClient.Client

open System
open System.Net.Http
open System.Runtime.Caching
open Fleece.SystemTextJson
open Finance.FSharp
open Finance.HttpClient
open Finance.HttpClient.Http.Response
open Finance.HttpClient.Model.Request.Nordigen
open Finance.HttpClient.Model.Response.Nordigen
open Microsoft.FSharp.Core

[<RequireQualifiedAccess>]
module NordigenClient =
    let [<Literal>] BaseUrl = "https://ob.nordigen.com/api/v2/"
    let [<Literal>] AuthKey = "Auth"

    type Credential = {
        Access : string
        AccessExpiresAt : DateTimeOffset
        Refresh : string
        RefreshExpiresAt : DateTimeOffset }

    let mutable private Authentication =
        { Credential.Access = String.Empty
          AccessExpiresAt =  DateTimeOffset.MinValue
          Refresh = String.Empty
          RefreshExpiresAt = DateTimeOffset.MinValue } |> AsyncResult.retn
    let private lockObj = obj()

    let private loginAction (login : Login) =
        let headers =
            let contentType =
                { ContentType.``type`` = "application"
                  subtype = "json"
                  charset = None
                  boundary = None }
            seq {
                yield "application/json" |> RequestHeader.Accept
                yield contentType |> RequestHeader.ContentType }

        let requestBody =
            (login
             |> toJson
             |> string
             |> RequestBody.BodyString)

        let handler (response : HttpResponseMessage) =
            let processResponse : AsyncResult<Authorization, exn> =
                response
                |> toResult
            processResponse
            |> AsyncResult.map(fun auth ->
                let accessExpiresAt = auth.AccessExpires - 60
                let refreshExpiresAt = auth.RefreshExpires - 60
                let now = DateTimeOffset.UtcNow
                { Credential.Access = auth.Access
                  AccessExpiresAt = now.AddSeconds(accessExpiresAt)
                  Refresh = auth.Refresh
                  RefreshExpiresAt = now.AddSeconds(refreshExpiresAt) })

        httpResponse {
            baseAddress (Uri(BaseUrl))
            method Method.Post
            path "token/new/"
            requestHeaders headers
            body requestBody
            responseHandler handler
        }

    let private refreshAction (login : Login) =
        let headers =
            let contentType =
                { ContentType.``type`` = "application"
                  subtype = "json"
                  charset = None
                  boundary = None }
            seq {
                yield "application/json" |> RequestHeader.Accept
                yield contentType |> RequestHeader.ContentType }

        let requestBody =
            (login
             |> toJson
             |> string
             |> RequestBody.BodyString)

        let handler (response : HttpResponseMessage) =
            let processResponse : AsyncResult<Authorization, exn> =
                response
                |> toResult
            processResponse
            |> AsyncResult.map(fun auth ->
                let accessExpiresAt = auth.AccessExpires - 60
                let refreshExpiresAt = auth.RefreshExpires - 60
                let now = DateTimeOffset.UtcNow
                { Credential.Access = auth.Access
                  AccessExpiresAt = now.AddSeconds(accessExpiresAt)
                  Refresh = auth.Refresh
                  RefreshExpiresAt = now.AddSeconds(refreshExpiresAt) })

        httpResponse {
            baseAddress (Uri(BaseUrl))
            method Method.Post
            path "token/refresh/"
            requestHeaders headers
            body requestBody
            responseHandler handler
        }

    let (|IsValid|_|) (credential : Credential) =
        Some credential
    let (|RefreshToken|_|) (credential : Credential) =
        Some ()
    let (|ExpiredRefresh|_|) (credential : Credential) =
        Some ()

    type Credential with
        static member BearerToken login =
            let getOrUpdateAuthentication _ =
                let validateCredentials (credential : Credential) =
                    match credential with
                    | IsValid s ->
                        s |> AsyncResult.retn
                    | RefreshToken _ ->
                        refreshAction login
                    | ExpiredRefresh _ ->
                        loginAction login

                let currentAuthentication =
                    Authentication
                    |> AsyncResult.bind validateCredentials
                Authentication <- currentAuthentication
                Authentication
            lock lockObj getOrUpdateAuthentication
            |> AsyncResult.map(fun a -> a.Access)

    let login (login : Login) =
        Credential.BearerToken login
    