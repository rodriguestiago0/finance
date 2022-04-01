namespace Finance.Application.Authentication

open System.Text
open System.Security.Cryptography
open Finance.FSharp
open Finance.Model

[<RequireQualifiedAccess>]
module AuthenticationService =
    let authenticate (username : Username) (password : string) currentUser =
        use sha256Hash = SHA256.Create()

        let computedHash =
            let hash = StringBuilder();
            (hash,
                $"{deconstruct username}_{password}"
                |> encodingGetBytes
                |> sha256Hash.ComputeHash)
            ||> Seq.fold (fun state c->
                state.Append(c.ToString("X2")) |> ignore
                state)
            |> ignore
            hash.ToString()

        if currentUser.Password = computedHash then
            currentUser |> AsyncResult.retn
        else
            "Invalid user credentials" |> exn |> AsyncResult.error
