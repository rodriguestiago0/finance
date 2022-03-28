namespace Finance.FSharp.Tests

open NUnit.Framework
open Finance.FSharp
open Finance.Fsharp.Tests.Common

[<TestFixture>]
module Encryption =

    [<Test>]
    let ``Validate encrypt and decrypt`` () =
        let password = "password"
        let key = "(G+KaPdSgVkYp3s6v9y$B&E)H@McQeTh"
        let encryptedPassword = Encryption.encrypt key password
        let decryptedPassword = Encryption.decrypt key encryptedPassword
        Assert.IsTrue(isOk decryptedPassword)
        Assert.AreEqual(password, getOk decryptedPassword)