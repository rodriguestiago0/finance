namespace Finance.FSharp

open System.IO
open System.Security.Cryptography

module Encryption =
    let private encryptAlgorithm key =
        let aes = Aes.Create ()
        aes.KeySize <- 256
        aes.Key <- (encodingGetBytes key)
        aes

    let private toString (cypher, iv) =
        String.concat "@" [cypher; iv]

    let private fromString (str : string) =
        match str.Split '@' with
        | [| cypher; iv |] -> (cypher, iv) |> Ok
        | _ -> "Incorrectly encoded cypher" |> exn |> Error

    let encrypt key plainText =
        let aes = encryptAlgorithm key
        let iv = toBase64String aes.IV
        let key =
            let bytes = encodingGetBytes plainText
            let encryptor = aes.CreateEncryptor ()
            use buffer = new MemoryStream ()
            use stream = new CryptoStream(buffer, encryptor, CryptoStreamMode.Write)
            stream.Write(bytes, 0, bytes.Length)
            stream.FlushFinalBlock ()
            toBase64String (buffer.ToArray ())
        (key, iv)
        |> toString

    let decrypt key cypher =
        use aes = encryptAlgorithm key
        let parsedCypher = fromString cypher

        let mk (cypher, iv) =
            aes.DecryptCbc(fromBase64String cypher, fromBase64String iv)
            |> encodingGetString
            
        parsedCypher
        |> Result.map mk



