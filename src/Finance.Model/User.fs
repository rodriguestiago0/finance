namespace Finance.Model

open System

type Username =
    Username of string
    with
        member this.Deconstruct() =
            let (Username username) = this
            username

type UserId =
    UserId of Guid
    with
        member this.Deconstruct() =
            let (UserId id) = this
            id

type User =
    { UserId : UserId
      Username : Username
      Password : string }