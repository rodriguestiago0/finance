namespace Finance.FSharp.Validation

open Finance.FSharp

type ValidationResult<'TSuccess, 'TFailure> =
    | Success of 'TSuccess
    | Failure of 'TFailure

[<RequireQualifiedAccess>]
module ValidationResult =
    let retn x = Success x

    let bind f x =
        match x with
        | Success r -> f r
        | Failure e -> Failure e

    let inline map f m = bind (f >> retn) m

    let mapError f res =
        match res with
        | Success x -> Success x
        | Failure e -> Failure <| f e

    let apply f v =
        match f, v with
        | Success f, Success x -> Success (f x)
        | Failure e, Success _ -> Failure e
        | Success _, Failure e -> Failure e
        | Failure e1, Failure e2 -> List.append e1 e2 |> Failure

    let inline toResult validationResult =
        match validationResult with
        | Success s -> Ok s
        | Failure f -> Error f
        |> Result.mapError ValidationException.ofErrors

    let inline ofResult (res: Result<_, exn>) =
        match res with
        | Ok s -> Success s
        | Error e -> Failure [ e.Message ]

    let traverse f xs =
        let folder x xs =
            bind (fun h ->
                    bind (fun t -> seq { yield h; yield! t } |> Success ) xs) (f x)
        Seq.foldBack folder xs (Success Seq.empty)

    let sequence xs = traverse id xs


[<AutoOpen>]
module Validation =
    type ValidationBuilder() =
        member _.Bind(x, f) =
            match x with
            | Failure e -> Failure e
            | Success a -> f a

        member _.MergeSources(left : ValidationResult<_, list<string>>, right : ValidationResult<_, list<string>>) =
            match left, right with
            | Success l, Success r -> Success (l, r)
            | Failure l, Success _ -> Failure l
            | Success _, Failure r -> Failure r
            | Failure r, Failure l -> List.append r l |> Failure

        member _.Return(x) =
            Success x

    let validator = ValidationBuilder()

[<RequireQualifiedAccess>]
module Validations =
    open System

    let inline validate errorMessage f v =
        if f v
        then Success v
        else Failure [ errorMessage ]

    let inline requireString propName =
        let errorMessage =
            sprintf "%s cannot be null, empty or whitespace." propName
        validate errorMessage (String.IsNullOrWhiteSpace >> not)

    let inline requireIntGreaterThan propName min =
        let errorMessage =
            sprintf "%s have to be greater or equal to '%d'." propName min
        validate errorMessage (flip (>) min)

    let inline requireEmail propName =
        let errorMessage =
            sprintf "%s is not a valid email address." propName
        let check (v: string) =
            try
                let _ = Net.Mail.MailAddress(v)
                true
            with ex ->
                false
        validate errorMessage check

    let inline requireGuid propName =
        validate (sprintf "%s is required" propName) (fun v -> v <> Guid.Empty)

    let inline requireObject propName =
        let check value = box value <> null
        validate (sprintf "%s is required" propName) check

    let inline requireWhenSome checkWhenSome =
        function
        | Some v ->
            checkWhenSome v
            |> ValidationResult.map Some
        | _ -> Success None

    let inline requireArrayValues check values =
        values
        |> Array.map check
        |> ValidationResult.sequence
        |> ValidationResult.map Seq.toArray

    let inline requireListValues check values =
        values
        |> List.map check
        |> ValidationResult.sequence
        |> ValidationResult.map Seq.toArray

    let inline requireAtLeastOne propName =
        let check values =
            match values with
            | NotEmptySeq _ -> true
            | _ -> false
        let errorMessage =
            sprintf "%s should have at least one element'." propName
        validate errorMessage check