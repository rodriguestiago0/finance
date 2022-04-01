namespace Finance.Repository.Models

open System
open Finance.FSharp
open Finance.Model

type UserDto =
    { UserId : Guid
      Username : string
      Password : string }
    with
        static member ofDomain (model : User) : UserDto =
            { UserDto.UserId = deconstruct model.UserId
              Username = deconstruct model.Username
              Password = model.Password }

        static member toDomain (dto : UserDto): User =
            { User.UserId = dto.UserId |> UserId
              Username = dto.Username |> Username
              Password = dto.Password }