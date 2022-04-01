namespace Finance.Model.Investment

type Username =
    Username of string
    with
        member this.Deconstruct() =
            let (Username username) = this
            username

type User =
    { Username : Username
      Password : string }