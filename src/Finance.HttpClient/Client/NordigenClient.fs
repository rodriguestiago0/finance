namespace Finance.HttpClient.Client

open System
open System.Net.Http
open Finance.FSharp
open Finance.HttpClient
open Finance.HttpClient.Http.Response
open Finance.HttpClient.Model.Request.Nordigen
open Finance.HttpClient.Model.Response.Nordigen
open Fleece.SystemTextJson
open Microsoft.FSharp.Core

[<RequireQualifiedAccess>]
module NordigenClient =
    let [<Literal>] private baseUrl = "https://ob.nordigen.com/api/v2/"

    let private nordigenUri = Uri(baseUrl)

    type Credential =
        { Access : string
          AccessExpiresAt : DateTimeOffset
          Refresh : string
          RefreshExpiresAt : DateTimeOffset }

    type RefreshCredential =
        { Access : string
          AccessExpiresAt : DateTimeOffset }

    let mutable private Authentication =
        { Credential.Access = String.Empty
          AccessExpiresAt =  DateTimeOffset.MinValue
          Refresh = String.Empty
          RefreshExpiresAt = DateTimeOffset.MinValue } |> AsyncResult.retn

    let private lockObj = obj()

    let (|IsValid|_|) (credential : Credential) =
        if credential.RefreshExpiresAt > DateTimeOffset.UtcNow then
            Some credential
        else None

    let (|ExpiredToken|_|) (credential : Credential) =
        if credential.AccessExpiresAt <= DateTimeOffset.UtcNow then
            Some()
        else None

    let (|ExpiredRefresh|_|) (credential : Credential) =
        if credential.RefreshExpiresAt <= DateTimeOffset.UtcNow then
            Some()
        else None

    let private contentType =
        { ContentType.``type`` = "application"
          subtype = "json"
          charset = None
          boundary = None }

    let private basicHeaders =
        seq {
            yield "application/json" |> RequestHeader.Accept
            yield contentType |> RequestHeader.ContentType }

    let inline private buildBody body =
        body
        |> toJson
        |> string
        |> RequestBody.BodyString

    let inline private handler (response : HttpResponseMessage) =
        response
        |> toResult

    let private loginAction (login : LoginRequest) =
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
            baseAddress nordigenUri
            method Method.Post
            path "token/new/"
            requestHeaders basicHeaders
            body (buildBody login)
            responseHandler handler }

    let private refreshAction (refresh : RefreshRequest) =
        let handler (response : HttpResponseMessage) =
            let processResponse : AsyncResult<RefreshAuthorization, exn> =
                response
                |> toResult
            processResponse
            |> AsyncResult.map(fun auth ->
                let accessExpiresAt = auth.AccessExpires - 60
                let now = DateTimeOffset.UtcNow
                { RefreshCredential.Access = auth.Access
                  AccessExpiresAt = now.AddSeconds(accessExpiresAt) })

        httpResponse {
            baseAddress nordigenUri
            method Method.Post
            path "token/refresh/"
            requestHeaders basicHeaders
            body (buildBody refresh)
            responseHandler handler }

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
                        refreshAction { RefreshRequest.Refresh = credential.Refresh }
                        |> AsyncResult.map(fun r ->
                            { credential with
                                Access = r.Access
                                AccessExpiresAt = r.AccessExpiresAt })
                    | _ -> "Invalid Credential" |> exn |> AsyncResult.error

                let currentAuthentication =
                    Authentication
                    |> AsyncResult.bind validateCredentials
                Authentication <- currentAuthentication
                Authentication
            lock lockObj getOrUpdateAuthentication
            |> AsyncResult.map(fun a -> $"Bearer {a.Access}")

    let private headersWithAuthentication login =
        let mkHeaders bearerToken  =
            seq {
                yield bearerToken |> RequestHeader.Authorization
                yield! basicHeaders }

        Credential.BearerToken login
        |> AsyncResult.map mkHeaders

    let getInstitutions (login : LoginRequest)  (country : Option<string>) : AsyncResult<Institution[], exn> =
        let query =
            match country with
            | Some s ->
                ("country", s) |> Seq.singleton
            | None -> Seq.empty

        let mk headers =
            httpResponse {
                baseAddress nordigenUri
                method Method.Get
                path "institutions/"
                queries query
                requestHeaders headers
                responseHandler handler }

        headersWithAuthentication login
        |> AsyncResult.bind mk

    let getInstitution (login : LoginRequest)  (institutionId : Guid) : AsyncResult<Institution, exn> =
        let mk headers =
            httpResponse {
                baseAddress nordigenUri
                method Method.Get
                path $"institutions/{institutionId}/"
                requestHeaders headers
                responseHandler handler }

        headersWithAuthentication login
        |> AsyncResult.bind mk

    let createAgreement (login : LoginRequest) (endUserAgreement : EndUserAgreementRequest) : AsyncResult<EndUserAgreement, exn> =
        let mk headers =
            httpResponse {
                baseAddress nordigenUri
                method Method.Post
                path "agreements/enduser/"
                body (buildBody endUserAgreement)
                requestHeaders headers
                responseHandler handler }

        headersWithAuthentication login
        |> AsyncResult.bind mk

    let getAgreements (login : LoginRequest): AsyncResult<EndUserAgreement[], exn> =
        let mk headers =
            httpResponse {
                baseAddress nordigenUri
                method Method.Get
                path "agreements/enduser/"
                requestHeaders headers
                responseHandler handler }

        headersWithAuthentication login
        |> AsyncResult.bind mk

    let getAgreement (login : LoginRequest) (agreementId : Guid) : AsyncResult<EndUserAgreement, exn> =
        let mk headers =
            httpResponse {
                baseAddress nordigenUri
                method Method.Get
                path $"agreements/enduser/{agreementId}/"
                requestHeaders headers
                responseHandler handler }

        headersWithAuthentication login
        |> AsyncResult.bind mk

    let deleteAgreement (login : LoginRequest) (agreementId : Guid) : AsyncResult<unit, exn> =
        let handler (response : HttpResponseMessage) =
            response
            |> toEmptyResult

        let mk headers =
            httpResponse {
                baseAddress nordigenUri
                method Method.Delete
                path $"agreements/enduser/{agreementId}/"
                requestHeaders headers
                responseHandler handler }

        headersWithAuthentication login
        |> AsyncResult.bind mk

    let createRequisition (login : LoginRequest) (requisition : RequisitionRequest) : AsyncResult<EndUserAgreement, exn> =
        let mk headers =
            httpResponse {
                baseAddress nordigenUri
                method Method.Post
                path "requisitions/"
                body (buildBody requisition)
                requestHeaders headers
                responseHandler handler }

        headersWithAuthentication login
        |> AsyncResult.bind mk

    let getRequisition (login : LoginRequest) (requisitionId : Guid) : AsyncResult<Requisition, exn> =
        let mk headers =
            httpResponse {
                baseAddress nordigenUri
                method Method.Get
                path $"requisitions/{requisitionId}/"
                requestHeaders headers
                responseHandler handler }

        headersWithAuthentication login
        |> AsyncResult.bind mk

    let deleteRequisition (login : LoginRequest) (requisitionId : Guid) : AsyncResult<unit, exn> =
        let handler (response : HttpResponseMessage) =
            response
            |> toEmptyResult

        let mk headers =
            httpResponse {
                baseAddress nordigenUri
                method Method.Delete
                path $"requisitions/{requisitionId}/"
                requestHeaders headers
                responseHandler handler }

        headersWithAuthentication login
        |> AsyncResult.bind mk

    let getRequisitions (login : LoginRequest) : AsyncResult<Requisition[], exn> =
        let mk headers =
            httpResponse {
                baseAddress nordigenUri
                method Method.Get
                path $"requisitions/"
                requestHeaders headers
                responseHandler handler }

        headersWithAuthentication login
        |> AsyncResult.bind mk