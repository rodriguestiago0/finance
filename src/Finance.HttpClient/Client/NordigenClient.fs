namespace Finance.HttpClient.Client

open System
open System.Net.Http
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

    let private loginAction (login : LoginRequest) =
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

    let private refreshAction (login : LoginRequest) =
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
            login
            |> toJson
            |> string
            |> RequestBody.BodyString

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

    let (|ExpiredToken|_|) (credential : Credential) =
        if credential.RefreshExpiresAt > DateTimeOffset.UtcNow then
            Some()
        else None

    let (|ExpiredRefresh|_|) (credential : Credential) =
        if credential.AccessExpiresAt > DateTimeOffset.UtcNow then
            Some()
        else None

    type Credential with
        static member BearerToken login =
            let getOrUpdateAuthentication _ =
                let validateCredentials (credential : Credential) =
                    match credential with
                    | IsValid s ->
                        s |> AsyncResult.retn
                    | ExpiredRefresh _ ->
                        loginAction login
                    | ExpiredToken _ ->
                        refreshAction login
                    | _ -> "Invalid Credential" |> exn |> AsyncResult.error

                let currentAuthentication =
                    Authentication
                    |> AsyncResult.bind validateCredentials
                Authentication <- currentAuthentication
                Authentication
            lock lockObj getOrUpdateAuthentication
            |> AsyncResult.map(fun a -> a.Access)


    let listBanks (login : LoginRequest) (country : Option<string>) =
        let mkHeaders bearerToken  =
            let contentType =
                { ContentType.``type`` = "application"
                  subtype = "json"
                  charset = None
                  boundary = None }
            seq {
                yield "application/json" |> RequestHeader.Accept
                yield bearerToken |> RequestHeader.Authorization
                yield contentType |> RequestHeader.ContentType }

        let handler (response : HttpResponseMessage) =
            let processResponse : AsyncResult<Institution[], exn> =
                response
                |> toResult
            processResponse

        let query =
            match country with
            | Some s ->
                ("country", s) |> Seq.singleton
            | None -> Seq.empty

        let headers =
            Credential.BearerToken login
            |> AsyncResult.map mkHeaders

        let mk headers =
            httpResponse {
                baseAddress (Uri(BaseUrl))
                method Method.Post
                path "institutions/?country=gb"
                queries query
                requestHeaders headers
                responseHandler handler
            }

        headers
        |> AsyncResult.bind mk

    let createEndUserAgreement (login : LoginRequest) (endUserAgreement : EndUserAgreementRequest) =
        let mkHeaders bearerToken  =
            let contentType =
                { ContentType.``type`` = "application"
                  subtype = "json"
                  charset = None
                  boundary = None }
            seq {
                yield "application/json" |> RequestHeader.Accept
                yield bearerToken |> RequestHeader.Authorization
                yield contentType |> RequestHeader.ContentType }

        let handler (response : HttpResponseMessage) =
            let processResponse : AsyncResult<EndUserAgreement, exn> =
                response
                |> toResult
            processResponse

        let headers =
            Credential.BearerToken login
            |> AsyncResult.map mkHeaders

        let requestBody =
            endUserAgreement
            |> toJson
            |> string
            |> RequestBody.BodyString

        let mk headers =
            httpResponse {
                baseAddress (Uri(BaseUrl))
                method Method.Post
                path "agreements/enduser/"
                body requestBody
                requestHeaders headers
                responseHandler handler
            }

        headers
        |> AsyncResult.bind mk

    let createRequisition (login : LoginRequest) (requisition : RequisitionRequest) =
        let mkHeaders bearerToken  =
            let contentType =
                { ContentType.``type`` = "application"
                  subtype = "json"
                  charset = None
                  boundary = None }
            seq {
                yield "application/json" |> RequestHeader.Accept
                yield bearerToken |> RequestHeader.Authorization
                yield contentType |> RequestHeader.ContentType }

        let handler (response : HttpResponseMessage) =
            let processResponse : AsyncResult<EndUserAgreement, exn> =
                response
                |> toResult
            processResponse

        let headers =
            Credential.BearerToken login
            |> AsyncResult.map mkHeaders

        let requestBody =
            requisition
            |> toJson
            |> string
            |> RequestBody.BodyString

        let mk headers =
            httpResponse {
                baseAddress (Uri(BaseUrl))
                method Method.Post
                path "requisitions/"
                body requestBody
                requestHeaders headers
                responseHandler handler
            }

        headers
        |> AsyncResult.bind mk

    let requisition (login : LoginRequest) (requisitionId : Guid) =
        let mkHeaders bearerToken  =
            let contentType =
                { ContentType.``type`` = "application"
                  subtype = "json"
                  charset = None
                  boundary = None }
            seq {
                yield "application/json" |> RequestHeader.Accept
                yield bearerToken |> RequestHeader.Authorization
                yield contentType |> RequestHeader.ContentType }

        let handler (response : HttpResponseMessage) =
            let processResponse : AsyncResult<Requisition, exn> =
                response
                |> toResult
            processResponse

        let headers =
            Credential.BearerToken login
            |> AsyncResult.map mkHeaders

        let mk headers =
            httpResponse {
                baseAddress (Uri(BaseUrl))
                method Method.Post
                path $"requisitions/{requisitionId}"
                requestHeaders headers
                responseHandler handler
            }

        headers
        |> AsyncResult.bind mk