namespace Finance.Repository

open Microsoft.FSharp.Core
open Npgsql.FSharp
open Finance.FSharp
open Finance.Model
open Finance.Repository.Models

module UserRepository =
    type UserDto
    with
        static member ofRowReader (read : RowReader) =
            { UserDto.UserId = read.uuid "user_id"
              Username = read.string "username"
              Password = read.string "password" }

    let getUser connectionString (username : Username) : AsyncResult<User, exn> =
        connectionString
        |> Sql.connect
        |> Sql.query "SELECT * FROM finance.user"
        |> Sql.parameters [ "@username", Sql.string (deconstruct username)]
        |> Sql.executeRowAsync UserDto.ofRowReader
        |> AsyncResult.ofTask
        |> AsyncResult.map UserDto.toDomain
        |> AsyncResult.mapError handleExceptions