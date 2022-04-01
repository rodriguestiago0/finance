namespace Finance.Api.Models

open Finance.FSharp
open Finance.Model

type UserDto =
    { Username : string
      Password : string }
    with
        static member ofDomain (model : User) : UserDto =
            { UserDto.Username = deconstruct model.Username
              Password = model.Password }

        static member toDomain (dto : UserDto) : User =
            { User.Username = dto.Username |> Username
              Password = dto.Password }

