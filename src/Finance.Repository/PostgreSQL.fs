namespace Finance.Repository

open Finance.FSharp
open Npgsql

[<AutoOpen>]
module PostgreSQL =
    let handleExceptions (exn : exn) =
        match exn.InnerException with
        | :? NoResultsException -> NotFoundException() :> exn
        | :? PostgresException -> ConflictException()
        | :? NpgsqlException -> BadRequestException()
        | _ -> BadRequestException exn.Message